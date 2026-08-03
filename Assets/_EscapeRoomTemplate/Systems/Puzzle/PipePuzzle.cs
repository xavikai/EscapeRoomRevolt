using System;
using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    [Flags]
    public enum PipeSide { None = 0, North = 1, East = 2, South = 4, West = 8 }

    [Serializable]
    public sealed class PipeTileDefinition
    {
        [Tooltip("Id of this pipe segment (matches whatever calls RotateTile(tileId)).")]
        public string tileId = "pipe";
        [Min(0)] public int row;
        [Min(0)] public int column;
        [Tooltip("Which sides have an opening before any rotation (e.g. North|South for a straight piece, North|East for an elbow).")]
        public PipeSide openSides = PipeSide.North | PipeSide.South;
        [Tooltip("Starting rotation in 90° clockwise steps (0-3). Ignored if the puzzle randomizes rotations.")]
        [Range(0, 3)] public int startingRotationSteps;
    }

    /// <summary>
    /// Connect-the-pipes puzzle: a grid of rotatable pipe segments, solved when rotating them
    /// creates a continuous path of matching openings from the source tile to the sink tile.
    /// Scene interactables (a clickable segment, a VR grab-and-twist, anything) call
    /// RotateTile(tileId) to turn a segment 90° clockwise; this controller only tracks grid state
    /// and connectivity, not how tiles are rendered or rotated visually.
    /// </summary>
    public class PipePuzzle : PuzzleController
    {
        private static readonly Dictionary<PipeSide, PipeSide> OppositeSide = new Dictionary<PipeSide, PipeSide>
        {
            { PipeSide.North, PipeSide.South },
            { PipeSide.South, PipeSide.North },
            { PipeSide.East, PipeSide.West },
            { PipeSide.West, PipeSide.East },
        };

        [Header("Grid")]
        [SerializeField] private List<PipeTileDefinition> _tiles = new List<PipeTileDefinition>();
        [SerializeField] private string _sourceTileId = "";
        [SerializeField] private string _sinkTileId = "";
        [Tooltip("Randomizes each tile's starting rotation (seeded from SaveManager.RunSeed) instead of using the authored startingRotationSteps, re-rolling once if that would spawn already solved.")]
        [SerializeField] private bool _randomizeRotations;

        private readonly Dictionary<string, int> _rotationSteps = new Dictionary<string, int>();

        public IReadOnlyList<PipeTileDefinition> Tiles => _tiles;

        protected override void Awake()
        {
            base.Awake();
            InitializeRotations();
        }

        /// <summary>Rotates a segment 90° clockwise and re-checks the path. No-ops once solved.</summary>
        public bool RotateTile(string tileId)
        {
            if (IsSolved) return false;
            PipeTileDefinition tile = FindTile(tileId);
            if (tile == null) return false;

            SetInProgress();
            _rotationSteps[tileId] = (GetRotationSteps(tileId) + 1) % 4;
            if (IsPathConnected()) Solve();
            return true;
        }

        public int GetRotationSteps(string tileId) => _rotationSteps.TryGetValue(tileId, out int steps) ? steps : 0;

        public PipeSide GetEffectiveOpenSides(string tileId)
        {
            PipeTileDefinition tile = FindTile(tileId);
            return tile == null ? PipeSide.None : RotateClockwise(tile.openSides, GetRotationSteps(tileId));
        }

        /// <summary>Breadth-first search from the source tile through matching openings to the sink tile. Safe to call anytime, including for a "check my progress" hint UI.</summary>
        public bool IsPathConnected()
        {
            PipeTileDefinition source = FindTile(_sourceTileId);
            PipeTileDefinition sink = FindTile(_sinkTileId);
            if (source == null || sink == null) return false;
            if (source.tileId == sink.tileId) return true;

            var visited = new HashSet<string> { source.tileId };
            var queue = new Queue<PipeTileDefinition>();
            queue.Enqueue(source);

            while (queue.Count > 0)
            {
                PipeTileDefinition current = queue.Dequeue();
                PipeSide currentOpen = GetEffectiveOpenSides(current.tileId);

                foreach (PipeTileDefinition neighbor in GetAdjacentTiles(current))
                {
                    if (visited.Contains(neighbor.tileId)) continue;
                    PipeSide directionToNeighbor = DirectionBetween(current, neighbor);
                    if ((currentOpen & directionToNeighbor) == 0) continue;
                    if ((GetEffectiveOpenSides(neighbor.tileId) & OppositeSide[directionToNeighbor]) == 0) continue;

                    if (neighbor.tileId == sink.tileId) return true;
                    visited.Add(neighbor.tileId);
                    queue.Enqueue(neighbor);
                }
            }
            return false;
        }

        protected override void OnPuzzleReset() => InitializeRotations();

        private void InitializeRotations()
        {
            _rotationSteps.Clear();
            if (!_randomizeRotations)
            {
                foreach (PipeTileDefinition tile in _tiles) _rotationSteps[tile.tileId] = tile.startingRotationSteps;
                return;
            }

            var random = new System.Random(ResolveVariantSeed());
            foreach (PipeTileDefinition tile in _tiles) _rotationSteps[tile.tileId] = random.Next(4);

            if (_tiles.Count > 0 && IsPathConnected())
            {
                PipeTileDefinition toNudge = _tiles.Find(t => t.tileId != _sourceTileId && t.tileId != _sinkTileId) ?? _tiles[0];
                _rotationSteps[toNudge.tileId] = (GetRotationSteps(toNudge.tileId) + 1) % 4;
            }
        }

        private PipeTileDefinition FindTile(string tileId) => _tiles.Find(t => t.tileId == tileId);

        private IEnumerable<PipeTileDefinition> GetAdjacentTiles(PipeTileDefinition tile)
        {
            foreach (PipeTileDefinition other in _tiles)
            {
                if (other == tile) continue;
                int rowDelta = other.row - tile.row, colDelta = other.column - tile.column;
                if ((rowDelta == 0 && Mathf.Abs(colDelta) == 1) || (colDelta == 0 && Mathf.Abs(rowDelta) == 1))
                    yield return other;
            }
        }

        private static PipeSide DirectionBetween(PipeTileDefinition from, PipeTileDefinition to)
        {
            if (to.row < from.row) return PipeSide.North;
            if (to.row > from.row) return PipeSide.South;
            return to.column > from.column ? PipeSide.East : PipeSide.West;
        }

        private static PipeSide RotateClockwise(PipeSide sides, int steps)
        {
            for (int i = 0; i < steps; i++)
            {
                PipeSide rotated = PipeSide.None;
                if ((sides & PipeSide.North) != 0) rotated |= PipeSide.East;
                if ((sides & PipeSide.East) != 0) rotated |= PipeSide.South;
                if ((sides & PipeSide.South) != 0) rotated |= PipeSide.West;
                if ((sides & PipeSide.West) != 0) rotated |= PipeSide.North;
                sides = rotated;
            }
            return sides;
        }

        [Serializable]
        private sealed class RotationEntry { public string tileId; public int steps; }

        [Serializable]
        private sealed class PipePuzzleSaveData
        {
            public int stateIndex;
            public List<RotationEntry> rotations = new List<RotationEntry>();
        }

        public override string SaveData()
        {
            var data = new PipePuzzleSaveData { stateIndex = (int)State };
            foreach (KeyValuePair<string, int> entry in _rotationSteps)
                data.rotations.Add(new RotationEntry { tileId = entry.Key, steps = entry.Value });
            return JsonUtility.ToJson(data);
        }

        public override void LoadData(string json)
        {
            base.LoadData(json);
            PipePuzzleSaveData data = JsonUtility.FromJson<PipePuzzleSaveData>(json);
            if (data?.rotations == null) return;
            _rotationSteps.Clear();
            foreach (RotationEntry entry in data.rotations) _rotationSteps[entry.tileId] = entry.steps;
        }
    }
}
