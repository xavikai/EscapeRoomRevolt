using System;
using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>
    /// Throw-and-hit puzzle: the player must knock every listed target with a thrown PhysicsGrabbable
    /// object. Unlike PlacementPuzzle (careful, precise placement), this rewards aim and force — any
    /// grabbable object hitting fast enough counts, so it's about throwing accuracy, not item matching.
    /// </summary>
    public class ThrowPuzzle : PuzzleController
    {
        [Header("Targets")]
        [Tooltip("Id of every target that must be hit, in any order, to solve this puzzle.")]
        [SerializeField] private List<string> _targetIds = new List<string>();

        private readonly HashSet<string> _hitTargets = new HashSet<string>();

        public int TargetCount => _targetIds.Count;
        public int HitCount => _hitTargets.Count;

        /// <summary>Called by a ThrowTarget when it registers a qualifying impact.</summary>
        public void RegisterHit(string targetId)
        {
            if (IsSolved || string.IsNullOrEmpty(targetId) || !_targetIds.Contains(targetId)) return;

            SetInProgress();
            _hitTargets.Add(targetId);

            if (_hitTargets.Count >= _targetIds.Count) Solve();
        }

        public bool IsTargetHit(string targetId) => _hitTargets.Contains(targetId);

        protected override void OnPuzzleReset() => _hitTargets.Clear();

        [Serializable]
        private sealed class ThrowPuzzleSaveData
        {
            public int stateIndex;
            public List<string> hitTargets = new List<string>();
        }

        public override string SaveData()
        {
            var data = new ThrowPuzzleSaveData { stateIndex = (int)State };
            data.hitTargets.AddRange(_hitTargets);
            return JsonUtility.ToJson(data);
        }

        public override void LoadData(string json)
        {
            base.LoadData(json);
            var data = JsonUtility.FromJson<ThrowPuzzleSaveData>(json);
            _hitTargets.Clear();
            if (data?.hitTargets == null) return;
            foreach (string id in data.hitTargets) _hitTargets.Add(id);
        }
    }
}
