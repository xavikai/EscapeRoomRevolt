using UnityEngine;
using UnityEngine.Events;
using EscapeRoomRevolt.Player.VR;
using EscapeRoomRevolt.Systems.Interaction;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>
    /// A trigger socket for the physical piece-placement puzzle: connects the PlacementPuzzle when a
    /// GrabbablePiece is dropped inside (not currently held) and disconnects it again once picked
    /// back up and carried away. Mirrors PhysicsSocket's held-check, keyed by piece id instead of
    /// an inventory item, since a piece is a puzzle prop, not something that belongs in the inventory.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class PieceSocketReceiver : MonoBehaviour
    {
        [SerializeField] private PlacementPuzzle _puzzle;
        [SerializeField] private string _socketId;

        [Header("Magnet snap")]
        [Tooltip("Where a captured piece is pulled to. If null, uses this transform.")]
        [SerializeField] private Transform _snapPoint;
        [Tooltip("How fast a captured piece is pulled/rotated into the exact snap position, per second.")]
        [SerializeField] private float _snapSpeed = 6f;

        [Header("Visual feedback (optional, uses this object's own Renderer)")]
        [SerializeField] private Color _emptyColor = Color.grey;
        [SerializeField] private Color _occupiedColor = Color.green;
        [SerializeField] private Color _solvedColor = Color.cyan;

        [Header("Events")]
        [Tooltip("A piece has just settled into this socket. Mechanical feedback belongs here — the "
               + "clunk, the piece sinking into its holder — not the payoff: this fires for a wrong "
               + "piece too, and lighting it up would hand the player the answer socket by socket.")]
        public UnityEvent OnPieceSeated;
        [Tooltip("The piece that was in this socket is gone — pulled back out, or placed elsewhere. Undo whatever OnPieceSeated did.")]
        public UnityEvent OnPieceRemoved;
        [Tooltip("The puzzle is solved and this socket's piece is locked in for good. This is the payoff: the fuse lights up, the model swaps, the animation plays.")]
        public UnityEvent OnPieceLocked;
        [Tooltip("Same end state as OnPieceLocked, but restored from a save rather than earned just now. Set the final look here with no fanfare — no sound, no animation, no camera-grabbing flourish at load time.")]
        public UnityEvent OnLockRestored;

        private enum SocketVisual { Empty, Occupied, Solved }

        private Renderer _renderer;
        private SelectionOutlineTarget _outlineTarget;
        private GrabbablePiece _seatedPiece;
        private bool _lockAttempted;
        private string _occupantPieceId;
        private SocketVisual? _appliedVisual;

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
            _renderer = GetComponent<Renderer>();

            _outlineTarget = GetComponent<SelectionOutlineTarget>();
            if (_outlineTarget == null) _outlineTarget = gameObject.AddComponent<SelectionOutlineTarget>();

            if (_puzzle != null) _puzzle.OnSolvedEvent.AddListener(HandleSolved);
            UpdateColor();
        }

        private void OnDestroy()
        {
            if (_puzzle != null) _puzzle.OnSolvedEvent.RemoveListener(HandleSolved);
        }

        private void Update()
        {
            // Also covers being solved without the event firing here — a solved state restored from a
            // save, where the piece would otherwise sit back at its starting spot as if untouched.
            // Only a restore can reach this: a live solve runs through OnSolvedEvent inside Connect(),
            // which sets _lockAttempted before any Update sees it.
            if (_puzzle != null && _puzzle.IsSolved && !_lockAttempted) LockSeatedPiece(restored: true);

            if (_puzzle != null) SyncOccupancy(_puzzle.GetPieceAtSocket(_socketId));
            UpdateColor();
        }

        /// <summary>Fires the seated/removed edge when this socket's occupant changes, so a UnityEvent runs once per placement instead of every frame the piece rests here.</summary>
        private void SyncOccupancy(string occupantPieceId)
        {
            if (occupantPieceId == _occupantPieceId) return;
            _occupantPieceId = occupantPieceId;

            if (occupantPieceId != null) OnPieceSeated?.Invoke();
            else OnPieceRemoved?.Invoke();
        }

        private void HandleSolved() => LockSeatedPiece(restored: false);

        /// <summary>
        /// Seats this socket's piece for good. Until the mechanism fires the piece is a loose
        /// rigidbody the magnet holds in position each physics step; once it fires, that magnet stops
        /// (the puzzle is solved, so OnTriggerStay bails out) and gravity would pull the piece
        /// straight back out. A fuse feeding a live circuit stays in its holder and can't be pulled.
        /// </summary>
        private void LockSeatedPiece(bool restored)
        {
            _lockAttempted = true;

            if (_seatedPiece == null) _seatedPiece = FindPiece(_puzzle.GetPieceAtSocket(_socketId));

            // A restored save never played the drop, so this socket's occupancy was never observed
            // changing. Adopt it silently, or the sync below would fire a "just placed" clunk for a
            // piece that has been sitting there since before the player loaded the game.
            if (restored) _occupantPieceId = _puzzle.GetPieceAtSocket(_socketId);

            if (_seatedPiece == null) return;

            _seatedPiece.LockInPlace(_snapPoint != null ? _snapPoint : transform);
            _outlineTarget?.SetHighlighted(false);
            UpdateColor();

            if (restored) OnLockRestored?.Invoke();
            else OnPieceLocked?.Invoke();
        }

        /// <summary>Locates a piece by id, for the restored-from-save case where nothing was ever dropped in this socket during the session.</summary>
        private static GrabbablePiece FindPiece(string pieceId)
        {
            if (string.IsNullOrEmpty(pieceId)) return null;
            foreach (GrabbablePiece candidate in FindObjectsByType<GrabbablePiece>(FindObjectsSortMode.None))
                if (candidate.PieceId == pieceId) return candidate;
            return null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<GrabbablePiece>() != null)
                _outlineTarget?.SetHighlighted(true);
        }

        private void OnTriggerStay(Collider other)
        {
            if (_puzzle == null || _puzzle.IsSolved) return;

            GrabbablePiece piece = other.GetComponentInParent<GrabbablePiece>();
            if (piece == null) return;

            if (IsCurrentlyHeld(piece.GetComponent<PhysicsGrabbable>())) return;

            // Remember who is sitting here before connecting: this very Connect can be the one that
            // completes the puzzle, and the solved handler needs to know which piece to lock down.
            _seatedPiece = piece;

            // Register the connection once, not every physics step — re-submitting an already-wrong
            // placement every tick while it rests here would re-trigger Fail() (and its sanity
            // penalty) continuously instead of once.
            if (_puzzle.GetConnectedSocket(piece.PieceId) != _socketId)
            {
                // Announce the seating before connecting, not after: this Connect can be the one that
                // solves the puzzle, and a piece must read as placed before it reads as locked in.
                SyncOccupancy(piece.PieceId);
                _puzzle.Connect(piece.PieceId, _socketId);
            }

            // That Connect may have solved the puzzle and locked the piece in place already; the
            // magnet below has nothing left to do on a frozen body.
            if (_puzzle.IsSolved) return;

            // The piece is now seated — the green color already shows that, so the "come here" outline
            // would just be noise from this point on. It comes back if the piece is pulled back out.
            _outlineTarget?.SetHighlighted(false);

            // Magnet pull: as long as the piece is resting inside the capture zone and isn't held,
            // ease it toward the exact snap position/rotation every step. This is what makes an
            // imprecise drop still land cleanly instead of requiring a pixel-perfect release, and
            // what keeps it sitting steady afterward (each step corrects any gravity drift).
            Transform snap = _snapPoint != null ? _snapPoint : transform;
            Rigidbody rb = piece.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = Vector3.MoveTowards(rb.position, snap.position, _snapSpeed * Time.fixedDeltaTime);
                rb.rotation = Quaternion.RotateTowards(rb.rotation, snap.rotation, _snapSpeed * 90f * Time.fixedDeltaTime);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            GrabbablePiece piece = other.GetComponentInParent<GrabbablePiece>();
            if (piece == null) return;

            _outlineTarget?.SetHighlighted(false);

            if (_puzzle == null || _puzzle.IsSolved) return;

            if (_seatedPiece == piece) _seatedPiece = null;

            if (_puzzle.GetConnectedSocket(piece.PieceId) == _socketId)
                _puzzle.Disconnect(piece.PieceId);
        }

        /// <summary>True while the player is currently carrying this specific piece — on PC via PhysicsGrabber, or in VR via an XR select — so a piece still in-hand inside the trigger doesn't count as "placed" yet.</summary>
        private static bool IsCurrentlyHeld(PhysicsGrabbable grabbable)
        {
            if (grabbable == null) return false;
            if (PhysicsGrabber.Instance != null && PhysicsGrabber.Instance.CurrentHeldObject == grabbable) return true;
            VRInteractionBridge bridge = grabbable.GetComponent<VRInteractionBridge>();
            return bridge != null && bridge.IsSelected;
        }

        /// <summary>Repaints the socket only when its state actually changes — writing to .material every frame instances the material and burns a draw call's worth of work to set the same colour it already had.</summary>
        private void UpdateColor()
        {
            if (_renderer == null || _puzzle == null) return;

            SocketVisual desired = _puzzle.IsSolved ? SocketVisual.Solved
                                 : _occupantPieceId != null ? SocketVisual.Occupied
                                 : SocketVisual.Empty;
            if (_appliedVisual == desired) return;
            _appliedVisual = desired;

            _renderer.material.color = desired switch
            {
                SocketVisual.Solved => _solvedColor,
                SocketVisual.Occupied => _occupiedColor,
                _ => _emptyColor,
            };
        }
    }
}
