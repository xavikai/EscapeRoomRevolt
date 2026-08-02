using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EscapeRoomRevolt.Core.Settings;
using EscapeRoomRevolt.Systems.Survival;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeRoomRevolt.Core.Save
{
    // The data structure saved to disk
    [Serializable]
    public class SaveGameData
    {
        public int version = 3;
        public string slotId;
        public string scenePath;
        public string sceneName;
        public string savedAtUtc;
        public float playTimeSeconds;
        public string thumbnailFile;
        public List<string> keys = new List<string>();
        public List<string> values = new List<string>();
        public List<string> destroyedEntities = new List<string>();
    }

    [Serializable]
    public class SaveSlotMetadata
    {
        public string slotId;
        public string scenePath;
        public string sceneName;
        public string savedAtUtc;
        public float playTimeSeconds;
        public string thumbnailFile;
        public int version;
    }

    /// <summary>
    /// Singleton that orchestrates the global save/load process.
    /// Saves to Application.persistentDataPath.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager _instance;
        public static SaveManager Instance
        {
            get
            {
                if (_instance == null && Application.isPlaying)
                    _instance = FindAnyObjectByType<SaveManager>();
                return _instance;
            }
        }

        private const string DefaultSlotId = "slot_0";
        private const string SaveFolderName = "SaveSlots";
        public const int ManualSlotCount = 3;

        // All active ISaveables in the scene
        private readonly HashSet<ISaveable> _saveables = new HashSet<ISaveable>();
        
        // A list of SaveIds that have been destroyed (e.g. PickableItems)
        private readonly HashSet<string> _destroyedEntities = new HashSet<string>();
        private float _playTimeSeconds;
        private SaveGameData _pendingLoad;

        public event Action<string> SaveCompleted;
        public event Action<string> LoadCompleted;
        public event Action<string, string> OperationFailed;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoadedForRestore;
            if (_instance == this) _instance = null;
        }

        private void Update()
        {
            if (Time.timeScale > 0f) _playTimeSeconds += Time.unscaledDeltaTime;

            EscapeRoomRevolt.Core.Input.InputRouter input = EscapeRoomRevolt.Core.Input.InputRouter.Instance;
            if (input != null && input.QuickSavePressed)
            {
                SaveGame();
            }
            if (input != null && input.QuickLoadPressed)
            {
                LoadGame();
            }
        }

        /// <summary>
        /// Registers a saveable entity. Must be called in Awake/Start.
        /// </summary>
        public void Register(ISaveable saveable)
        {
            if (!IsAlive(saveable) || string.IsNullOrWhiteSpace(saveable.SaveId)) return;
            if (_destroyedEntities.Contains(saveable.SaveId))
            {
                if (saveable is MonoBehaviour mb)
                {
                    Destroy(mb.gameObject);
                }
                return;
            }
            _saveables.Add(saveable);
        }

        /// <summary>
        /// Unregisters a saveable entity.
        /// </summary>
        public void Unregister(ISaveable saveable)
        {
            _saveables.Remove(saveable);
        }

        /// <summary>
        /// Marks an entity as permanently destroyed so it doesn't spawn again.
        /// </summary>
        public void MarkAsDestroyed(string saveId)
        {
            _destroyedEntities.Add(saveId);
        }

        /// <summary>
        /// Checks if an entity was permanently destroyed.
        /// </summary>
        public bool IsDestroyed(string saveId)
        {
            return _destroyedEntities.Contains(saveId);
        }

        public HashSet<string> CaptureDestroyedEntities() => new HashSet<string>(_destroyedEntities);

        public void RestoreDestroyedEntities(IEnumerable<string> destroyedEntities)
        {
            _destroyedEntities.Clear();
            if (destroyedEntities == null) return;
            foreach (string saveId in destroyedEntities)
                if (!string.IsNullOrWhiteSpace(saveId)) _destroyedEntities.Add(saveId);
        }

        public void SaveGame()
        {
            SaveGame(DefaultSlotId);
        }

        public void SaveGame(string slotId)
        {
            if (string.IsNullOrWhiteSpace(slotId)) slotId = DefaultSlotId;
            if (GameFeatures.IsEnabled(OptionalGameFeature.PlayerVitals) && !SurvivalDifficultyService.AllowsManualSaving)
            {
                const string message = "Manual saving is disabled by the active Survival Horror difficulty.";
                Debug.LogWarning($"[SaveManager] {message}");
                OperationFailed?.Invoke(slotId, message);
                return;
            }
            CleanupSaveables();

            SaveGameData data = new SaveGameData
            {
                slotId = SanitizeSlotId(slotId),
                scenePath = SceneManager.GetActiveScene().path,
                sceneName = SceneManager.GetActiveScene().name,
                savedAtUtc = DateTime.UtcNow.ToString("O"),
                playTimeSeconds = _playTimeSeconds,
                thumbnailFile = $"{SanitizeSlotId(slotId)}.png"
            };

            var usedIds = new HashSet<string>();

            foreach (var saveable in _saveables.Where(IsAlive).ToArray())
            {
                try
                {
                    if (!usedIds.Add(saveable.SaveId))
                    {
                        Debug.LogWarning($"[SaveManager] Duplicate SaveId ignored: '{saveable.SaveId}'.");
                        continue;
                    }
                    data.keys.Add(saveable.SaveId);
                    data.values.Add(saveable.SaveData());
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveManager] Failed to save {saveable.SaveId}: {e.Message}");
                }
            }

            data.destroyedEntities.AddRange(_destroyedEntities);

            try
            {
                WriteSlot(data.slotId, JsonUtility.ToJson(data, true));
                if (Application.isPlaying) ScreenCapture.CaptureScreenshot(GetThumbnailPath(data.slotId));
            }
            catch (Exception exception)
            {
                string message = $"No se pudo guardar: {exception.Message}";
                OperationFailed?.Invoke(data.slotId, message);
                Debug.LogError($"[SaveManager] {message}");
                return;
            }

            EscapeRoomRevolt.Core.EventBus.Publish(new EscapeRoomRevolt.Core.OnGameSaved { slotId = data.slotId });
            SaveCompleted?.Invoke(data.slotId);
            Debug.Log($"[SaveManager] Game saved to slot '{data.slotId}'.");
        }

        public void LoadGame()
        {
            LoadGame(DefaultSlotId);
        }

        public void LoadGame(string slotId)
        {
            if (string.IsNullOrWhiteSpace(slotId)) slotId = DefaultSlotId;
            string path = GetSlotPath(slotId);
            if (!File.Exists(path))
            {
                const string message = "No se ha encontrado la partida.";
                OperationFailed?.Invoke(slotId, message);
                Debug.LogWarning($"[SaveManager] {message}");
                return;
            }

            SaveGameData data;
            try
            {
                data = JsonUtility.FromJson<SaveGameData>(File.ReadAllText(path));
            }
            catch (Exception exception)
            {
                OperationFailed?.Invoke(slotId, exception.Message);
                Debug.LogError($"[SaveManager] Failed to read slot '{slotId}': {exception.Message}");
                return;
            }

            if (data == null)
            {
                Debug.LogError("[SaveManager] Failed to parse save file.");
                return;
            }

            if (!string.IsNullOrEmpty(data.scenePath))
            {
                BeginSceneLoad(data);
                return;
            }

            Restore(data, slotId);
        }

        public bool HasSave(string slotId) => File.Exists(GetSlotPath(slotId));

        public List<SaveSlotMetadata> GetSlots()
        {
            string folder = GetSaveFolder();
            if (!Directory.Exists(folder)) return new List<SaveSlotMetadata>();

            return Directory.GetFiles(folder, "*.json")
                .Select(TryReadSlot)
                .Where(data => data != null)
                .Select(data => new SaveSlotMetadata
                {
                    slotId = data.slotId,
                    scenePath = data.scenePath,
                    sceneName = string.IsNullOrEmpty(data.sceneName) ? Path.GetFileNameWithoutExtension(data.scenePath) : data.sceneName,
                    savedAtUtc = data.savedAtUtc,
                    playTimeSeconds = data.playTimeSeconds,
                    thumbnailFile = data.thumbnailFile,
                    version = data.version
                })
                .OrderByDescending(slot => slot.savedAtUtc)
                .ToList();
        }

        public SaveSlotMetadata GetSlot(string slotId)
        {
            string sanitized = SanitizeSlotId(slotId);
            return GetSlots().FirstOrDefault(slot => slot.slotId == sanitized);
        }

        public static string GetManualSlotId(int index) => $"slot_{Mathf.Clamp(index, 1, ManualSlotCount)}";

        public void DeleteSlot(string slotId)
        {
            string path = GetSlotPath(slotId);
            if (File.Exists(path)) File.Delete(path);
            string thumbnailPath = GetThumbnailPath(slotId);
            if (File.Exists(thumbnailPath)) File.Delete(thumbnailPath);
        }

        /// <summary>Clears runtime-only progress before loading the first scene of a new game. Save files are untouched.</summary>
        public void ResetSession()
        {
            SceneManager.sceneLoaded -= OnSceneLoadedForRestore;
            _pendingLoad = null;
            _destroyedEntities.Clear();
            _saveables.Clear();
            _playTimeSeconds = 0f;
        }

        private void BeginSceneLoad(SaveGameData data)
        {
            _pendingLoad = data;
            _destroyedEntities.Clear();
            foreach (string destroyed in data.destroyedEntities) _destroyedEntities.Add(destroyed);
            _saveables.Clear();

            SceneManager.sceneLoaded -= OnSceneLoadedForRestore;
            SceneManager.sceneLoaded += OnSceneLoadedForRestore;
            AsyncOperation operation = SceneManager.LoadSceneAsync(data.scenePath);
            if (operation != null) return;

            SceneManager.sceneLoaded -= OnSceneLoadedForRestore;
            _pendingLoad = null;
            const string message = "Unity no pudo iniciar la carga de la escena guardada.";
            OperationFailed?.Invoke(data.slotId, message);
            Debug.LogError($"[SaveManager] {message} Scene: {data.scenePath}");
        }

        private void OnSceneLoadedForRestore(Scene scene, LoadSceneMode mode)
        {
            if (_pendingLoad == null) return;
            SceneManager.sceneLoaded -= OnSceneLoadedForRestore;
            SaveGameData data = _pendingLoad;
            _pendingLoad = null;

            // sceneLoaded runs after Awake/OnEnable but before Start. Discovering the scene
            // explicitly makes restore deterministic and avoids registration-order races.
            foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))
                if (behaviour is ISaveable saveable) Register(saveable);

            if (data != null) Restore(data, data.slotId);
        }

        private void Restore(SaveGameData data, string slotId)
        {
            CleanupSaveables();
            _destroyedEntities.Clear();
            foreach (var destroyed in data.destroyedEntities)
            {
                _destroyedEntities.Add(destroyed);
            }

            Dictionary<string, string> savedStates = new Dictionary<string, string>();
            for (int i = 0; i < data.keys.Count; i++)
            {
                savedStates[data.keys[i]] = data.values[i];
            }

            foreach (var saveable in _saveables.Where(IsAlive).ToArray())
            {
                // First check if it should be destroyed
                if (_destroyedEntities.Contains(saveable.SaveId))
                {
                    // If it's a MonoBehaviour, destroy it
                    if (saveable is MonoBehaviour mb)
                    {
                        Destroy(mb.gameObject);
                    }
                    continue;
                }

                if (savedStates.TryGetValue(saveable.SaveId, out string stateJson))
                {
                    try
                    {
                        saveable.LoadData(stateJson);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[SaveManager] Failed to load {saveable.SaveId}: {e.Message}");
                    }
                }
            }

            EscapeRoomRevolt.Core.EventBus.Publish(new EscapeRoomRevolt.Core.OnGameLoaded { slotId = slotId });
            _playTimeSeconds = Mathf.Max(0f, data.playTimeSeconds);
            LoadCompleted?.Invoke(slotId);
            Debug.Log($"[SaveManager] Loaded slot '{slotId}'.");
        }

        private static SaveGameData TryReadSlot(string path)
        {
            try { return JsonUtility.FromJson<SaveGameData>(File.ReadAllText(path)); }
            catch (Exception exception)
            {
                Debug.LogWarning($"[SaveManager] Ignoring invalid slot '{Path.GetFileName(path)}': {exception.Message}");
                return null;
            }
        }

        private void CleanupSaveables() => _saveables.RemoveWhere(saveable => !IsAlive(saveable));

        private static bool IsAlive(ISaveable saveable)
        {
            if (saveable == null) return false;
            return !(saveable is UnityEngine.Object unityObject) || unityObject != null;
        }

        private static string SanitizeSlotId(string slotId)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars()) slotId = slotId.Replace(invalid, '_');
            return slotId;
        }

        private string GetSaveFolder()
        {
            string folder = Path.Combine(Application.persistentDataPath, SaveFolderName);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return folder;
        }

        private string GetSlotPath(string slotId) => Path.Combine(GetSaveFolder(), $"{SanitizeSlotId(slotId)}.json");
        public string GetThumbnailPath(string slotId) => Path.Combine(GetSaveFolder(), $"{SanitizeSlotId(slotId)}.png");

        private void WriteSlot(string slotId, string json)
        {
            string path = GetSlotPath(slotId);
            string temporaryPath = path + ".tmp";
            File.WriteAllText(temporaryPath, json);
            if (File.Exists(path)) File.Delete(path);
            File.Move(temporaryPath, path);
        }
    }
}
