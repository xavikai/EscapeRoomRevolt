using System;
using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>
    /// Classic sliding-tile puzzle (15-puzzle style): a grid of tile ids with one empty slot
    /// (represented by ""), solved when every tile matches the target arrangement. Starts
    /// pre-shuffled by performing random *legal* slides from the solved state, seeded from
    /// PuzzleController.ResolveVariantSeed() — this guarantees the starting arrangement is always
    /// solvable (a naive random permutation of tiles has only a 50% chance of being solvable) and
    /// stays the same across a save/reload of this playthrough.
    ///
    /// The solved arrangement is normally not authored at all: tiles are numbered 1..N over the grid
    /// in reading order and the designer only picks which cell stays empty. That is what an
    /// image-fragment board wants, and it means the correct answer is legible from the art rather
    /// than from a list in the Inspector. Boards whose solution genuinely is not the natural order
    /// can still hand-author Target Order under Advanced.
    ///
    /// Scene interactables (a clickable tile, a drag gesture, anything) call TryMoveTile(tileId);
    /// this controller only tracks the grid state, not how tiles are rendered.
    /// </summary>
    public class SlidingPuzzle : PuzzleController
    {
        [Header("Grid")]
        [SerializeField, Min(1)] private int _columns = 3;
        [SerializeField, Min(1)] private int _rows = 3;
        [Tooltip("Which cell is empty once the puzzle is solved, as (column, row) counting from the top-left. Everything else follows from it: the remaining cells get tiles 1..N in reading order.")]
        [SerializeField] private Vector2Int _holeCell = new Vector2Int(2, 2);
        [Tooltip("How many random legal slides to scramble from the solved state. Higher = harder to solve by eyeballing it, but always still solvable.")]
        [SerializeField, Min(1)] private int _shuffleMoveCount = 60;

        [Header("Advanced")]
        [Tooltip("Off (normal): the solved arrangement is tiles 1..N in reading order with Hole Cell empty. On: Target Order below is authored by hand, for the rare board whose solution is not the natural order.")]
        [SerializeField] private bool _customTargetOrder;
        [Tooltip("Solved arrangement, row-major (left-to-right, top-to-bottom). Exactly one entry must be empty (\"\") to represent the open slot, and the size must be columns * rows. Only used when Custom Target Order is on.")]
        [SerializeField] private List<string> _targetOrder = new List<string>();

        private List<string> _currentOrder;

        public int Columns => _columns;
        public int Rows => _rows;
        public Vector2Int HoleCell => _holeCell;
        public IReadOnlyList<string> CurrentOrder => _currentOrder;
        /// <summary>The solved arrangement. Outside play mode this is the only order that exists, so views use it to preview the assembled board — and to decide which image fragment each tile carries.</summary>
        public IReadOnlyList<string> TargetOrder => _targetOrder;

        /// <summary>Raised with the tile id after a legal slide, for views that want to react (sound, particles).</summary>
        public event Action<string> TileMoved;
        /// <summary>Raised with the tile id when the player clicks a tile that cannot move. Without feedback for this the board reads as broken, since most clicks on a sliding puzzle are illegal.</summary>
        public event Action<string> TileMoveBlocked;

        protected override void Awake()
        {
            base.Awake();
            // Also runs in a build: serialized data can be stale (a grid resized without the
            // Inspector ever reserialising the list), and a mismatched target order would make the
            // puzzle unsolvable rather than merely wrong.
            EnsureTargetOrder();
            _currentOrder = Shuffle(new System.Random(ResolveVariantSeed()));
        }

#if UNITY_EDITOR
        /// <summary>
        /// Keeps the solved arrangement consistent with the grid, so changing Columns/Rows/Hole Cell
        /// in the Inspector is enough to reshape the puzzle without hand-editing a list and risking a
        /// wrong count or a missing empty slot.
        /// </summary>
        private void OnValidate()
        {
            _holeCell.x = Mathf.Clamp(_holeCell.x, 0, Mathf.Max(1, _columns) - 1);
            _holeCell.y = Mathf.Clamp(_holeCell.y, 0, Mathf.Max(1, _rows) - 1);
            EnsureTargetOrder();
        }
#endif

        /// <summary>Slides the tile with the given id into the adjacent empty slot, if that move is legal. Returns whether the move was applied.</summary>
        public bool TryMoveTile(string tileId)
        {
            if (IsSolved || _currentOrder == null) return false;

            int tileIndex = _currentOrder.IndexOf(tileId);
            int emptyIndex = IndexOfHole(_currentOrder);
            if (tileIndex < 0 || emptyIndex < 0 || !AreAdjacent(tileIndex, emptyIndex))
            {
                TileMoveBlocked?.Invoke(tileId);
                return false;
            }

            SetInProgress();
            (_currentOrder[tileIndex], _currentOrder[emptyIndex]) = (_currentOrder[emptyIndex], _currentOrder[tileIndex]);
            TileMoved?.Invoke(tileId);
            CheckSolved();
            return true;
        }

        public string GetTileAt(int index) =>
            _currentOrder != null && index >= 0 && index < _currentOrder.Count ? _currentOrder[index] : null;

        /// <summary>Row-major index of the cell a tile occupies when the puzzle is solved — which fragment of the board's image it carries. -1 if the id is not on this board.</summary>
        public int GetSolvedIndex(string tileId)
        {
            for (int i = 0; i < _targetOrder.Count; i++)
                if (_targetOrder[i] == tileId) return i;
            return -1;
        }

        // ── Target arrangement ───────────────────────────────────────────────

        /// <summary>
        /// Guarantees _targetOrder describes a playable board. Regenerates it from the grid unless
        /// the designer opted into authoring it, in which case a broken hand-authored list is
        /// reported and replaced rather than silently producing an unsolvable puzzle.
        /// </summary>
        private void EnsureTargetOrder()
        {
            if (_targetOrder == null) _targetOrder = new List<string>();
            int cells = Mathf.Max(1, _columns) * Mathf.Max(1, _rows);

            if (_customTargetOrder)
            {
                if (_targetOrder.Count == cells && CountHoles(_targetOrder) == 1) return;
                if (_targetOrder.Count > 0)
                    Debug.LogWarning($"[SlidingPuzzle:{name}] Custom Target Order needs {cells} entries with exactly one empty slot, "
                        + $"but has {_targetOrder.Count} with {CountHoles(_targetOrder)} empty. Regenerated from the grid.", this);
            }

            BuildDefaultTargetOrder();
        }

        private void BuildDefaultTargetOrder()
        {
            int columns = Mathf.Max(1, _columns);
            int rows = Mathf.Max(1, _rows);
            int hole = Mathf.Clamp(_holeCell.y, 0, rows - 1) * columns + Mathf.Clamp(_holeCell.x, 0, columns - 1);

            _targetOrder.Clear();
            int nextId = 1;
            for (int i = 0; i < columns * rows; i++)
                _targetOrder.Add(i == hole ? string.Empty : (nextId++).ToString());
        }

        private static int IndexOfHole(List<string> order)
        {
            for (int i = 0; i < order.Count; i++)
                if (string.IsNullOrEmpty(order[i])) return i;
            return -1;
        }

        private static int CountHoles(List<string> order)
        {
            int count = 0;
            for (int i = 0; i < order.Count; i++)
                if (string.IsNullOrEmpty(order[i])) count++;
            return count;
        }

        // ── Grid maths ───────────────────────────────────────────────────────

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
            int emptyIndex = IndexOfHole(order);
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
