using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>
    /// Classic sliding-tile puzzle (15-puzzle style): a grid of tile ids with one empty slot
    /// (represented by ""), solved when every tile matches the authored target order. Starts
    /// pre-shuffled by performing random *legal* slides from the solved state, seeded from
    /// PuzzleController.ResolveVariantSeed() — this guarantees the starting arrangement is always
    /// solvable (a naive random permutation of tiles has only a 50% chance of being solvable) and
    /// stays the same across a save/reload of this playthrough.
    /// Scene interactables (a clickable tile, a drag gesture, anything) call TryMoveTile(tileId);
    /// this controller only tracks the grid state, not how tiles are rendered.
    /// </summary>
    public class SlidingPuzzle : PuzzleController
    {
        [Header("Grid")]
        [SerializeField, Min(1)] private int _columns = 3;
        [SerializeField, Min(1)] private int _rows = 3;
        [Tooltip("Solved arrangement, row-major (left-to-right, top-to-bottom). Exactly one entry must be empty (\"\") to represent the open slot. Size must be columns * rows.")]
        [SerializeField] private List<string> _targetOrder = new List<string>();
        [Tooltip("How many random legal slides to scramble from the solved state. Higher = harder to solve by eyeballing it, but always still solvable.")]
        [SerializeField, Min(1)] private int _shuffleMoveCount = 60;

        private List<string> _currentOrder;

        public int Columns => _columns;
        public int Rows => _rows;
        public IReadOnlyList<string> CurrentOrder => _currentOrder;

        protected override void Awake()
        {
            base.Awake();
            _currentOrder = Shuffle(new System.Random(ResolveVariantSeed()));
        }

        /// <summary>Slides the tile with the given id into the adjacent empty slot, if that move is legal. Returns whether the move was applied.</summary>
        public bool TryMoveTile(string tileId)
        {
            if (IsSolved || _currentOrder == null) return false;
            int tileIndex = _currentOrder.IndexOf(tileId);
            int emptyIndex = _currentOrder.IndexOf("");
            if (tileIndex < 0 || emptyIndex < 0 || !AreAdjacent(tileIndex, emptyIndex)) return false;

            SetInProgress();
            (_currentOrder[tileIndex], _currentOrder[emptyIndex]) = (_currentOrder[emptyIndex], _currentOrder[tileIndex]);
            CheckSolved();
            return true;
        }

        public string GetTileAt(int index) =>
            _currentOrder != null && index >= 0 && index < _currentOrder.Count ? _currentOrder[index] : null;

        private void CheckSolved()
        {
            for (int i = 0; i < _currentOrder.Count; i++)
                if (_currentOrder[i] != _targetOrder[i]) return;
            Solve();
        }

        private bool AreAdjacent(int a, int b)
        {
            int ax = a % _columns, ay = a / _columns;
            int bx = b % _columns, by = b / _columns;
            return (ax == bx && Mathf.Abs(ay - by) == 1) || (ay == by && Mathf.Abs(ax - bx) == 1);
        }

        private List<int> GetNeighborIndices(int index)
        {
            List<int> result = new List<int>();
            int x = index % _columns, y = index / _columns;
            if (x > 0) result.Add(index - 1);
            if (x < _columns - 1) result.Add(index + 1);
            if (y > 0) result.Add(index - _columns);
            if (y < _rows - 1) result.Add(index + _columns);
            return result;
        }

        /// <summary>Scrambles by taking random legal slides from the solved state, never immediately undoing the previous move — guarantees the result is always solvable.</summary>
        private List<string> Shuffle(System.Random random)
        {
            List<string> order = new List<string>(_targetOrder);
            int emptyIndex = order.IndexOf("");
            if (emptyIndex < 0) return order;

            int lastMovedFrom = -1;
            for (int i = 0; i < _shuffleMoveCount; i++)
            {
                List<int> neighbors = GetNeighborIndices(emptyIndex);
                neighbors.Remove(lastMovedFrom);
                if (neighbors.Count == 0) continue;
                int chosen = neighbors[random.Next(neighbors.Count)];
                (order[emptyIndex], order[chosen]) = (order[chosen], order[emptyIndex]);
                lastMovedFrom = emptyIndex;
                emptyIndex = chosen;
            }
            return order;
        }

        protected override void OnPuzzleReset() => _currentOrder = Shuffle(new System.Random(ResolveVariantSeed()));

        [System.Serializable]
        private sealed class SlidingSaveData
        {
            public int stateIndex;
            public List<string> currentOrder;
        }

        public override string SaveData()
        {
            return JsonUtility.ToJson(new SlidingSaveData { stateIndex = (int)State, currentOrder = _currentOrder });
        }

        public override void LoadData(string json)
        {
            base.LoadData(json);
            SlidingSaveData data = JsonUtility.FromJson<SlidingSaveData>(json);
            if (data?.currentOrder != null && data.currentOrder.Count == _targetOrder.Count) _currentOrder = data.currentOrder;
        }
    }
}
