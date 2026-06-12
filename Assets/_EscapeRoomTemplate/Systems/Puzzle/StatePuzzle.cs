using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using EscapeRoomRevolt.Systems.Interaction;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    [System.Serializable]
    public class StateCondition
    {
        public InteractableToggle Toggle;
        public bool RequiredState;
    }

    /// <summary>
    /// A puzzle that requires multiple toggles (levers, switches) to be in a specific state.
    /// Order does not matter.
    /// </summary>
    public class StatePuzzle : PuzzleController
    {
        [Header("State Settings")]
        [Tooltip("The required states for each toggle to solve the puzzle.")]
        [SerializeField] private List<StateCondition> _conditions;

        private void OnEnable()
        {
            foreach (var condition in _conditions)
            {
                if (condition.Toggle != null)
                {
                    condition.Toggle.OnStateToggled.AddListener(OnToggleChanged);
                }
            }
        }

        private void OnDisable()
        {
            foreach (var condition in _conditions)
            {
                if (condition.Toggle != null)
                {
                    condition.Toggle.OnStateToggled.RemoveListener(OnToggleChanged);
                }
            }
        }

        private void OnToggleChanged(bool newState)
        {
            if (IsSolved) return;

            SetInProgress();
            CheckStates();
        }

        private void CheckStates()
        {
            foreach (var condition in _conditions)
            {
                if (condition.Toggle == null) continue;
                
                if (condition.Toggle.IsOn != condition.RequiredState)
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
