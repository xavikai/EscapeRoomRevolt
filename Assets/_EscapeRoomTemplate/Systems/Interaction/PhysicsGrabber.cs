using UnityEngine;

namespace EscapeRoomRevolt.Systems.Interaction
{
    public class PhysicsGrabber : MonoBehaviour
    {
        public static PhysicsGrabber Instance { get; private set; }

        [Header("Grab Settings")]
        [SerializeField] private float _holdDistance = 1.25f;
        [SerializeField] private float _springForce = 500f;
        [SerializeField] private float _damper = 50f;
        [SerializeField] private float _throwForce = 15f;
        [SerializeField] private float _autoDropDistance = 3.0f; 

        private PhysicsGrabbable _currentHeldObject;
        private Rigidbody _heldRigidbody;
        private Transform _holdPoint;
        private Rigidbody _holdPointRb;
        private SpringJoint _currentJoint;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            _holdPoint = new GameObject("PhysicsHoldPoint").transform;
            _holdPoint.parent = transform;
            _holdPoint.localPosition = new Vector3(0, 0, _holdDistance);
            
            _holdPointRb = _holdPoint.gameObject.AddComponent<Rigidbody>();
            _holdPointRb.isKinematic = true;
        }

        private void Update()
        {
            if (_currentHeldObject == null) return;

            if (EscapeRoomRevolt.UI.PC.UIManager.Instance != null && EscapeRoomRevolt.UI.PC.UIManager.Instance.IsUIBlockingGameplay)
            {
                Drop();
                return;
            }

            if (Input.GetMouseButtonDown(1))
            {
                Throw();
                return;
            }

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

            float distanceToPoint = Vector3.Distance(_holdPoint.position, _heldRigidbody.position);
            
            if (distanceToPoint > _autoDropDistance)
            {
                Drop();
            }
        }

        // State backups
        private float _originalAngularDamping;
        private bool _originalUseGravity;
        private bool _justGrabbedThisFrame = false;

        public void Grab(PhysicsGrabbable grabbable)
        {
            if (_currentHeldObject != null) Drop();

            _currentHeldObject = grabbable;
            _heldRigidbody = grabbable.GetComponent<Rigidbody>();
            
            if (_heldRigidbody == null) return;

            // Backup physics state to prevent spinning and sagging
            _originalAngularDamping = _heldRigidbody.angularDamping;
            _originalUseGravity = _heldRigidbody.useGravity;

            _heldRigidbody.angularDamping = 10f; // High angular damping stops crazy spinning
            _heldRigidbody.useGravity = false; // Disable gravity so it doesn't sag down

            _currentJoint = grabbable.gameObject.AddComponent<SpringJoint>();
            _currentJoint.connectedBody = _holdPointRb;
            _currentJoint.spring = _springForce;
            _currentJoint.damper = _damper;
            _currentJoint.autoConfigureConnectedAnchor = false;
            _currentJoint.connectedAnchor = Vector3.zero;
            _currentJoint.anchor = Vector3.zero;
            _currentJoint.minDistance = 0f;
            _currentJoint.maxDistance = 0f;

            _justGrabbedThisFrame = true;
        }

        public void Drop()
        {
            if (_currentHeldObject == null || _heldRigidbody == null) return;

            if (_currentJoint != null)
            {
                Destroy(_currentJoint);
            }

            // Restore physics state
            _heldRigidbody.angularDamping = _originalAngularDamping;
            _heldRigidbody.useGravity = _originalUseGravity;

            _currentHeldObject.OnDropped();
            
            _currentHeldObject = null;
            _heldRigidbody = null;
        }

        private void Throw()
        {
            if (_currentHeldObject == null || _heldRigidbody == null) return;

            Rigidbody rb = _heldRigidbody;
            Drop();
            
            rb.AddForce(transform.forward * _throwForce, ForceMode.Impulse);
        }

        public bool IsHoldingObject => _currentHeldObject != null;
    }
}
