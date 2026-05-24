using System.Collections.Generic;
using System.IO;
using UnityEngine;
using EscapeRoomRevolt.Core;

namespace EscapeRoomRevolt.Systems.SaveLoad
{
    /// <summary>
    /// Core system for saving and loading the game state.
    /// Place one instance in the scene or manage it via Bootstrapper.
    ///
    /// Publishes: OnGameSaved, OnGameLoaded
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool _logActions = true;
        [SerializeField] private string _currentSlot = "Slot1";

        private List<ISaveable> _saveables = new List<ISaveable>();
        private SaveData _currentData = new SaveData();

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, $"{_currentSlot}.json");

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Find all saveables in the scene automatically
            var foundSaveables = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude);
            foreach (var mono in foundSaveables)
            {
                if (mono is ISaveable saveable)
                {
                    RegisterSaveable(saveable);
                }
            }
        }

        public void RegisterSaveable(ISaveable saveable)
        {
            if (!_saveables.Contains(saveable))
            {
                _saveables.Add(saveable);
            }
        }

        public void UnregisterSaveable(ISaveable saveable)
        {
            _saveables.Remove(saveable);
        }

        /// <summary>Saves the current state of all registered ISaveable entities.</summary>
        public void SaveGame()
        {
            _currentData.slotId = _currentSlot;
            _currentData.lastSavedTimeUnix = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            // Optional: update playTimeSeconds here

            foreach (var saveable in _saveables)
            {
                string id = saveable.SaveId;
                if (string.IsNullOrEmpty(id)) continue;

                object state = saveable.SaveState();
                string stateJson = JsonUtility.ToJson(state); // Serialize the sub-state
                _currentData.SetState(id, stateJson);
            }

            string fullJson = JsonUtility.ToJson(_currentData, true);
            File.WriteAllText(SaveFilePath, fullJson);

            Log($"Game saved to {SaveFilePath}");
            EventBus.Publish(new OnGameSaved { slotId = _currentSlot });
        }

        /// <summary>Loads the state from disk and applies it to all registered ISaveable entities.</summary>
        public void LoadGame()
        {
            if (!File.Exists(SaveFilePath))
            {
                Log($"No save file found at {SaveFilePath}");
                return;
            }

            string fullJson = File.ReadAllText(SaveFilePath);
            _currentData = JsonUtility.FromJson<SaveData>(fullJson);

            foreach (var saveable in _saveables)
            {
                string id = saveable.SaveId;
                if (string.IsNullOrEmpty(id)) continue;

                string stateJson = _currentData.GetState(id);
                if (!string.IsNullOrEmpty(stateJson))
                {
                    saveable.LoadState(stateJson);
                }
            }

            Log($"Game loaded from {SaveFilePath}");
            EventBus.Publish(new OnGameLoaded { slotId = _currentSlot });
        }

        private void Log(string msg)
        {
            if (_logActions) Debug.Log($"[SaveManager] {msg}");
        }
    }
}
