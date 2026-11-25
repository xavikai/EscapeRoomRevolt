using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EscapeRoomRevolt.Core.Settings;
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
        public int runSeed;
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

        /// <summary>
        /// Stable per-playthrough seed, persisted with the save. A fresh one is rolled on
        /// StartNewGame() (via ResetSession); loading a save restores the one it was played with, so
        /// anything seeded from it (e.g. a randomized puzzle code) stays the same across a reload
        /// instead of rerolling into a state that no longer matches an already-solved puzzle or a
        /// hint the player already read.
        /// </summary>
        public int RunSeed { get; private set; }

        /// <summary>
        /// Optional gate an OptionalGameFeature.PlayerVitals system (e.g. SurvivalDifficultyService)
        /// can register to forbid manual saving under the active difficulty. Core.Save has no
        /// compile-time dependency on Systems, so it exposes this extension point instead of
        /// referencing the concrete type directly; defaults to always-allowed.
        /// </summary>
        public static Func<bool> ManualSaveGate { get; set; } = () => true;

        /// <summary>
        /// Raised by ResetSession (New game, Retry). Systems that live across scenes
        /// (DontDestroyOnLoad) and keep progress of their own must clear it here: a fresh run must
        /// not inherit state from the previous one.
        /// </summary>
        public static event Action SessionReset;

        public event Action<string> SaveCompleted;
        public event Action<string> LoadCompleted;
        public event Action<string, string> OperationFailed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticEvents() => SessionReset = null;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            RunSeed = Guid.NewGuid().GetHashCode();
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
            if (input == null) return;
            // Quick save only while actually playing: F5 in the main menu, the intro, a loading
            // screen or the results screen used to write a "save" of that scene, and Continue then
            // opened the menu instead of the game.
            if (input.QuickSavePressed && CanSaveNow()) SaveGame();
            if (input.QuickLoadPressed && CanQuickLoadNow())
            {
                // Through the flow manager, so the game state and results are handled like the
                // Load menu does it.
                if (EscapeRoomRevolt.Core.Flow.GameFlowManager.Instance != null)
                    EscapeRoomRevolt.Core.Flow.GameFlowManager.Instance.LoadSlot(DefaultSlotId);
                else
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

        /// <summary>
        /// True while a game is in progress (playing or paused). Without a GameFlowManager (tests,
        /// a bare scene) saving is always allowed.
        /// </summary>
        public static bool CanSaveNow()
        {
            if (EscapeRoomRevolt.Core.Flow.GameFlowManager.Instance == null) return true;
            var state = EscapeRoomRevolt.Core.Flow.GameFlowManager.State;
            return state == EscapeRoomRevolt.Core.Flow.GameFlowState.Playing
                || state == EscapeRoomRevolt.Core.Flow.GameFlowState.Paused;
        }

        private static bool CanQuickLoadNow()
        {
            if (EscapeRoomRevolt.Core.Flow.GameFlowManager.Instance == null) return true;
            var state = EscapeRoomRevolt.Core.Flow.GameFlowManager.State;
            return state != EscapeRoomRevolt.Core.Flow.GameFlowState.Loading
                && state != EscapeRoomRevolt.Core.Flow.GameFlowState.Boot;
        }

        public void SaveGame(string slotId)
        {
            if (string.IsNullOrWhiteSpace(slotId)) slotId = DefaultSlotId;
            if (!CanSaveNow())
            {
                const string message = "Solo se puede guardar durante la partida.";
                Debug.LogWarning($"[SaveManager] {message}");
                OperationFailed?.Invoke(slotId, message);
                return;
            }
            if (GameFeatures.IsEnabled(OptionalGameFeature.PlayerVitals) && !ManualSaveGate())
            {
                const string message = "La dificultad Survival Horror activa no permite guardar manualmente.";
                Debug.LogWarning($"[SaveManager] {message}");
                OperationFailed?.Invoke(slotId, message);
                return;
            }

            SaveGameData data = CaptureSnapshot();
            data.slotId = SanitizeSlotId(slotId);
            data.thumbnailFile = $"{data.slotId}.png";

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

        /// <summary>
        /// Captures every registered ISaveable into an in-memory snapshot, without touching disk.
        /// Used by SaveGame (which adds slot metadata and writes it) and by systems that need to
        /// preserve state across a scene change without a real save file (e.g. room-to-room
        /// transitions via GameFlowManager.TransitionToRoom).
        /// </summary>
        public SaveGameData CaptureSnapshot()
        {
            CleanupSaveables();

            SaveGameData data = new SaveGameData
            {
                scenePath = SceneManager.GetActiveScene().path,
                sceneName = SceneManager.GetActiveScene().name,
                savedAtUtc = DateTime.UtcNow.ToString("O"),
                playTimeSeconds = _playTimeSeconds,
                runSeed = RunSeed
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
                    string state = saveable.SaveData();
                    data.keys.Add(saveable.SaveId);
                    data.values.Add(state);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveManager] Failed to save {saveable.SaveId}: {e.Message}");
                }
            }

            data.destroyedEntities.AddRange(_destroyedEntities);
            return data;
        }

        public void LoadGame()
        {
            LoadGame(DefaultSlotId);
        }

        public void LoadGame(string slotId)
        {
            if (string.IsNullOrWhiteSpace(slotId)) slotId = DefaultSlotId;
            string path = GetSlotPath(slotId);
            if (!File.Exists(path) && !File.Exists(path + ".bak"))
            {
                const string message = "No se ha encontrado la partida.";
                OperationFailed?.Invoke(slotId, message);
                Debug.LogWarning($"[SaveManager] {message}");
                return;
            }

            SaveGameData data = TryReadSlot(path);
            if (data == null)
            {
                const string message = "La partida guardada está dañada y no se pudo recuperar ni desde su copia de seguridad.";
                OperationFailed?.Invoke(slotId, message);
                Debug.LogError($"[SaveManager] {message}");
                return;
            }

            if (!string.IsNullOrEmpty(data.scenePath))
            {
                BeginSceneLoad(data);
                return;
            }

            RestoreSnapshot(data, slotId);
        }

        public bool HasSave(string slotId) => TryReadSlot(GetSlotPath(slotId)) != null;

        public List<SaveSlotMetadata> GetSlots()
        {
            string folder = GetSaveFolder();
            if (!Directory.Exists(folder)) return new List<SaveSlotMetadata>();

            return Directory.GetFiles(folder, "*.json")
                .Concat(Directory.GetFiles(folder, "*.json.bak").Select(path => path.Substring(0, path.Length - 4)))
                .Distinct()
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
            string backupPath = path + ".bak";
            if (File.Exists(backupPath)) File.Delete(backupPath);
            string thumbnailPath = GetThumbnailPath(slotId);
            if (File.Exists(thumbnailPath)) File.Delete(thumbnailPath);
        }

        /// <summary>Clears runtime-only progress before loading the first scene of a new game. Save files are untouched.</summary>
        public void ResetSession()
        {
            SceneManager.sceneLoaded -= OnSceneLoadedForRestore;
            _pendingLoad = null;
            _destroyedEntities.Clear();
            // Scene objects go away with their scene and register again when the next one starts.
            // Objects that survive scene loads (DontDestroyOnLoad registries) register only once, in
            // their Awake: dropping them here meant their state was never saved again for the rest
            // of the session. Keep them, and let them clear their own progress.
            _saveables.RemoveWhere(saveable => !IsAlive(saveable) || !IsPersistent(saveable));
            SessionReset?.Invoke();
            _playTimeSeconds = 0f;
            RunSeed = Guid.NewGuid().GetHashCode();
        }

        private void BeginSceneLoad(SaveGameData data)
        {
            if (!Application.CanStreamedLevelBeLoaded(data.scenePath))
            {
                const string unavailableMessage = "La escena guardada no está incluida en esta versión del juego.";
                OperationFailed?.Invoke(data.slotId, unavailableMessage);
                Debug.LogWarning($"[SaveManager] {unavailableMessage} Scene: {data.scenePath}");
                return;
            }
            _pendingLoad = data;
            RunSeed = data.runSeed;
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

            if (data != null) RestoreSnapshot(data, data.slotId);
        }

        /// <summary>
        /// Restores every ISaveable in the current scene from an in-memory snapshot, without
        /// touching disk. Used by LoadGame (after reading a slot from disk) and by systems that
        /// need to restore state after a scene change without a real save file (e.g. room-to-room
        /// transitions via GameFlowManager.TransitionToRoom).
        /// </summary>
        public void RestoreSnapshot(SaveGameData data, string slotId)
        {
            if (!IsValidSnapshot(data))
            {
                const string message = "La estructura de la partida guardada no es válida.";
                OperationFailed?.Invoke(slotId, message);
                Debug.LogWarning($"[SaveManager] {message}");
                return;
            }
            CleanupSaveables();
            RunSeed = data.runSeed;
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

        /// <summary>Reads a slot, falling back to its <c>.bak</c> copy (written by WriteSlot's File.Replace) if the primary file is missing or corrupted.</summary>
        private static SaveGameData TryReadSlot(string path)
        {
            SaveGameData data = TryReadSlotFile(path);
            if (data != null) return data;

            string backupPath = path + ".bak";
            if (!File.Exists(backupPath)) return null;

            data = TryReadSlotFile(backupPath);
            if (data != null) Debug.LogWarning($"[SaveManager] Slot '{Path.GetFileName(path)}' was corrupted; recovered from its backup.");
            return data;
        }

        private static SaveGameData TryReadSlotFile(string path)
        {
            if (!File.Exists(path)) return null;
            try
            {
                SaveGameData data = JsonUtility.FromJson<SaveGameData>(File.ReadAllText(path));
                return IsValidSnapshot(data) ? data : null;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[SaveManager] Could not read '{Path.GetFileName(path)}': {exception.Message}");
                return null;
            }
        }

        private static bool IsValidSnapshot(SaveGameData data)
        {
            if (data == null || data.version < 1 || data.version > 3 || data.keys == null ||
                data.values == null || data.destroyedEntities == null || data.keys.Count != data.values.Count)
                return false;
            var ids = new HashSet<string>();
            for (int i = 0; i < data.keys.Count; i++)
                if (string.IsNullOrWhiteSpace(data.keys[i]) || !ids.Add(data.keys[i]) || data.values[i] == null)
                    return false;
            return true;
        }

        private void CleanupSaveables() => _saveables.RemoveWhere(saveable => !IsAlive(saveable));

        private static bool IsPersistent(ISaveable saveable) =>
            saveable is Component component && component != null
            && component.gameObject.scene.buildIndex == -1
            && component.gameObject.scene.name == "DontDestroyOnLoad";

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
            string folder = Path.Combine(ProductConfiguration.PersistentDataDirectory, SaveFolderName);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return folder;
        }

        private string GetSlotPath(string slotId) => Path.Combine(GetSaveFolder(), $"{SanitizeSlotId(slotId)}.json");
        public string GetThumbnailPath(string slotId) => Path.Combine(GetSaveFolder(), $"{SanitizeSlotId(slotId)}.png");

        /// <summary>
        /// Writes the temp file first, then atomically swaps it into place. If the process dies at
        /// any point before the final swap, the previous save on disk is untouched — there is no
        /// window where the slot file is missing, unlike a Delete-then-Move sequence.
        /// </summary>
        private void WriteSlot(string slotId, string json)
        {
            string path = GetSlotPath(slotId);
            string temporaryPath = path + ".tmp";
            string backupPath = path + ".bak";
            File.WriteAllText(temporaryPath, json);

            if (File.Exists(path)) File.Replace(temporaryPath, path, backupPath);
            else File.Move(temporaryPath, path);
        }
    }
}
