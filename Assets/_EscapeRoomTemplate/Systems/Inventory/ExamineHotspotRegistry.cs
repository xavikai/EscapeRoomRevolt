using System;
using System.Collections.Generic;
using EscapeRoomRevolt.Core.Save;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Inventory
{
    [Serializable]
    public sealed class ExamineHotspotSaveState { public List<string> revealed = new List<string>(); }

    /// <summary>
    /// Persists which ExamineHotspot instances have already been revealed. A separate registry is
    /// needed because the examined 3D model (and any ExamineHotspot on it) is instantiated fresh
    /// each time an item is opened in the examiner and destroyed on close, so hotspots cannot hold
    /// their own save state.
    /// </summary>
    public sealed class ExamineHotspotRegistry : MonoBehaviour, ISaveable
    {
        public static ExamineHotspotRegistry Instance { get; private set; }
        private readonly HashSet<string> _revealed = new HashSet<string>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SaveManager.Instance?.Register(this);
        }

        private void OnDestroy()
        {
            SaveManager.Instance?.Unregister(this);
            if (Instance == this) Instance = null;
        }

        public bool IsRevealed(string key) => _revealed.Contains(key);
        public void MarkRevealed(string key) => _revealed.Add(key);

        public string SaveId => "ExamineHotspots";
        public string SaveData() => JsonUtility.ToJson(new ExamineHotspotSaveState { revealed = new List<string>(_revealed) });

        public void LoadData(string json)
        {
            ExamineHotspotSaveState state = JsonUtility.FromJson<ExamineHotspotSaveState>(json);
            _revealed.Clear();
            if (state?.revealed == null) return;
            foreach (string key in state.revealed) _revealed.Add(key);
        }
    }
}
