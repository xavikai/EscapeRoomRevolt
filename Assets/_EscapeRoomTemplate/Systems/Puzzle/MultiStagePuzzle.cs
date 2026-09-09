using System;
using System.Collections.Generic;
using EscapeRoomRevolt.Systems.Interaction;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    [Serializable]
    public sealed class ChainedPuzzle
    {
        [Tooltip("Name used in logs and authoring. It only needs to be unique inside this chain.")]
        public string id = "puzzle";
        [Tooltip("The independent puzzle that must be solved.")]
        public PuzzleController puzzle;
        [Tooltip("Root containing this puzzle's interactive pieces. Defaults to the puzzle object.")]
        public GameObject interactionRoot;
    }

    /// <summary>
    /// Coordinates any number of independent puzzles that remain visible in the room. In free mode
    /// they may be solved in any order. In ordered mode only the current puzzle accepts interaction;
    /// later puzzles stay visible and unlock one by one. The coordinator solves only after every
    /// configured child puzzle has been solved, which is when a door or mechanism should be wired.
    /// </summary>
    public sealed class MultiStagePuzzle : PuzzleController
    {
        [Header("Chained Puzzles")]
        [SerializeField] private List<ChainedPuzzle> _puzzles = new List<ChainedPuzzle>();
        [Tooltip("Require the list order. Turn this off when all puzzles may be solved freely.")]
        [SerializeField] private bool _requireOrder = true;
        [Tooltip("In ordered mode, keep future puzzles visible but prevent their controls from responding until their turn.")]
        [SerializeField] private bool _lockFuturePuzzles = true;

        private readonly List<Action> _solvedCallbacks = new List<Action>();
        private readonly HashSet<int> _completed = new HashSet<int>();
        private readonly Dictionary<InteractableBase, bool> _authoredInteractionStates =
            new Dictionary<InteractableBase, bool>();
        private int _currentPuzzleIndex;

        public int PuzzleCount => _puzzles.Count;
        public int CurrentPuzzleIndex => _currentPuzzleIndex;
        public bool RequiresOrder => _requireOrder;
        public ChainedPuzzle GetPuzzle(int index) => index >= 0 && index < _puzzles.Count ? _puzzles[index] : null;

        protected override void Awake()
        {
            base.Awake();
            SubscribeToChildren();
            SynchronizeFromChildren();
        }

        protected override void OnDestroy()
        {
            UnsubscribeFromChildren();
            base.OnDestroy();
        }

        private void SubscribeToChildren()
        {
            UnsubscribeFromChildren();
            for (int index = 0; index < _puzzles.Count; index++)
            {
                int capturedIndex = index;
                Action callback = () => HandleChildSolved(capturedIndex);
                _solvedCallbacks.Add(callback);
                if (_puzzles[index]?.puzzle != null)
                    _puzzles[index].puzzle.OnSolvedEvent.AddListener(callback.Invoke);
            }
        }

        private void UnsubscribeFromChildren()
        {
            for (int index = 0; index < _solvedCallbacks.Count && index < _puzzles.Count; index++)
                if (_puzzles[index]?.puzzle != null)
                    _puzzles[index].puzzle.OnSolvedEvent.RemoveListener(_solvedCallbacks[index].Invoke);
            _solvedCallbacks.Clear();
        }

        private void SynchronizeFromChildren()
        {
            _completed.Clear();
            if (_requireOrder)
            {
                for (int index = 0; index < _puzzles.Count; index++)
                {
                    if (_puzzles[index]?.puzzle == null || !_puzzles[index].puzzle.IsSolved) break;
                    _completed.Add(index);
                }
            }
            else
            {
                for (int index = 0; index < _puzzles.Count; index++)
                    if (_puzzles[index]?.puzzle != null && _puzzles[index].puzzle.IsSolved)
                        _completed.Add(index);
            }

            _currentPuzzleIndex = FindNextIncomplete();
            ApplyInteractionLocks();
            EvaluateCompletion();
        }

        private void HandleChildSolved(int index)
        {
            if (IsSolved || index < 0 || index >= _puzzles.Count) return;

            if (_requireOrder && index != _currentPuzzleIndex)
            {
                Debug.LogWarning($"[MultiStagePuzzle:{DisplayName}] '{_puzzles[index].id}' was solved out of order and was reset.", this);
                _puzzles[index].puzzle?.ResetPuzzle();
                return;
            }

            _completed.Add(index);
            SetInProgress();
            _currentPuzzleIndex = FindNextIncomplete();
            ApplyInteractionLocks();
            EvaluateCompletion();
        }

        private int FindNextIncomplete()
        {
            for (int index = 0; index < _puzzles.Count; index++)
                if (!_completed.Contains(index)) return index;
            return _puzzles.Count;
        }

        private void EvaluateCompletion()
        {
            if (_puzzles.Count == 0 || _completed.Count < _puzzles.Count) return;
            foreach (ChainedPuzzle entry in _puzzles)
                if (entry?.puzzle == null || !entry.puzzle.IsSolved) return;
            Solve();
        }

        private void ApplyInteractionLocks()
        {
            for (int index = 0; index < _puzzles.Count; index++)
            {
                ChainedPuzzle entry = _puzzles[index];
                if (entry == null) continue;
                GameObject root = entry.interactionRoot != null
                    ? entry.interactionRoot
                    : entry.puzzle != null ? entry.puzzle.gameObject : null;
                if (root == null) continue;

                bool enabled = !_requireOrder || !_lockFuturePuzzles || index == _currentPuzzleIndex || _completed.Contains(index);
                foreach (InteractableBase interactable in root.GetComponentsInChildren<InteractableBase>(true))
                {
                    if (!_authoredInteractionStates.ContainsKey(interactable))
                        _authoredInteractionStates.Add(interactable, interactable.InteractionEnabled);
                    interactable.SetInteractionEnabled(enabled && !_completed.Contains(index)
                        && _authoredInteractionStates[interactable]);
                }
            }
        }

        protected override void OnPuzzleReset()
        {
            _completed.Clear();
            foreach (ChainedPuzzle entry in _puzzles)
                entry?.puzzle?.ResetPuzzle();
            _currentPuzzleIndex = 0;
            ApplyInteractionLocks();
        }

        public override void LoadData(string json)
        {
            base.LoadData(json);
            SynchronizeFromChildren();
        }
    }
}
