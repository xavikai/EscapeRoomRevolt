using System.Collections.Generic;
using UnityEngine;
using EscapeRoomRevolt.Systems.Interaction;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    [System.Serializable]
    public class StateCondition
    {
        public SteppedPositioner Positioner;
        [Tooltip("Which position index (0-based, in the Positioner's own position list) this object must be showing to solve the puzzle.")]
        public int RequiredIndex;
    }

    /// <summary>
    /// A puzzle that requires multiple stepped objects (levers, switches, dials — anything with N
    /// discrete positions) to each be showing a specific position. Order does not matter.
    /// </summary>
    public class StatePuzzle : PuzzleController
    {
        [Header("State Settings")]
        [Tooltip("The required position index for each stepped object to solve the puzzle.")]
        [SerializeField] private List<StateCondition> _conditions = new List<StateCondition>();

        private void OnEnable()
        {
            foreach (var condition in _conditions)
            {
                if (condition != null && condition.Positioner != null)
                {
                    condition.Positioner.OnPositionChanged.AddListener(OnPositionChanged);
                }
            }
        }

        private void OnDisable()
        {
            foreach (var condition in _conditions)
            {
                if (condition != null && condition.Positioner != null)
                {
                    condition.Positioner.OnPositionChanged.RemoveListener(OnPositionChanged);
                }
            }
        }

        private void OnPositionChanged(int newIndex)
        {
            if (IsSolved) return;

            SetInProgress();
            CheckStates();
        }

        private void CheckStates()
        {
            if (_conditions.Count == 0) return;
            foreach (var condition in _conditions)
            {
                if (condition == null || condition.Positioner == null) return;

                if (condition.Positioner.CurrentIndex != condition.RequiredIndex)
                {
                    // At least one condition is not met
                    return;
                }
            }

            // All conditions met!
            Solve();
        }

        // ── Save/Load ────────────────────────────────────────────────────────
        // SteppedPositioner is deliberately not an ISaveable (it is shared by pipes, cyclers and
        // wheels), so the puzzle that owns the conditions persists their positions. Without this a
        // half-dialled combination was lost on load, and a solved one reappeared at its start index.

        [System.Serializable]
        private sealed class StatePuzzleSaveData
        {
            public int stateIndex;
            public List<int> positions = new List<int>();
        }

        public override string SaveData()
        {
            var data = new StatePuzzleSaveData { stateIndex = (int)State };
            foreach (var condition in _conditions)
                data.positions.Add(condition != null && condition.Positioner != null ? condition.Positioner.CurrentIndex : -1);
            return JsonUtility.ToJson(data);
        }

        public override void LoadData(string json)
        {
            base.LoadData(json);
            StatePuzzleSaveData data = JsonUtility.FromJson<StatePuzzleSaveData>(json);
            bool hasPositions = data?.positions != null && data.positions.Count == _conditions.Count;
            for (int i = 0; i < _conditions.Count; i++)
            {
                StateCondition condition = _conditions[i];
                if (condition == null || condition.Positioner == null) continue;
                // Older saves only stored the solved flag: show a solved lock at its answer.
                if (hasPositions && data.positions[i] >= 0) condition.Positioner.SetIndexInstant(data.positions[i]);
                else if (IsSolved) condition.Positioner.SetIndexInstant(condition.RequiredIndex);
            }
        }
    }
}
