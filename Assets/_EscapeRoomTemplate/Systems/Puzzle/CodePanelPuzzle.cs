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

        [Header("Variants")]
        [Tooltip("Rolls a different numeric code each playthrough, seeded from SaveManager.RunSeed so it stays the same across a save/reload of this playthrough but differs from the next one. The rolled code is itself persisted in this puzzle's own save data, so loading never rerolls it out from under an already-solved puzzle or a hint the player already read.")]
        [SerializeField] private bool _randomizeCode;

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

        [Header("Accessibility")]
        [Tooltip("Idle LED breathes between these two intensities instead of staying a flat color, so solved/unsolved reads without relying on hue alone.")]
        [SerializeField, Min(0f)] private float _idlePulseSpeed = 2.5f;

        private string _currentInput = "";
        private bool _isFlashingLed;

        public string CurrentInput => _currentInput;

        protected override void Awake()
        {
            base.Awake();
            if (_randomizeCode) _correctCode = RollRandomCode();
        }

        private string RollRandomCode()
        {
            System.Random random = new System.Random(ResolveVariantSeed());
            char[] digits = new char[Mathf.Max(1, _maxCodeLength)];
            for (int i = 0; i < digits.Length; i++) digits[i] = (char)('0' + random.Next(10));
            return new string(digits);
        }

        private void Start()
        {
            if (_statusLed != null && !IsSolved)
            {
                _statusLed.material.color = Color.red;
                _statusLed.material.EnableKeyword("_EMISSION");
                _statusLed.material.SetColor("_EmissionColor", Color.red * 2f);
            }
        }

        private void Update()
        {
            if (_statusLed == null || IsSolved || _isFlashingLed) return;
            float pulse = (Mathf.Sin(Time.time * _idlePulseSpeed) + 1f) * .5f;
            _statusLed.material.SetColor("_EmissionColor", Color.red * Mathf.Lerp(.4f, 2f, pulse));
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
            _isFlashingLed = true;
            _statusLed.material.EnableKeyword("_EMISSION");
            _statusLed.material.SetColor("_EmissionColor", Color.red * 2f);
            
            yield return new WaitForSeconds(0.2f);
            
            _statusLed.material.SetColor("_EmissionColor", Color.red * 0.5f);
            
            yield return new WaitForSeconds(0.1f);
            
            _statusLed.material.SetColor("_EmissionColor", Color.red * 2f);
            
            yield return new WaitForSeconds(0.2f);
            
            _statusLed.material.SetColor("_EmissionColor", Color.red * 0.5f);
            _isFlashingLed = false;
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

        protected override void OnPuzzleCompleted()
        {
            base.OnPuzzleCompleted();
            _currentInput = _correctCode;
            UpdateDisplay();

            if (_statusLed != null)
            {
                _statusLed.material.color = Color.green;
                _statusLed.material.EnableKeyword("_EMISSION");
                _statusLed.material.SetColor("_EmissionColor", Color.green * 2f);
            }
        }

        protected override void OnPuzzleReset()
        {
            _currentInput = "";
            UpdateDisplay();
            if (_statusLed == null) return;
            _statusLed.material.color = Color.red;
            _statusLed.material.EnableKeyword("_EMISSION");
            _statusLed.material.SetColor("_EmissionColor", Color.red * 2f);
        }

        [System.Serializable]
        private sealed class CodePanelSaveData
        {
            public int stateIndex;
            public string chosenCode;
        }

        public override string SaveData()
        {
            return JsonUtility.ToJson(new CodePanelSaveData { stateIndex = (int)State, chosenCode = _correctCode });
        }

        public override void LoadData(string json)
        {
            base.LoadData(json);
            CodePanelSaveData data = JsonUtility.FromJson<CodePanelSaveData>(json);
            if (data != null && !string.IsNullOrEmpty(data.chosenCode)) _correctCode = data.chosenCode;
        }
    }
}
