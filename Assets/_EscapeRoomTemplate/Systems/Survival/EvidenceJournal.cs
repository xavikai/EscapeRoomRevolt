using System;
using System.Collections.Generic;
using System.Linq;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Core.Save;
using EscapeRoomRevolt.Core.Settings;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    [Serializable]
    public sealed class EvidenceJournalSaveState
    {
        public int version = 1;
        public List<string> recordedIds = new();
    }

    /// <summary>Persistent source of truth for all evidence recorded in the current game.</summary>
    [DefaultExecutionOrder(-45)]
    public sealed class EvidenceJournal : MonoBehaviour, ISaveable
    {
        public static EvidenceJournal Instance { get; private set; }

        private readonly HashSet<string> _recordedIds = new(StringComparer.Ordinal);
        private readonly Dictionary<string, EvidenceDefinition> _definitions = new(StringComparer.Ordinal);

        public IReadOnlyCollection<string> RecordedIds => _recordedIds;
        public IEnumerable<EvidenceDefinition> RecordedEvidence => _recordedIds
            .Select(id => _definitions.TryGetValue(id, out EvidenceDefinition definition) ? definition : null)
            .Where(definition => definition != null);

        public event Action<EvidenceDefinition> EvidenceRecorded;
        public event Action JournalChanged;

        private void Awake()
        {
            if (!GameFeatures.IsEnabled(OptionalGameFeature.EvidenceRecording)) { enabled = false; return; }
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

        public static EvidenceJournal EnsureInstance()
        {
            if (Instance != null) return Instance;
            return new GameObject("EvidenceJournal").AddComponent<EvidenceJournal>();
        }

        public void RegisterDefinition(EvidenceDefinition definition)
        {
            if (definition != null) _definitions[definition.EvidenceId] = definition;
        }

        public bool IsRecorded(string evidenceId) => !string.IsNullOrWhiteSpace(evidenceId) && _recordedIds.Contains(evidenceId);

        public bool Record(EvidenceDefinition definition)
        {
            if (definition == null) return false;
            RegisterDefinition(definition);
            if (!_recordedIds.Add(definition.EvidenceId)) return false;
            EvidenceRecorded?.Invoke(definition);
            JournalChanged?.Invoke();
            EventBus.Publish(new OnEvidenceRecorded { evidenceId = definition.EvidenceId, title = definition.Title });
            return true;
        }

        public void ClearJournal()
        {
            if (_recordedIds.Count == 0) return;
            _recordedIds.Clear();
            JournalChanged?.Invoke();
        }

        public string SaveId => "EvidenceJournal";

        public string SaveData() => JsonUtility.ToJson(new EvidenceJournalSaveState
        {
            recordedIds = _recordedIds.OrderBy(id => id).ToList()
        });

        public void LoadData(string json)
        {
            EvidenceJournalSaveState state = JsonUtility.FromJson<EvidenceJournalSaveState>(json);
            _recordedIds.Clear();
            if (state?.recordedIds != null)
                foreach (string id in state.recordedIds.Where(id => !string.IsNullOrWhiteSpace(id))) _recordedIds.Add(id);
            JournalChanged?.Invoke();
        }
    }
}
