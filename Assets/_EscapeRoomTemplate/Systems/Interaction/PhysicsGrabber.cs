using UnityEngine;

namespace EscapeRoomRevolt.Systems.Interaction
{
    public class PhysicsGrabber : MonoBehaviour
    {
        public static PhysicsGrabber Instance { get; private set; }

        [Header("Grab Settings")]
        [SerializeField] private float _holdDistance = 1.25f;
        [SerializeField] private float _pullForce = 350f;
        [SerializeField] private float _throwForce = 15f;
        [SerializeField] private float _autoDropDistance = 3.0f; 

        private PhysicsGrabbable _currentHeldObject;
        private Rigidbody _heldRigidbody;
        private Transform _holdPoint;

        // Backup properties
        private float _originalDrag;
        private float _originalAngularDrag;
        private bool _originalUseGravity;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // Create a virtual hold point in front of the camera (assuming this script is on the camera or player)
            _holdPoint = new GameObject("PhysicsHoldPoint").transform;
            _holdPoint.parent = transform;
            _holdPoint.localPosition = new Vector3(0, 0, _holdDistance);
        }

        private void Update()
        {
            if (_currentHeldObject == null) return;

            // When UI is blocking, drop the object automatically
            if (EscapeRoomRevolt.UI.PC.UIManager.Instance != null && EscapeRoomRevolt.UI.PC.UIManager.Instance.IsUIBlockingGameplay)
            {
                Drop();
                return;
            }

            // Right Click to Throw
            if (Input.GetMouseButtonDown(1))
            {
                Throw();
                return;
            }

            // E or Left Click to Drop
            // We use GetKeyDown to avoid immediately dropping if the user just clicked to pick it up (though Interact handles that).
            // Actually, Interact() is called on GetKeyDown. If we check GetKeyDown here in the same frame, it might drop it immediately.
            // Better to check GetKeyDown, but if it was just grabbed this frame, ignore.
            if (_justGrabbedThisFrame)
            {
                _justGrabbedThisFrame = false;
                return;
            }

            if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
            {
                Drop();
            }
        }

        private void FixedUpdate()
        {
            if (_heldRigidbody == null) return;

            Vector3 targetPos = _holdPoint.position;
            Vector3 directionToPoint = targetPos - _heldRigidbody.position;
            float distanceToPoint = directionToPoint.magnitude;

            // If it gets stuck behind a wall and player walks away, drop it
            if (distanceToPoint > _autoDropDistance)
            {
                Drop();
                return;
            }

            // Apply velocity to pull it towards the hold point smoothly
            _heldRigidbody.linearVelocity = directionToPoint * _pullForce * Time.fixedDeltaTime;
        }

        private bool _justGrabbedThisFrame = false;

        public void Grab(PhysicsGrabbable grabbable)
        {
            if (_currentHeldObject != null) Drop();

            _currentHeldObject = grabbable;
            _heldRigidbody = grabbable.GetComponent<Rigidbody>();
            
            if (_heldRigidbody == null) return;

            // Save original state
            _originalUseGravity = _heldRigidbody.useGravity;
            _originalDrag = _heldRigidbody.linearDamping;
            _originalAngularDrag = _heldRigidbody.angularDamping;

            // Apply holding physics
            _heldRigidbody.useGravity = false;
            _heldRigidbody.linearDamping = 10f; // High drag prevents overshooting and makes it snappy
            _heldRigidbody.angularDamping = 10f; // Prevent crazy spinning
            
            _justGrabbedThisFrame = true;
            
            // Force disable InteractionManager from trying to interact with it again
            // while we hold it (it blocks raycasts). 
            // The fact that we check _justGrabbedThisFrame prevents immediate drop.
        }

        public void Drop()
        {
            if (_currentHeldObject == null || _heldRigidbody == null) return;

            // Reset velocity so it drops softly instead of retaining the pull velocity
            _heldRigidbody.linearVelocity = Vector3.zero;
            _heldRigidbody.angularVelocity = Vector3.zero;

            // Restore original state
            _heldRigidbody.useGravity = _originalUseGravity;
            _heldRigidbody.linearDamping = _originalDrag;
            _heldRigidbody.angularDamping = _originalAngularDrag;

            _currentHeldObject.OnDropped();
            
            _currentHeldObject = null;
            _heldRigidbody = null;
        }

        private void Throw()
        {
            if (_currentHeldObject == null || _heldRigidbody == null) return;

            Rigidbody rb = _heldRigidbody;
            Drop();
            
            // Add throw impulse
            // Assume the forward direction of this component (Camera) is where we are looking
            rb.AddForce(transform.forward * _throwForce, ForceMode.Impulse);
        }

        public bool IsHoldingObject => _currentHeldObject != null;
    }
}
