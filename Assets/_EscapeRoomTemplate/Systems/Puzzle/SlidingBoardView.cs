using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>
    /// Gives a SlidingPuzzle a body: lays its tiles out on the real grid and slides each one to the
    /// cell it currently occupies. The puzzle itself only tracks the arrangement and says nothing
    /// about presentation, so without a view like this the tiles never appear to move and the board
    /// is unreadable.
    ///
    /// The board owns the geometry: Cell Size is the only measurement the designer picks, and tile
    /// size follows from it via Tile Fill. Authoring the tile scale by hand alongside a separate
    /// spacing value is how a board ends up with tiles that overlap or float in a grid they no
    /// longer match.
    ///
    /// Assigning a Source Image cuts that texture into one fragment per tile, so the solved
    /// arrangement is the reassembled picture — the player can see what "correct" means, and nobody
    /// has to author the answer as a list.
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
        [Tooltip("Centre of the board. If null, this transform is used. Moving, rotating or scaling it moves the whole puzzle.")]
        [SerializeField] private Transform _origin;
        [SerializeField] private List<TileBinding> _tiles = new List<TileBinding>();
        [Tooltip("Optional marker that follows the open slot, so the gap reads as part of the board.")]
        [SerializeField] private Transform _emptyMarker;

        [Header("Layout")]
        [Tooltip("Distance between the centres of two neighbouring cells. This is the only size you set: the tiles are scaled to fit it.")]
        [FormerlySerializedAs("_spacing")]
        [SerializeField, Min(.01f)] private float _cellSize = .46f;
        [Tooltip("How much of its cell a tile fills. 1 = tiles touch, lower values open the gap that makes the grid readable.")]
        [SerializeField, Range(.1f, 1f)] private float _tileFill = .74f;
        [Tooltip("How deep the tiles are, along the board's forward axis.")]
        [SerializeField, Min(.001f)] private float _thickness = .12f;
        [Tooltip("Let the board scale the tiles to fit the cell. Turn off only for tiles whose size is part of their art (a sculpted fragment, a prop).")]
        [SerializeField] private bool _driveTileScale = true;

        [Header("Motion")]
        [Tooltip("Seconds a tile takes to cross one cell. Set as a duration rather than a speed so the slide feels the same on a small board and a large one.")]
        [SerializeField, Min(.01f)] private float _moveDuration = .12f;
        [Tooltip("How far a tile twitches when the player clicks it and it cannot move.")]
        [SerializeField, Min(0f)] private float _blockedNudgeDistance = .015f;
        [SerializeField, Min(0f)] private float _blockedNudgeDuration = .18f;

        [Header("Image")]
        [Tooltip("Cut into one fragment per tile, so solving the puzzle reassembles the picture. Leave empty to keep whatever material the tiles already have.")]
        [SerializeField] private Texture2D _sourceImage;

        [Header("Authoring")]
        [Tooltip("Used as the tile when the board is rebuilt. Leave empty to generate plain cubes, which is enough for an image board since the picture is what the player reads.")]
        [SerializeField] private GameObject _tilePrefab;

        [Header("Audio")]
        [SerializeField] private AudioClip _slideSound;
        [Tooltip("Played when the player clicks a tile that cannot move — most clicks on a sliding puzzle are illegal, and silence reads as a broken game.")]
        [SerializeField] private AudioClip _blockedSound;

        private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
        private static readonly int BaseMapStId = Shader.PropertyToID("_BaseMap_ST");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int MainTexStId = Shader.PropertyToID("_MainTex_ST");

        private MaterialPropertyBlock _propertyBlock;
        private string _nudgedTileId;
        private float _nudgeTimeLeft;

        /// <summary>
        /// A piece as this board needs to see it: the logic node it moves, the art it sizes, and the
        /// interaction volume it keeps matching the cell. Cached because resolving the art means a
        /// name lookup, and this runs every frame.
        /// </summary>
        private sealed class PieceParts
        {
            public Transform visual;
            public BoxCollider collider;
            public Renderer[] renderers;
        }

        private readonly Dictionary<Transform, PieceParts> _pieceCache = new Dictionary<Transform, PieceParts>();

        public SlidingPuzzle Puzzle => _puzzle;
        /// <summary>Read by the editor-side board builder; the board itself never instantiates anything at runtime.</summary>
        public GameObject TilePrefab => _tilePrefab;
        public Transform BoardOrigin => Origin;
        public float CellSize => _cellSize;
        public float Thickness => _thickness;
        public int TileCount => _tiles.Count;
        /// <summary>The side of a tile, derived from the cell rather than authored — the board owns its own geometry.</summary>
        public float TileSize => _cellSize * _tileFill;

        private Transform Origin => _origin != null ? _origin : transform;

        private void OnEnable()
        {
            // Material property blocks do not survive a domain reload or a scene open, so the image
            // has to be re-cut every time this component wakes up, edit mode included.
            InvalidatePieces();
            ApplyImage();
            SnapAll();

            if (_puzzle == null) return;
            _puzzle.TileMoved += HandleTileMoved;
            _puzzle.TileMoveBlocked += HandleTileMoveBlocked;
        }

        private void OnDisable()
        {
            if (_puzzle == null) return;
            _puzzle.TileMoved -= HandleTileMoved;
            _puzzle.TileMoveBlocked -= HandleTileMoveBlocked;
        }

        private void Start() => SnapAll();

        private void Update()
        {
            // Outside play mode there is no animation to run and no CurrentOrder to follow: just keep
            // the board showing the solved arrangement so it can be positioned and eyeballed.
            bool animate = Application.isPlaying;
            if (animate && _nudgeTimeLeft > 0f) _nudgeTimeLeft -= Time.deltaTime;
            ApplyLayout(animate);
        }

        /// <summary>Places every tile on its cell immediately, with no sliding — used at startup so the shuffled board is correct on the first frame.</summary>
        public void SnapAll() => ApplyLayout(false);

        private void ApplyLayout(bool animate)
        {
            if (_puzzle == null) return;

            // One cell per _moveDuration, whatever the board measures.
            float speed = _cellSize / Mathf.Max(.01f, _moveDuration);
            Vector3 tileScale = new Vector3(TileSize, TileSize, _thickness);

            foreach (TileBinding binding in _tiles)
            {
                if (binding.tile == null) continue;
                if (!TryGetSlotPosition(binding.tileId, out Vector3 target)) continue;

                if (animate && _nudgeTimeLeft > 0f && binding.tileId == _nudgedTileId) target += CurrentNudgeOffset();

                binding.tile.position = animate
                    ? Vector3.MoveTowards(binding.tile.position, target, speed * Time.deltaTime)
                    : target;
                if (_driveTileScale) ApplyPieceSize(binding.tile, tileScale);
            }

            if (_emptyMarker != null && TryGetSlotPosition(string.Empty, out Vector3 emptyTarget))
            {
                _emptyMarker.position = emptyTarget;
                // Shallower than a tile, so the open slot reads as a recess rather than a black tile.
                if (_driveTileScale) ApplyPieceSize(_emptyMarker, new Vector3(TileSize, TileSize, _thickness * .4f));
            }
        }

        /// <summary>
        /// Sizes a piece to its cell without touching its logic node: the placeholder art is scaled
        /// and the interaction collider resized to match. A modelled mesh dropped in through
        /// ReplaceableModelSlot therefore keeps the proportions it was built with, instead of being
        /// squashed into a square by a scale meant for a stand-in cube. Pieces built before the
        /// Logic/Visuals split have their own transform scaled, exactly as they always did.
        /// </summary>
        private void ApplyPieceSize(Transform piece, Vector3 size)
        {
            PieceParts parts = ResolveParts(piece);

            if (parts.visual == piece)
            {
                piece.localScale = size;
                return;
            }

            parts.visual.localScale = size;
            if (parts.collider != null) parts.collider.size = size;
        }

        private PieceParts ResolveParts(Transform piece)
        {
            if (_pieceCache.TryGetValue(piece, out PieceParts cached) && cached.visual != null) return cached;

            Transform named = piece.Find(piece.name.Replace("_Logic", "") + "_Visuals");
            var parts = new PieceParts
            {
                // Falls back to the piece itself, which is what a board authored before the split has.
                visual = named != null ? named : piece,
                collider = piece.GetComponent<BoxCollider>(),
                renderers = piece.GetComponentsInChildren<Renderer>(),
            };
            _pieceCache[piece] = parts;
            return parts;
        }

        /// <summary>Drops the cached art lookups, so renaming, restructuring or swapping a model is picked up.</summary>
        public void InvalidatePieces() => _pieceCache.Clear();

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

            position = Origin.TransformPoint(CellOffset(index % _puzzle.Columns, index / _puzzle.Columns));
            return true;
        }

        /// <summary>Column 0 on the left, row 0 on top, both centred on the origin. Local to the origin, so the board can be moved, turned and scaled as one object.</summary>
        private Vector3 CellOffset(int column, int row) => new Vector3(
            (column - (_puzzle.Columns - 1) * .5f) * _cellSize,
            ((_puzzle.Rows - 1) * .5f - row) * _cellSize,
            0f);

        // ── Feedback ─────────────────────────────────────────────────────────

        private void HandleTileMoved(string tileId)
        {
            if (_slideSound == null) return;
            Vector3 position = TryGetSlotPosition(tileId, out Vector3 slot) ? slot : Origin.position;
            EscapeRoomRevolt.Systems.Audio.AudioManager.Instance?.PlaySoundAt(_slideSound, position, 1f, .06f);
        }

        private void HandleTileMoveBlocked(string tileId)
        {
            _nudgedTileId = tileId;
            _nudgeTimeLeft = _blockedNudgeDuration;

            if (_blockedSound == null) return;
            Vector3 position = TryGetSlotPosition(tileId, out Vector3 slot) ? slot : Origin.position;
            EscapeRoomRevolt.Systems.Audio.AudioManager.Instance?.PlaySoundAt(_blockedSound, position, .8f, .06f);
        }

        /// <summary>A short damped twitch along the board's own right axis, so it reads as "stuck" rather than as a missed click.</summary>
        private Vector3 CurrentNudgeOffset()
        {
            float decay = Mathf.Clamp01(_nudgeTimeLeft / Mathf.Max(.01f, _blockedNudgeDuration));
            float elapsed = _blockedNudgeDuration - _nudgeTimeLeft;
            return Origin.right * (Mathf.Sin(elapsed * 90f) * _blockedNudgeDistance * decay);
        }

        // ── Image fragments ──────────────────────────────────────────────────

        /// <summary>
        /// Gives each tile the piece of Source Image belonging to the cell it occupies when solved,
        /// which is what makes the correct arrangement readable to the player. Uses a property block
        /// rather than per-tile material instances so nothing is leaked in edit mode.
        /// </summary>
        public void ApplyImage()
        {
            if (_puzzle == null) return;
            _propertyBlock ??= new MaterialPropertyBlock();

            int columns = Mathf.Max(1, _puzzle.Columns);
            int rows = Mathf.Max(1, _puzzle.Rows);

            foreach (TileBinding binding in _tiles)
            {
                if (binding.tile == null) continue;
                // Whatever is actually visible: the placeholder, or the model that replaced it. The
                // renderer is not on the logic node once a piece follows the Logic/Visuals split.
                Renderer[] renderers = ResolveParts(binding.tile).renderers;
                if (renderers == null || renderers.Length == 0) continue;

                if (_sourceImage == null)
                {
                    foreach (Renderer renderer in renderers)
                        if (renderer != null) renderer.SetPropertyBlock(null);
                    continue;
                }

                int index = _puzzle.GetSolvedIndex(binding.tileId);
                if (index < 0) continue;

                int column = index % columns;
                int row = index / columns;
                // UVs run bottom-up while rows run top-down, hence the flipped row.
                Vector4 scaleOffset = new Vector4(1f / columns, 1f / rows,
                    (float)column / columns, (float)(rows - 1 - row) / rows);

                foreach (Renderer renderer in renderers)
                {
                    if (renderer == null) continue;
                    renderer.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetTexture(BaseMapId, _sourceImage);
                    _propertyBlock.SetVector(BaseMapStId, scaleOffset);
                    _propertyBlock.SetTexture(MainTexId, _sourceImage);
                    _propertyBlock.SetVector(MainTexStId, scaleOffset);
                    // The creator paints tiles in flat colours; left alone they would tint the fragment.
                    _propertyBlock.SetColor(BaseColorId, Color.white);
                    renderer.SetPropertyBlock(_propertyBlock);
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Deferred: OnValidate runs mid-serialisation, where touching renderers and transforms
            // is unsafe.
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this == null) return;
                InvalidatePieces();
                ApplyImage();
                SnapAll();
            };
        }

        /// <summary>Board geometry at a glance while positioning it, including the cells that have no tile yet.</summary>
        private void OnDrawGizmosSelected()
        {
            if (_puzzle == null) return;
            Gizmos.color = new Color(.35f, .8f, 1f, .5f);
            for (int row = 0; row < _puzzle.Rows; row++)
                for (int column = 0; column < _puzzle.Columns; column++)
                {
                    Gizmos.matrix = Matrix4x4.TRS(Origin.TransformPoint(CellOffset(column, row)), Origin.rotation, Origin.lossyScale);
                    Gizmos.DrawWireCube(Vector3.zero, new Vector3(TileSize, TileSize, _thickness));
                }
            Gizmos.matrix = Matrix4x4.identity;
        }
#endif
    }
}
