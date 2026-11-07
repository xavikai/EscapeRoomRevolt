using System;
using System.Collections.Generic;
using System.Linq;
using EscapeRoomRevolt.Core.Save;
using UnityEngine;

namespace EscapeRoomRevolt.Core.Flow
{
    [Serializable]
    internal sealed class ObjectiveSaveState
    {
        public List<string> completedIds = new List<string>();
    }

    /// <summary>Evaluates a data-driven objective set and exposes a small, stable API for custom puzzles.</summary>
    public sealed class ObjectiveManager : MonoBehaviour, ISaveable
    {
        public static ObjectiveManager Instance { get; private set; }

        [SerializeField] private string _saveId = "ObjectiveManager";
        [SerializeField] private ObjectiveSet _objectiveSet;
        private readonly HashSet<string> _completed = new HashSet<string>(StringComparer.Ordinal);
        private float _startedAt;

        public string SaveId => string.IsNullOrWhiteSpace(_saveId) ? "ObjectiveManager" : _saveId;
        public ObjectiveSet Definition => _objectiveSet;
        public event Action<ObjectiveDefinition> ObjectiveCompleted;
        public event Action ObjectivesChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[Objectives] More than one ObjectiveManager exists in the scene. The newest one was disabled.", this);
                enabled = false;
                return;
            }
            Instance = this;
            _startedAt = Time.unscaledTime;
            SaveManager.Instance?.Register(this);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<OnPuzzleSolved>(HandlePuzzleSolved);
            EventBus.Subscribe<OnItemPickedUp>(HandleItemCollected);
            EventBus.Subscribe<OnNoteRead>(HandleNoteRead);
            EventBus.Subscribe<OnInteractionPerformed>(HandleInteraction);
            EventBus.Subscribe<OnEvidenceRecorded>(HandleEvidenceRecorded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<OnPuzzleSolved>(HandlePuzzleSolved);
            EventBus.Unsubscribe<OnItemPickedUp>(HandleItemCollected);
            EventBus.Unsubscribe<OnNoteRead>(HandleNoteRead);
            EventBus.Unsubscribe<OnInteractionPerformed>(HandleInteraction);
            EventBus.Unsubscribe<OnEvidenceRecorded>(HandleEvidenceRecorded);
        }

        private void OnDestroy()
        {
            SaveManager.Instance?.Unregister(this);
            if (Instance == this) Instance = null;
        }

        public bool IsComplete(string objectiveId) => !string.IsNullOrWhiteSpace(objectiveId) && _completed.Contains(objectiveId);

        public bool IsAvailable(ObjectiveDefinition objective)
        {
            if (objective == null) return false;
            return objective.Prerequisites.All(prerequisite => prerequisite != null && IsComplete(prerequisite.ObjectiveId));
        }

        public bool CompleteObjective(string objectiveId)
        {
            ObjectiveDefinition objective = FindObjective(objectiveId);
            if (objective == null || !IsAvailable(objective) || !_completed.Add(objective.ObjectiveId)) return false;

            ObjectiveCompleted?.Invoke(objective);
            ObjectivesChanged?.Invoke();
            EventBus.Publish(new OnObjectiveCompleted { objectiveId = objective.ObjectiveId });
            TryFinishRoom();
            return true;
        }

        public IReadOnlyList<ObjectiveDefinition> GetVisibleObjectives()
        {
            if (_objectiveSet == null) return Array.Empty<ObjectiveDefinition>();
            return _objectiveSet.Objectives
                .Where(objective => objective != null && (!objective.HiddenUntilAvailable || IsAvailable(objective)))
                .ToArray();
        }

        public string SaveData()
        {
            var state = new ObjectiveSaveState { completedIds = _completed.OrderBy(id => id).ToList() };
            return JsonUtility.ToJson(state);
        }

        public void LoadData(string json)
        {
            ObjectiveSaveState state = JsonUtility.FromJson<ObjectiveSaveState>(json);
            _completed.Clear();
            if (state?.completedIds != null)
                foreach (string id in state.completedIds.Where(id => !string.IsNullOrWhiteSpace(id))) _completed.Add(id);
            ObjectivesChanged?.Invoke();
        }

        private ObjectiveDefinition FindObjective(string objectiveId)
        {
            if (_objectiveSet == null || string.IsNullOrWhiteSpace(objectiveId)) return null;
            return _objectiveSet.Objectives.FirstOrDefault(objective => objective != null && objective.ObjectiveId == objectiveId);
        }

        private void CompleteMatching(ObjectiveTrigger trigger, string targetId)
        {
            if (_objectiveSet == null) return;
            foreach (ObjectiveDefinition objective in _objectiveSet.Objectives)
                if (objective != null && objective.Trigger == trigger
                    && string.Equals(objective.TargetId, targetId, StringComparison.Ordinal))
                    CompleteObjective(objective.ObjectiveId);
        }

        private void TryFinishRoom()
        {
            if (_objectiveSet == null || _objectiveSet.Objectives.Count == 0) return;
            if (_objectiveSet.Objectives.Any(objective => objective != null && !_completed.Contains(objective.ObjectiveId))) return;

            EventBus.Publish(new OnRoomEscaped
            {
                roomId = _objectiveSet.RoomId,
                completionTimeSeconds = Mathf.Max(0f, Time.unscaledTime - _startedAt)
            });

            if (!string.IsNullOrWhiteSpace(_objectiveSet.NextRoomScene))
            {
                GameFlowManager.EnsureInstance().TransitionToRoom(_objectiveSet.NextRoomScene, _objectiveSet.NextRoomSpawnId, RoomLoadMode.Single);
                return;
            }

            GameFlowManager.EnsureInstance().CompleteGame(_objectiveSet.CompletionEnding);
        }

        private void HandlePuzzleSolved(OnPuzzleSolved evt) => CompleteMatching(ObjectiveTrigger.PuzzleSolved, evt.puzzleId);
        private void HandleItemCollected(OnItemPickedUp evt) => CompleteMatching(ObjectiveTrigger.ItemCollected, evt.itemId);
        private void HandleNoteRead(OnNoteRead evt) => CompleteMatching(ObjectiveTrigger.NoteRead, evt.noteId);
        private void HandleInteraction(OnInteractionPerformed evt) => CompleteMatching(ObjectiveTrigger.InteractionPerformed, evt.interactableId);
        private void HandleEvidenceRecorded(OnEvidenceRecorded evt) => CompleteMatching(ObjectiveTrigger.EvidenceRecorded, evt.evidenceId);
    }
}
