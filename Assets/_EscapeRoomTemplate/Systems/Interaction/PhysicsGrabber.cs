using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Core.Input;
using EscapeRoomRevolt.Player;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Interaction
{
    public class PhysicsGrabber : MonoBehaviour
    {
        public static PhysicsGrabber Instance { get; private set; }
        public bool IsHoldingObject => _currentHeldObject != null;
        public PhysicsGrabbable CurrentHeldObject => _currentHeldObject;
        [SerializeField] private float _holdDistance = 1.25f;
        [SerializeField] private float _pullSpeed = 15f;
        [SerializeField] private float _throwForce = 15f;
        [SerializeField] private float _autoDropDistance = 3.0f; 

        private PhysicsGrabbable _currentHeldObject;
        private Rigidbody _heldRigidbody;
        private Transform _holdPoint;
        private Collider[] _playerColliders;

        // State backups
        private float _originalAngularDamping;
        private float _originalLinearDamping;
        private bool _originalUseGravity;
        private CollisionDetectionMode _originalCollisionMode;
        private bool _justGrabbedThisFrame = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            _holdPoint = new GameObject("PhysicsHoldPoint").transform;
            _holdPoint.parent = transform;
            _holdPoint.localPosition = new Vector3(0, 0, _holdDistance);

            // Fetch player colliders to ignore them when holding objects
            _playerColliders = transform.root.GetComponentsInChildren<Collider>();
        }

        private void Update()
        {
            if (_currentHeldObject == null) return;

            if (GameplayBlockState.IsBlocking)
            {
                Drop();
                return;
            }

            if (_justGrabbedThisFrame)
            {
                _justGrabbedThisFrame = false;
                return;
            }

            InputRouter input = InputRouter.Instance;
            if (input == null) return;

            if (input.PrimaryActionPressed)
            {
                Throw();
                return;
            }

            if (input.DropHeldPressed)
            {
                Drop();
                return;
            }

            if (input.InteractPressed)
            {
                var pickable = _currentHeldObject.GetComponent<EscapeRoomRevolt.Systems.Inventory.PickableItem>();
                if (pickable != null)
                {
                    Drop();
                    pickable.Interact();
                }
                return;
            }

            if (input.SecondaryActionHeld)
            {
                PlayerPlatformRegistry.Current?.SetLookBlocked(true);

                Vector2 look = input.Look * 5f;
                Transform head = PlayerPlatformRegistry.Current?.Head;

                if (head != null)
                {
                    _heldRigidbody.transform.Rotate(head.up, -look.x, Space.World);
                    _heldRigidbody.transform.Rotate(head.right, look.y, Space.World);
                }
            }
            else
            {
                PlayerPlatformRegistry.Current?.SetLookBlocked(false);
            }
        }

        private void FixedUpdate()
        {
            if (_heldRigidbody == null) return;

            Vector3 direction = _holdPoint.position - _heldRigidbody.position;
            float distance = direction.magnitude;
            
            if (distance > _autoDropDistance)
            {
                Drop();
                return;
            }

            // Smoothly move towards point by scaling velocity based on distance
            _heldRigidbody.linearVelocity = direction * (_pullSpeed * distance);
        }

        public void Grab(PhysicsGrabbable grabbable)
        {
            if (_currentHeldObject != null) Drop();

            _currentHeldObject = grabbable;
            _heldRigidbody = grabbable.GetComponent<Rigidbody>();
            
            if (_heldRigidbody == null) return;

            // Backup physics state
            _originalAngularDamping = _heldRigidbody.angularDamping;
            _originalLinearDamping = _heldRigidbody.linearDamping;
            _originalUseGravity = _heldRigidbody.useGravity;
            _originalCollisionMode = _heldRigidbody.collisionDetectionMode;

            // Apply holding physics
            _heldRigidbody.angularDamping = 10f; // High angular drag stops crazy spinning
            _heldRigidbody.linearDamping = 5f; // Small drag helps stabilize
            _heldRigidbody.useGravity = false;
            _heldRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // Prevents phasing through walls while held

            // Ignore Player collision
            Collider heldCol = grabbable.GetComponent<Collider>();
            if (heldCol != null && _playerColliders != null)
            {
                foreach (Collider pc in _playerColliders)
                {
                    Physics.IgnoreCollision(heldCol, pc, true);
                }
            }

            _justGrabbedThisFrame = true;
        }

        public void Drop()
        {
            if (_currentHeldObject == null || _heldRigidbody == null) return;

            // Stop dead to prevent shooting forward!
            _heldRigidbody.linearVelocity = Vector3.zero;
            _heldRigidbody.angularVelocity = Vector3.zero;

            // Restore physics state
            _heldRigidbody.angularDamping = _originalAngularDamping;
            _heldRigidbody.linearDamping = _originalLinearDamping;
            _heldRigidbody.useGravity = _originalUseGravity;
            _heldRigidbody.collisionDetectionMode = _originalCollisionMode;

            // Restore Player collision
            Collider heldCol = _currentHeldObject.GetComponent<Collider>();
            if (heldCol != null && _playerColliders != null)
            {
                foreach (Collider pc in _playerColliders)
                {
                    Physics.IgnoreCollision(heldCol, pc, false);
                }
            }

            PlayerPlatformRegistry.Current?.SetLookBlocked(false);

            _currentHeldObject.OnDropped();
            
            _currentHeldObject = null;
            _heldRigidbody = null;
        }

        private void Throw()
        {
            if (_currentHeldObject == null || _heldRigidbody == null) return;

            Rigidbody rb = _heldRigidbody;
            PhysicsGrabbable thrown = _currentHeldObject;
            Drop();
            
            rb.AddForce(transform.forward * _throwForce, ForceMode.Impulse);
            thrown?.OnThrown();
        }
    }
}
