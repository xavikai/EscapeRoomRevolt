using UnityEngine;
using UnityEngine.Events;
using EscapeRoomRevolt.Core;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>
    /// Base class for all puzzles. Inherit from this to create any puzzle type.
    /// Handles state, events and EventBus publishing automatically.
    ///
    /// Publishes: OnPuzzleSolved, OnPuzzleFailed
    /// </summary>
    public abstract class PuzzleController : MonoBehaviour
    {
        public enum PuzzleState { Unsolved, InProgress, Solved }

        [Header("Puzzle Identity")]
        [SerializeField] private string _puzzleId = "";
        [SerializeField] private string _puzzleName = "New Puzzle";

        [Header("Unity Events")]
        [SerializeField] private UnityEvent _onSolved;
        [SerializeField] private UnityEvent _onFailed;

        [Header("Debug")]
        [SerializeField] private bool _logState = true;

        private PuzzleState _state = PuzzleState.Unsolved;

        // ── Public API ───────────────────────────────────────────────────────
        public PuzzleState State => _state;
        public bool IsSolved => _state == PuzzleState.Solved;
        public string PuzzleId => string.IsNullOrEmpty(_puzzleId) ? name : _puzzleId;

        // ── Protected Methods ────────────────────────────────────────────────

        /// <summary>Call this when the puzzle is solved.</summary>
        protected void Solve()
        {
            if (_state == PuzzleState.Solved) return;

            _state = PuzzleState.Solved;
            Log("SOLVED!");

            EventBus.Publish(new OnPuzzleSolved { puzzleId = PuzzleId });
            _onSolved?.Invoke();

            OnPuzzleCompleted();
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
