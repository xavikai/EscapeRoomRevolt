using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    [Serializable]
    public sealed class PuzzleStage
    {
        [Tooltip("Unique within this puzzle. Used by AdvanceToStage(string) for branches and by save/load.")]
        public string id = "stage";
        [Tooltip("Marks this as a solving endpoint: reaching it calls Solve() automatically. Leave false for dead-end/reset branches or intermediate stages.")]
        public bool isSolvedStage;
        [Tooltip("Fired whenever this stage becomes current during normal play (never replayed on load).")]
        public UnityEvent onEnter;
        [Tooltip("Fired when leaving this stage, forward or via rollback (never replayed on load).")]
        public UnityEvent onExit;
    }

    /// <summary>
    /// Ordered-or-branching puzzle built from a list of PuzzleStage entries, each with its own
    /// enter/exit feedback. Callers (levers, code panels, dialogue choices, anything wired to a
    /// UnityEvent) drive progress by calling AdvanceStage() for the next stage in list order, or
    /// AdvanceToStage(id) to jump to any stage by id — that jump is the branching mechanism.
    /// Reaching a stage with isSolvedStage calls the base Solve() automatically. Rollback and
    /// Save/Load from any stage are both supported.
    /// </summary>
    public class MultiStagePuzzle : PuzzleController
    {
        [Header("Stages")]
        [SerializeField] private List<PuzzleStage> _stages = new List<PuzzleStage>();
        [Tooltip("If enabled, RollbackStage() returns to the previous stage in this playthrough's history.")]
        [SerializeField] private bool _allowRollback = true;

        private readonly List<int> _history = new List<int>();
        private int _currentStageIndex = -1;

        public int CurrentStageIndex => _currentStageIndex;
        public PuzzleStage CurrentStage => _currentStageIndex >= 0 && _currentStageIndex < _stages.Count ? _stages[_currentStageIndex] : null;
        public int StageCount => _stages.Count;

        protected override void Awake()
        {
            base.Awake();
            if (_currentStageIndex < 0 && _stages.Count > 0) ApplyStage(0, fireEvents: false, recordHistory: true);
        }

        /// <summary>Advances to the next stage in list order. No-ops once solved or already on the last stage.</summary>
        public void AdvanceStage()
        {
            if (IsSolved || _currentStageIndex + 1 >= _stages.Count) return;
            AdvanceToStage(_currentStageIndex + 1);
        }

        /// <summary>Jumps to a specific stage by id — the mechanism for branches.</summary>
        public void AdvanceToStage(string stageId)
        {
            int index = _stages.FindIndex(stage => stage.id == stageId);
            if (index < 0)
            {
                Debug.LogWarning($"[MultiStagePuzzle:{DisplayName}] Unknown stage id '{stageId}'.", this);
                return;
            }
            AdvanceToStage(index);
        }

        public void AdvanceToStage(int stageIndex)
        {
            if (IsSolved || stageIndex < 0 || stageIndex >= _stages.Count) return;
            SetInProgress();
            ApplyStage(stageIndex, fireEvents: true, recordHistory: true);
        }

        /// <summary>Returns to the previous stage in this playthrough's history, if rollback is allowed and there is one.</summary>
        public bool RollbackStage()
        {
            if (!_allowRollback || IsSolved || _history.Count < 2) return false;
            _history.RemoveAt(_history.Count - 1);
            int previous = _history[_history.Count - 1];
            _history.RemoveAt(_history.Count - 1);
            ApplyStage(previous, fireEvents: true, recordHistory: true);
            return true;
        }

        protected override void OnPuzzleReset()
        {
            _history.Clear();
            if (_stages.Count > 0) ApplyStage(0, fireEvents: false, recordHistory: true);
        }

        private void ApplyStage(int index, bool fireEvents, bool recordHistory)
        {
            if (fireEvents) CurrentStage?.onExit?.Invoke();
            _currentStageIndex = index;
            if (recordHistory) _history.Add(index);
            PuzzleStage stage = CurrentStage;
            if (fireEvents) stage?.onEnter?.Invoke();
            if (stage != null && stage.isSolvedStage) Solve();
        }

        [Serializable]
        private sealed class MultiStageSaveData
        {
            public int stateIndex;
            public int stageIndex;
        }

        public override string SaveData()
        {
            return JsonUtility.ToJson(new MultiStageSaveData { stateIndex = (int)State, stageIndex = _currentStageIndex });
        }

        public override void LoadData(string json)
        {
            base.LoadData(json);
            MultiStageSaveData data = JsonUtility.FromJson<MultiStageSaveData>(json);
            if (data == null || _stages.Count == 0) return;
            int restoredIndex = Mathf.Clamp(data.stageIndex, 0, _stages.Count - 1);
            _history.Clear();
            ApplyStage(restoredIndex, fireEvents: false, recordHistory: true);
        }
    }
}
