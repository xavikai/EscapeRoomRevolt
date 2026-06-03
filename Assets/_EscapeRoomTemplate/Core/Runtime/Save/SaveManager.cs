using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EscapeRoomRevolt.Core.Save
{
    // The data structure saved to disk
    [Serializable]
    public class SaveGameData
    {
        public List<string> keys = new List<string>();
        public List<string> values = new List<string>();
        public List<string> destroyedEntities = new List<string>();
    }

    /// <summary>
    /// Singleton that orchestrates the global save/load process.
    /// Saves to Application.persistentDataPath.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string SAVE_FILE_NAME = "savegame.json";

        // All active ISaveables in the scene
        private readonly HashSet<ISaveable> _saveables = new HashSet<ISaveable>();
        
        // A list of SaveIds that have been destroyed (e.g. PickableItems)
        private readonly HashSet<string> _destroyedEntities = new HashSet<string>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            // For now, hardcode F5 and F9 if we have no main menu
            if (Input.GetKeyDown(KeyCode.F5))
            {
                SaveGame();
            }
            if (Input.GetKeyDown(KeyCode.F9))
            {
                LoadGame();
            }
        }

        /// <summary>
        /// Registers a saveable entity. Must be called in Awake/Start.
        /// </summary>
        public void Register(ISaveable saveable)
        {
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

        public void SaveGame()
        {
            SaveGameData data = new SaveGameData();

            foreach (var saveable in new List<ISaveable>(_saveables))
            {
                try
                {
                    data.keys.Add(saveable.SaveId);
                    data.values.Add(saveable.SaveData());
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveManager] Failed to save {saveable.SaveId}: {e.Message}");
                }
            }

            data.destroyedEntities.AddRange(_destroyedEntities);

            string json = JsonUtility.ToJson(data, true);
            string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            File.WriteAllText(path, json);

            Debug.Log($"[SaveManager] Game saved to {path}");
        }

        public void LoadGame()
        {
            string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            if (!File.Exists(path))
            {
                Debug.LogWarning("[SaveManager] No save file found.");
                return;
            }

            string json = File.ReadAllText(path);
            SaveGameData data = JsonUtility.FromJson<SaveGameData>(json);

            if (data == null)
            {
                Debug.LogError("[SaveManager] Failed to parse save file.");
                return;
            }

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

            foreach (var saveable in new List<ISaveable>(_saveables))
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

            Debug.Log("[SaveManager] Game loaded successfully.");
        }
    }
}
