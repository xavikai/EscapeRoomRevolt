using UnityEngine;
using TMPro;
using EscapeRoomRevolt.Systems.Puzzle;

namespace EscapeRoomRevolt.UI.PC
{
    public class KeypadUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _displayText;
        
        private CodePanelPuzzle _currentPuzzle;

        public void Setup(CodePanelPuzzle puzzle)
        {
            _currentPuzzle = puzzle;
            UpdateDisplay();
        }

        private void Update()
        {
            if (_currentPuzzle == null || _currentPuzzle.IsSolved) return;

            // Detect keyboard input (0-9)
            for (int i = 0; i <= 9; i++)
            {
                if (Input.GetKeyDown(i.ToString()) || Input.GetKeyDown(KeyCode.Keypad0 + i))
                {
                    OnDigitPressed(i.ToString());
                }
            }

            if (Input.GetKeyDown(KeyCode.Backspace)) OnClearPressed();
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) OnSubmitPressed();
        }

        public void OnDigitPressed(string digit)
        {
            if (_currentPuzzle == null || _currentPuzzle.IsSolved) return;
            _currentPuzzle.InputDigit(digit);
            UpdateDisplay();
            
            if (_currentPuzzle.IsSolved)
            {
                // Auto-close when solved successfully
                Invoke(nameof(Close), 1f); 
            }
        }

        public void OnClearPressed()
        {
            if (_currentPuzzle != null) _currentPuzzle.ClearInput();
            UpdateDisplay();
        }
        
        public void OnSubmitPressed()
        {
            if (_currentPuzzle != null) _currentPuzzle.SubmitCode();
            UpdateDisplay();
            if (_currentPuzzle.IsSolved) Invoke(nameof(Close), 1f);
        }

        private void UpdateDisplay()
        {
            if (_currentPuzzle != null && _displayText != null)
            {
                _displayText.text = _currentPuzzle.IsSolved ? "OK" : _currentPuzzle.CurrentInput;
            }
        }

        public void Close()
        {
            UIManager.Instance.HideKeypad();
        }
    }
}
