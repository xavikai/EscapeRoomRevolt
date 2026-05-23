using UnityEngine;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>
    /// A numeric keypad or combination lock puzzle.
    /// Inputs can be gathered from UI buttons or 3D interactable buttons.
    /// </summary>
    public class CodePanelPuzzle : PuzzleController
    {
        [Header("Code Settings")]
        [SerializeField] private string _correctCode = "1234";
        [SerializeField] private int _maxCodeLength = 4;
        [SerializeField] private bool _autoCheckWhenFull = true;

        private string _currentInput = "";

        public string CurrentInput => _currentInput;

        /// <summary>Adds a digit/character to the current input sequence.</summary>
        public void InputDigit(string digit)
        {
            if (IsSolved) return;

            SetInProgress();
            
            if (_currentInput.Length < _maxCodeLength)
            {
                _currentInput += digit;
            }

            if (_autoCheckWhenFull && _currentInput.Length == _maxCodeLength)
            {
                SubmitCode();
            }
        }

        /// <summary>Checks if the current input matches the correct code.</summary>
        public void SubmitCode()
        {
            if (IsSolved) return;

            if (_currentInput == _correctCode)
            {
                Solve();
            }
            else
            {
                Fail("Incorrect code");
                ClearInput();
            }
        }

        /// <summary>Clears the current input.</summary>
        public void ClearInput()
        {
            if (IsSolved) return;
            _currentInput = "";
        }
    }
}
