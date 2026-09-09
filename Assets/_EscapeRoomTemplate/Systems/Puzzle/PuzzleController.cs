using UnityEngine;
using UnityEngine.Events;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Core.Save;
using EscapeRoomRevolt.Core.Settings;
using EscapeRoomRevolt.Systems.Hint;
using EscapeRoomRevolt.Systems.Survival;

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
        [SerializeField] private PuzzleDefinition _definition;
        [SerializeField] private string _puzzleId = "";
        [SerializeField] private string _puzzleName = "New Puzzle";

        [Header("Unity Events")]
        [SerializeField] private UnityEvent _onSolved = new UnityEvent();
        [SerializeField] private UnityEvent _onFailed = new UnityEvent();

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
        public string PuzzleId => _definition != null && !string.IsNullOrWhiteSpace(_definition.PersistentId)
            ? _definition.PersistentId
            : (string.IsNullOrEmpty(_puzzleId) ? name : _puzzleId);
        public string DisplayName => _definition != null ? _definition.DisplayName : _puzzleName;
        public PuzzleDefinition Definition => _definition;

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
            if (_definition != null && _definition.Hints != null)
                HintManager.Instance?.ClearActivePuzzle(_definition.Hints);

            // Skipped in VR on purpose: yanking the view to another camera fights head tracking and
            // is a reliable way to make players ill. The solve still reads through sound and through
            // whatever OnSolved drives (a door opening, a light), which need no camera cut.
            if (_feedbackCamera != null && !IsVirtualReality)
            {
                StartCoroutine(PlayFeedbackCinematic());
            }

            OnPuzzleCompleted();
        }

        /// <summary>True while the player is in a headset, so presentation that assumes control of the camera can stand aside.</summary>
        protected static bool IsVirtualReality =>
            EscapeRoomRevolt.Player.PlayerPlatformRegistry.Current != null &&
            EscapeRoomRevolt.Player.PlayerPlatformRegistry.Current.Platform == EscapeRoomRevolt.Player.PlayerPlatform.VirtualReality;

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
            if (GameFeatures.IsEnabled(OptionalGameFeature.Sanity)
                && _definition != null && _definition.SanityPenalty > 0f)
                SanityController.Instance?.ApplyStress(_definition.SanityPenalty);

            OnPuzzleFailed(reason);
        }

        protected void SetInProgress()
        {
            if (_state == PuzzleState.Unsolved && _definition != null && _definition.Hints != null)
                HintManager.Instance?.SetActivePuzzle(_definition.Hints);
            _state = PuzzleState.InProgress;
        }

        /// <summary>Override to react when the puzzle is solved (animations, doors, etc.)</summary>
        protected virtual void OnPuzzleCompleted() { }

        /// <summary>Override to react on failure (shake animation, sound, etc.)</summary>
        protected virtual void OnPuzzleFailed(string reason) { }

        /// <summary>
        /// Safely returns the puzzle to Unsolved and clears its hint tracking, then lets the
        /// subclass clear its own transient attempt state via OnPuzzleReset(). Safe to call at any
        /// state, including Solved — useful for a "reset this puzzle" trigger without reloading the
        /// scene. Does not publish OnPuzzleSolved/Failed or re-apply their side effects.
        /// </summary>
        public void ResetPuzzle()
        {
            _state = PuzzleState.Unsolved;
            if (_definition != null && _definition.Hints != null)
                HintManager.Instance?.ClearActivePuzzle(_definition.Hints);
            OnPuzzleReset();
        }

        /// <summary>Override to clear subclass-specific transient state (current input, connections, stage index, etc.) when ResetPuzzle() is called.</summary>
        protected virtual void OnPuzzleReset() { }

        /// <summary>
        /// Deterministic per-puzzle, per-playthrough seed for optional randomized variants (a
        /// shuffled sequence, a rerolled code, a scrambled wiring). Combines SaveManager.RunSeed
        /// with this puzzle's own SaveId, so every puzzle in a scene gets a different roll, but the
        /// same puzzle rolls the same result every time within one playthrough.
        /// </summary>
        protected int ResolveVariantSeed() =>
            (SaveManager.Instance != null ? SaveManager.Instance.RunSeed : 0) ^ SaveId.GetHashCode();

        private void Log(string msg)
        {
            if (_logState) Debug.Log($"[Puzzle:{DisplayName}] {msg}");
        }
    }
}
