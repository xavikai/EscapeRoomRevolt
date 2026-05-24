using System;
using System.Collections.Generic;

namespace EscapeRoomRevolt.Systems.SaveLoad
{
    /// <summary>
    /// Represents the entire saved state of the game, serializable to JSON.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        // Metadata
        public string slotId;
        public long lastSavedTimeUnix;
        public float playTimeSeconds;

        // The actual game state, stored by SaveId
        // Dictionary is not directly serialized by Unity's JsonUtility, 
        // so we use parallel lists or a custom dictionary wrapper if using JsonUtility.
        // For simplicity with JsonUtility, we use lists of key-value pairs.
        
        public List<SaveKVP> entityStates = new List<SaveKVP>();

        [Serializable]
        public class SaveKVP
        {
            public string key;
            public string valueJson; // We store the sub-state as a nested JSON string
        }

        /// <summary>Retrieves a state string by ID.</summary>
        public string GetState(string id)
        {
            var kvp = entityStates.Find(x => x.key == id);
            return kvp?.valueJson;
        }

        /// <summary>Sets a state string by ID.</summary>
        public void SetState(string id, string stateJson)
        {
            var kvp = entityStates.Find(x => x.key == id);
            if (kvp != null)
            {
                kvp.valueJson = stateJson;
            }
            else
            {
                entityStates.Add(new SaveKVP { key = id, valueJson = stateJson });
            }
        }
    }
}
