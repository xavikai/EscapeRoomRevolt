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
        [SerializeField] private List<StateCondition> _conditions;

        private void OnEnable()
        {
            foreach (var condition in _conditions)
            {
                if (condition.Positioner != null)
                {
                    condition.Positioner.OnPositionChanged.AddListener(OnPositionChanged);
                }
            }
        }

        private void OnDisable()
        {
            foreach (var condition in _conditions)
            {
                if (condition.Positioner != null)
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
            foreach (var condition in _conditions)
            {
                if (condition.Positioner == null) continue;

                if (condition.Positioner.CurrentIndex != condition.RequiredIndex)
                {
                    // At least one condition is not met
                    return;
                }
            }

            // All conditions met!
            Solve();
        }
    }
}
