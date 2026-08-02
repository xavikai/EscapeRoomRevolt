using System.Collections;
using UnityEngine;
using EscapeRoomRevolt.UI.PC;
using System;
using EscapeRoomRevolt.Core.Input;

namespace EscapeRoomRevolt.Systems.Hint
{
    /// <summary>
    /// Manages displaying hints to the player when they spend too long
    /// on an active puzzle without solving it.
    /// </summary>
    public class HintManager : MonoBehaviour
    {
        public static HintManager Instance { get; private set; }

        private HintData _activePuzzleData;
        private float _timeInActivePuzzle;
        private int _currentHintIndex;
        private Coroutine _hideSubtitleCoroutine;
        public event Action<HintEntry, int> HintShown;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (InputRouter.Instance != null && InputRouter.Instance.HintPressed) RequestNextHint();
            if (_activePuzzleData == null) return;
            if (_currentHintIndex >= _activePuzzleData.hints.Count) return;

            _timeInActivePuzzle += Time.deltaTime;

            float targetTime = (_currentHintIndex == 0) 
                ? _activePuzzleData.delayBeforeFirstHint 
                : _activePuzzleData.delayBetweenHints;

            if (_timeInActivePuzzle >= targetTime)
            {
                ShowNextHint();
                _timeInActivePuzzle = 0f;
            }
        }

        public void SetActivePuzzle(HintData puzzleData)
        {
            if (_activePuzzleData == puzzleData) return;
            
            _activePuzzleData = puzzleData;
            _timeInActivePuzzle = 0f;
            _currentHintIndex = 0;
        }

        public void ClearActivePuzzle()
        {
            _activePuzzleData = null;
            _timeInActivePuzzle = 0f;
            _currentHintIndex = 0;
            if (_hideSubtitleCoroutine != null)
            {
                StopCoroutine(_hideSubtitleCoroutine);
            }
            UIManager.Instance?.HideSubtitle();
        }

        public void ClearActivePuzzle(HintData puzzleData)
        {
            if (_activePuzzleData == puzzleData) ClearActivePuzzle();
        }

        public bool RequestNextHint()
        {
            if (_activePuzzleData == null || _currentHintIndex >= _activePuzzleData.hints.Count) return false;
            ShowNextHint();
            _timeInActivePuzzle = 0f;
            return true;
        }

        private void ShowNextHint()
        {
            if (_activePuzzleData == null || _currentHintIndex >= _activePuzzleData.hints.Count) return;

            HintEntry currentHint = _activePuzzleData.hints[_currentHintIndex];
            int shownIndex = _currentHintIndex;
            
            // Format as character thoughts (italics)
            string formattedText = $"<i>{currentHint.hintText}</i>";
            
            UIManager.Instance?.ShowSubtitle(formattedText);
            
            // Play Audio if it exists
            if (currentHint.hintAudio != null && EscapeRoomRevolt.Systems.Audio.AudioManager.Instance != null)
            {
                EscapeRoomRevolt.Systems.Audio.AudioManager.Instance.PlayVoice(currentHint.hintAudio);
            }
            
            if (_hideSubtitleCoroutine != null)
                StopCoroutine(_hideSubtitleCoroutine);
                
            _hideSubtitleCoroutine = StartCoroutine(HideSubtitleAfterDelay(5f));

            _currentHintIndex++;
            HintShown?.Invoke(currentHint, shownIndex);
        }

        private IEnumerator HideSubtitleAfterDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            UIManager.Instance?.HideSubtitle();
        }
    }
}
