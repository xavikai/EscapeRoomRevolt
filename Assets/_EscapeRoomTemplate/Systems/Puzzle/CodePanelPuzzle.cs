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

        [Header("Display")]
        [Tooltip("Optional 3D Text Display to show the current code.")]
        [SerializeField] private TMPro.TMP_Text _display3D;
        [Tooltip("Optional LED that turns green when solved.")]
        [SerializeField] private Renderer _statusLed;
        
        [Header("Audio")]
        [SerializeField] private AudioClip _beepSound;
        [SerializeField] private AudioClip _successSound;
        [SerializeField] private AudioClip _errorSound;
        [SerializeField] private float _pitchVariance = 0.05f;

        private string _currentInput = "";

        public string CurrentInput => _currentInput;

        private void Start()
        {
            if (_statusLed != null)
            {
                _statusLed.material.color = Color.red;
            }
        }

        /// <summary>Adds a digit/character to the current input sequence.</summary>
        public void InputDigit(string digit)
        {
            if (IsSolved) return;

            if (digit == "C")
            {
                ClearInput();
                return;
            }

            SetInProgress();
            
            if (_currentInput.Length < _maxCodeLength)
            {
                _currentInput += digit;
                UpdateDisplay();
                
                if (_beepSound != null && EscapeRoomRevolt.Systems.Audio.AudioManager.Instance != null)
                {
                    EscapeRoomRevolt.Systems.Audio.AudioManager.Instance.PlaySoundAt(_beepSound, transform.position, 1f, _pitchVariance);
                }
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
                if (_successSound != null && EscapeRoomRevolt.Systems.Audio.AudioManager.Instance != null)
                {
                    EscapeRoomRevolt.Systems.Audio.AudioManager.Instance.PlaySoundAt(_successSound, transform.position);
                }
                if (_statusLed != null)
                {
                    _statusLed.material.color = Color.green;
                    _statusLed.material.EnableKeyword("_EMISSION");
                    _statusLed.material.SetColor("_EmissionColor", Color.green * 2f);
                }
                Solve();
            }
            else
            {
                if (_errorSound != null && EscapeRoomRevolt.Systems.Audio.AudioManager.Instance != null)
                {
                    EscapeRoomRevolt.Systems.Audio.AudioManager.Instance.PlaySoundAt(_errorSound, transform.position);
                }
                Fail("Incorrect code");
                ClearInput();
                if (_statusLed != null)
                {
                    StartCoroutine(FlashRedLed());
                }
            }
        }

        private System.Collections.IEnumerator FlashRedLed()
        {
            _statusLed.material.EnableKeyword("_EMISSION");
            _statusLed.material.SetColor("_EmissionColor", Color.red * 2f);
            
            yield return new WaitForSeconds(0.2f);
            
            _statusLed.material.SetColor("_EmissionColor", Color.red * 0.5f);
            
            yield return new WaitForSeconds(0.1f);
            
            _statusLed.material.SetColor("_EmissionColor", Color.red * 2f);
            
            yield return new WaitForSeconds(0.2f);
            
            _statusLed.material.SetColor("_EmissionColor", Color.red * 0.5f);
        }

        /// <summary>Clears the current input.</summary>
        public void ClearInput()
        {
            if (IsSolved) return;
            _currentInput = "";
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_display3D != null)
            {
                _display3D.text = _currentInput;
            }
        }
    }
}
