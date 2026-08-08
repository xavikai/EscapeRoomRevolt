using System;
using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>
    /// Gives a SlidingPuzzle a body: lays its tiles out on the real grid and slides each one to the
    /// cell it currently occupies. The puzzle itself only tracks the arrangement and says nothing
    /// about presentation, so without a view like this the tiles never appear to move and the board
    /// is unreadable.
    ///
    /// Runs in edit mode too, showing the solved arrangement, so the board is authored as the grid
    /// it will actually be rather than as a row of loose cubes.
    /// </summary>
    [ExecuteAlways]
    public sealed class SlidingBoardView : MonoBehaviour
    {
        [Serializable]
        public sealed class TileBinding
        {
            [Tooltip("Must match the tile id used in the puzzle's Target Order.")]
            public string tileId;
            public Transform tile;
        }

        [SerializeField] private SlidingPuzzle _puzzle;
        [Tooltip("Centre of the board. If null, this transform is used.")]
        [SerializeField] private Transform _origin;
        [SerializeField] private List<TileBinding> _tiles = new List<TileBinding>();
        [Tooltip("Optional marker that follows the open slot, so the gap reads as part of the board.")]
        [SerializeField] private Transform _emptyMarker;

        [Header("Layout")]
        [SerializeField, Min(.05f)] private float _spacing = .6f;
        [Tooltip("How fast a tile slides to its cell, in units per second. Edit mode always snaps.")]
        [SerializeField, Min(.1f)] private float _slideSpeed = 3.5f;

        public SlidingPuzzle Puzzle => _puzzle;
        private Transform Origin => _origin != null ? _origin : transform;

        private void Start() => SnapAll();

        private void Update()
        {
            if (_puzzle == null) return;

            // Outside play mode there is no animation to run and no CurrentOrder to follow: just keep
            // the board showing the solved arrangement so it can be positioned and eyeballed.
            bool animate = Application.isPlaying;

            foreach (TileBinding binding in _tiles)
            {
                if (binding.tile == null) continue;
                if (!TryGetSlotPosition(binding.tileId, out Vector3 target)) continue;
                binding.tile.position = animate
                    ? Vector3.MoveTowards(binding.tile.position, target, _slideSpeed * Time.deltaTime)
                    : target;
            }

            if (_emptyMarker != null && TryGetSlotPosition(string.Empty, out Vector3 emptyTarget))
                _emptyMarker.position = emptyTarget;
        }

        /// <summary>Places every tile on its cell immediately, with no sliding — used at startup so the shuffled board is correct on the first frame.</summary>
        public void SnapAll()
        {
            if (_puzzle == null) return;

            foreach (TileBinding binding in _tiles)
            {
                if (binding.tile == null) continue;
                if (TryGetSlotPosition(binding.tileId, out Vector3 target)) binding.tile.position = target;
            }

            if (_emptyMarker != null && TryGetSlotPosition(string.Empty, out Vector3 emptyTarget))
                _emptyMarker.position = emptyTarget;
        }

        /// <summary>The arrangement to display: the live one while playing, the solved one in the editor.</summary>
        private IReadOnlyList<string> ActiveOrder
        {
            get
            {
                IReadOnlyList<string> live = _puzzle.CurrentOrder;
                return live != null && live.Count > 0 ? live : _puzzle.TargetOrder;
            }
        }

        private bool TryGetSlotPosition(string tileId, out Vector3 position)
        {
            position = default;
            IReadOnlyList<string> order = ActiveOrder;
            if (order == null) return false;

            int index = -1;
            for (int i = 0; i < order.Count; i++)
                if (order[i] == tileId) { index = i; break; }
            if (index < 0) return false;

            int column = index % _puzzle.Columns;
            int row = index / _puzzle.Columns;

            // Column 0 on the left, row 0 on top, both centred on the origin.
            float x = (column - (_puzzle.Columns - 1) * .5f) * _spacing;
            float y = ((_puzzle.Rows - 1) * .5f - row) * _spacing;

            Transform o = Origin;
            position = o.position + o.right * x + o.up * y;
            return true;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Recreates one tile per non-empty cell by cloning the first bound tile, so changing the
        /// puzzle's Columns/Rows is all it takes to reshape the board. Existing tiles are reused;
        /// surplus ones are removed. Run from the component's context menu.
        /// </summary>
        [ContextMenu("Rebuild tiles for grid size")]
        private void RebuildTiles()
        {
            if (_puzzle == null) { Debug.LogWarning("[SlidingBoardView] No puzzle assigned."); return; }

            Transform template = null;
            foreach (TileBinding b in _tiles) if (b.tile != null) { template = b.tile; break; }
            if (template == null) { Debug.LogWarning("[SlidingBoardView] Needs at least one existing tile to clone."); return; }

            int needed = _puzzle.Columns * _puzzle.Rows - 1;
            var rebuilt = new List<TileBinding>(needed);

            for (int i = 0; i < needed; i++)
            {
                string id = (i + 1).ToString();
                Transform tile = i < _tiles.Count && _tiles[i].tile != null ? _tiles[i].tile : null;

                if (tile == null)
                {
                    GameObject clone = UnityEditor.PrefabUtility.IsPartOfAnyPrefab(template.gameObject)
                        ? (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(template.gameObject, template.parent)
                        : Instantiate(template.gameObject, template.parent);
                    UnityEditor.Undo.RegisterCreatedObjectUndo(clone, "Rebuild sliding tiles");
                    tile = clone.transform;
                }

                tile.gameObject.name = "Tile" + id + "_Logic";

                var button = tile.GetComponent<SlidingTileButton>();
                if (button != null)
                {
                    var so = new UnityEditor.SerializedObject(button);
                    so.FindProperty("_puzzle").objectReferenceValue = _puzzle;
                    so.FindProperty("_tileId").stringValue = id;
                    so.ApplyModifiedProperties();
                }

                rebuilt.Add(new TileBinding { tileId = id, tile = tile });
            }

            // Drop any tiles the smaller grid no longer needs.
            for (int i = needed; i < _tiles.Count; i++)
                if (_tiles[i].tile != null) UnityEditor.Undo.DestroyObjectImmediate(_tiles[i].tile.gameObject);

            _tiles = rebuilt;
            UnityEditor.EditorUtility.SetDirty(this);
            SnapAll();
            Debug.Log($"[SlidingBoardView] Board rebuilt for {_puzzle.Columns}x{_puzzle.Rows}: {needed} tiles.");
        }
#endif
    }
}
