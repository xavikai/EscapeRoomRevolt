using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>
    /// A puzzle that requires interacting with objects in a specific order.
    /// E.g. pulling levers, pressing colored buttons, or stepping on plates.
    /// </summary>
    public class SequencePuzzle : PuzzleController
    {
        [Header("Sequence Settings")]
        [Tooltip("The correct sequence of IDs that the player must input")]
        [SerializeField] private List<string> _correctSequence;
        
        private List<string> _currentSequence = new List<string>();

        /// <summary>Registers an input step into the current sequence.</summary>
        public void InputStep(string stepId)
        {
            if (IsSolved) return;

            SetInProgress();
            _currentSequence.Add(stepId);

            CheckSequence();
        }

        private void CheckSequence()
        {
            // Check if the current steps match the beginning of the correct sequence
            for (int i = 0; i < _currentSequence.Count; i++)
            {
                if (_currentSequence[i] != _correctSequence[i])
                {
                    Fail("Wrong sequence step");
                    _currentSequence.Clear();
                    return;
                }
            }

            // If we reached the end of the sequence successfully
            if (_currentSequence.Count == _correctSequence.Count)
            {
                Solve();
            }
        }
    }
}
