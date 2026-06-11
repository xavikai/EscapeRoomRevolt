using UnityEngine;
using UnityEngine.Events;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Core.Save;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>
    /// Base class for all puzzles. Inherit from this to create any puzzle type.
    /// Handles state, events and EventBus publishing automatically.
    ///
    /// Publishes: OnPuzzleSolved, OnPuzzleFailed
    /// </summary>
    public abstract class PuzzleController : MonoBehaviour, ISaveable
    {
        public enum PuzzleState { Unsolved, InProgress, Solved }

        [Header("Puzzle Identity")]
        [SerializeField] private string _puzzleId = "";
        [SerializeField] private string _puzzleName = "New Puzzle";

        [Header("Unity Events")]
        [SerializeField] private UnityEvent _onSolved;
        [SerializeField] private UnityEvent _onFailed;

        public UnityEvent OnSolvedEvent => _onSolved;
        public UnityEvent OnFailedEvent => _onFailed;

        [Header("Cinematics")]
        [Tooltip("If assigned, this camera will turn on when the puzzle is solved to show the result (e.g. a door opening).")]
        [SerializeField] private Camera _feedbackCamera;
        [Tooltip("How long to wait after solving before cutting to the feedback camera.")]
        [SerializeField] private float _feedbackDelay = 1.0f;
        [Tooltip("How long to stay on the feedback camera before returning to the player.")]
        [SerializeField] private float _feedbackDuration = 2f;

        [Header("Debug")]
        [SerializeField] private bool _logState = true;

        private PuzzleState _state = PuzzleState.Unsolved;

        // ── Public API ───────────────────────────────────────────────────────
        public PuzzleState State => _state;
        public bool IsSolved => _state == PuzzleState.Solved;
        public string PuzzleId => string.IsNullOrEmpty(_puzzleId) ? name : _puzzleId;

        protected virtual void Awake()
        {
            SaveManager.Instance?.Register(this);
        }

        protected virtual void OnDestroy()
        {
            SaveManager.Instance?.Unregister(this);
        }

        // ── Save/Load ────────────────────────────────────────────────────────

        public string SaveId => PuzzleId;

        [System.Serializable]
        private class PuzzleSaveData
        {
            public int stateIndex;
        }

        public virtual string SaveData()
        {
            var data = new PuzzleSaveData
            {
                stateIndex = (int)_state
            };
            return JsonUtility.ToJson(data);
        }

        public virtual void LoadData(string json)
        {
            var data = JsonUtility.FromJson<PuzzleSaveData>(json);
            if (data == null) return;

            _state = (PuzzleState)data.stateIndex;
            
            if (_state == PuzzleState.Solved)
            {
                // Call virtual method to restore visuals immediately
                OnPuzzleCompleted();
            }
        }

        // ── Protected Methods ────────────────────────────────────────────────

        /// <summary>Call this when the puzzle is solved.</summary>
        protected void Solve()
        {
            if (_state == PuzzleState.Solved) return;

            _state = PuzzleState.Solved;
            Log("SOLVED!");

            EventBus.Publish(new OnPuzzleSolved { puzzleId = PuzzleId });
            _onSolved?.Invoke();

            if (_feedbackCamera != null)
            {
                StartCoroutine(PlayFeedbackCinematic());
            }

            OnPuzzleCompleted();
        }

        private System.Collections.IEnumerator PlayFeedbackCinematic()
        {
            if (_feedbackDelay > 0)
            {
                yield return new WaitForSeconds(_feedbackDelay);
            }

            _feedbackCamera.gameObject.SetActive(true);
            _feedbackCamera.depth = 5; // Ensure it renders on top of everything
            
            yield return new WaitForSeconds(_feedbackDuration);
            
            _feedbackCamera.gameObject.SetActive(false);
        }

        /// <summary>Call this when the player makes a wrong attempt.</summary>
        protected void Fail(string reason = "Wrong answer")
        {
            _state = PuzzleState.InProgress;
            Log($"Failed — {reason}");

            EventBus.Publish(new OnPuzzleFailed { puzzleId = PuzzleId, reason = reason });
            _onFailed?.Invoke();

            OnPuzzleFailed(reason);
        }

        protected void SetInProgress() => _state = PuzzleState.InProgress;

        /// <summary>Override to react when the puzzle is solved (animations, doors, etc.)</summary>
        protected virtual void OnPuzzleCompleted() { }

        /// <summary>Override to react on failure (shake animation, sound, etc.)</summary>
        protected virtual void OnPuzzleFailed(string reason) { }

        private void Log(string msg)
        {
            if (_logState) Debug.Log($"[Puzzle:{_puzzleName}] {msg}");
        }
    }
}
