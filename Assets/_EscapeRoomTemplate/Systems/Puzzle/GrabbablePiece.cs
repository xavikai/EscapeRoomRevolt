using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using EscapeRoomRevolt.Player.VR;
using EscapeRoomRevolt.Systems.Interaction;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>
    /// Tags a physically grabbable prop as a specific puzzle piece, so a PieceSocketReceiver can
    /// identify it when the player carries it over and drops it inside the socket's trigger.
    ///
    /// Also brings the piece home if it ever ends up somewhere it can't be picked up again — under
    /// the floor, off the edge, wedged behind scenery. Without that, one bad throw or bounce leaves
    /// the puzzle permanently unsolvable with no way for the player to tell why.
    /// </summary>
    [RequireComponent(typeof(PhysicsGrabbable))]
    public sealed class GrabbablePiece : MonoBehaviour
    {
        [SerializeField] private string _pieceId;

        [Header("Recovery")]
        [Tooltip("Return the piece to where it started if it strays out of reach.")]
        [SerializeField] private bool _returnWhenLost = true;
        [Tooltip("Distance from its starting point beyond which the piece counts as lost.")]
        [SerializeField, Min(1f)] private float _maximumDistance = 12f;
        [Tooltip("Height below its starting point that counts as having fallen out of the level.")]
        [SerializeField, Min(.5f)] private float _fallDepth = 3f;

        [Header("Lock")]
        [Tooltip("Parent the piece to its socket once locked, so it travels with whatever the socket "
               + "is mounted on (a panel that swings open, a lift) instead of hanging in mid-air.")]
        [SerializeField] private bool _followSocketWhenLocked = true;

        public string PieceId => _pieceId;

        /// <summary>True once the piece has been seated for good and is no longer a loose object.</summary>
        public bool IsLocked { get; private set; }

        private PhysicsGrabbable _grabbable;
        private Rigidbody _body;
        private Vector3 _home;
        private Quaternion _homeRotation;

        private void Awake()
        {
            _grabbable = GetComponent<PhysicsGrabbable>();
            _body = GetComponent<Rigidbody>();
            _home = transform.position;
            _homeRotation = transform.rotation;
        }

        private void FixedUpdate()
        {
            if (IsLocked || !_returnWhenLost || IsHeld()) return;

            bool tooFar = Vector3.Distance(transform.position, _home) > _maximumDistance;
            bool fellThrough = transform.position.y < _home.y - _fallDepth;
            if (tooFar || fellThrough) ReturnHome();
        }

        /// <summary>
        /// Seats the piece permanently at <paramref name="anchor"/>: it stops being physics-driven,
        /// stops being grabbable on PC and in VR, and stops running its lost-piece recovery.
        ///
        /// This is what a fuse pushed home into a live holder should do. Leaving it a loose
        /// rigidbody after the mechanism fires means gravity drops it out of the socket the moment
        /// the socket stops magnet-holding it, and the player can still pull it back out of a
        /// circuit that is already running.
        /// </summary>
        public void LockInPlace(Transform anchor)
        {
            if (IsLocked) return;
            IsLocked = true;

            // If it is somehow still in the player's hand at this instant, take it out first —
            // otherwise the grabber keeps writing velocity to a body we are about to freeze.
            if (PhysicsGrabber.Instance != null && PhysicsGrabber.Instance.CurrentHeldObject == _grabbable)
                PhysicsGrabber.Instance.Drop();

            if (anchor != null) transform.SetPositionAndRotation(anchor.position, anchor.rotation);

            if (_body != null)
            {
                _body.linearVelocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
                _body.isKinematic = true;
            }

            if (anchor != null && _followSocketWhenLocked) transform.SetParent(anchor, true);

            // No longer something the player can reach for, on either platform: CanInteract off kills
            // the PC interact, the layer change keeps the crosshair ray from even finding it (so no
            // outline and no prompt), and disabling the XR interactable keeps VR hands off it.
            _grabbable.SetGrabbable(false);
            GetComponent<SelectionOutlineTarget>()?.SetHighlighted(false);

            int defaultLayer = LayerMask.NameToLayer("Default");
            if (defaultLayer >= 0) gameObject.layer = defaultLayer;

            XRBaseInteractable xrInteractable = GetComponent<XRBaseInteractable>();
            if (xrInteractable != null) xrInteractable.enabled = false;
        }

        /// <summary>Puts the piece back where it started, at rest. Safe to call from a UnityEvent as a manual "reset piece".</summary>
        public void ReturnHome()
        {
            if (IsLocked) return;
            if (_body != null)
            {
                _body.linearVelocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
                _body.position = _home;
                _body.rotation = _homeRotation;
            }
            transform.SetPositionAndRotation(_home, _homeRotation);
        }

        private bool IsHeld()
        {
            if (PhysicsGrabber.Instance != null && PhysicsGrabber.Instance.CurrentHeldObject == _grabbable) return true;
            VRInteractionBridge bridge = GetComponent<VRInteractionBridge>();
            return bridge != null && bridge.IsSelected;
        }
    }
}
