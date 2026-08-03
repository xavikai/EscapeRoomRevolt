using UnityEngine;
using UnityEngine.Events;
using EscapeRoomRevolt.Systems.Inventory;
using EscapeRoomRevolt.Player.VR;
using System.Collections;

namespace EscapeRoomRevolt.Systems.Interaction
{
    /// <summary>
    /// A trigger area that detects if the correct physical object is dropped inside it.
    /// It then smoothly snaps the object into place and fires an event.
    /// Also supports object combinations by spawning a _resultPrefab.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PhysicsSocket : MonoBehaviour
    {
        [Header("Requirement")]
        [Tooltip("The ItemId required to snap into this socket.")]
        [SerializeField] private string _requiredItemId;
        
        [Header("Snapping")]
        [Tooltip("Where the object should snap. If null, uses this transform.")]
        [SerializeField] private Transform _snapTransform;
        [SerializeField] private float _snapSpeed = 10f;
        
        [Header("Combination Result (Optional)")]
        [Tooltip("If set, the socket will consume the item and spawn this new prefab. Useful for combining two objects.")]
        [SerializeField] private GameObject _resultPrefab;
        [Tooltip("If true, destroys the entire root GameObject this socket is attached to when combined. Useful to replace the whole object.")]
        [SerializeField] private bool _replaceRootObject = false;

        [Header("Events")]
        public UnityEvent OnItemSnapped;

        private Collider _triggerCollider;
        private bool _isSolved = false;

        private void Awake()
        {
            _triggerCollider = GetComponent<Collider>();
            _triggerCollider.isTrigger = true;
            if (_snapTransform == null) _snapTransform = transform;
        }

        private void OnTriggerStay(Collider other)
        {
            if (_isSolved) return;

            var grabbable = other.GetComponentInParent<PhysicsGrabbable>();
            if (grabbable == null) return;

            // If the player is still holding this specific object — on PC via PhysicsGrabber, or in
            // VR via an XRGrabInteractable — wait. We only snap once it's actually let go, on either
            // platform. Checking the specific object (not just "holding something") also means a
            // different item held in-hand no longer blocks a nearby dropped match from snapping.
            if (IsCurrentlyHeld(grabbable)) return;

            var pickable = grabbable.GetComponent<PickableItem>();
            if (pickable == null || pickable.Data == null) return;

            if (pickable.Data.ItemId == _requiredItemId)
            {
                // IT'S A MATCH! AND IT'S DROPPED! Snap it!
                StartCoroutine(SnapRoutine(grabbable));
            }
        }

        private static bool IsCurrentlyHeld(PhysicsGrabbable grabbable)
        {
            if (PhysicsGrabber.Instance != null && PhysicsGrabber.Instance.CurrentHeldObject == grabbable) return true;
            VRInteractionBridge bridge = grabbable.GetComponent<VRInteractionBridge>();
            return bridge != null && bridge.IsSelected;
        }

        private IEnumerator SnapRoutine(PhysicsGrabbable grabbable)
        {
            _isSolved = true;

            // Disable physics so it doesn't fall or get knocked away
            var rb = grabbable.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Disable its grabbable script so player can't steal it back while snapping
            grabbable.enabled = false;

            Transform t = grabbable.transform;
            
            // Lerp smoothly to the snap point
            float distance = Vector3.Distance(t.position, _snapTransform.position);
            while (distance > 0.01f)
            {
                t.position = Vector3.Lerp(t.position, _snapTransform.position, Time.deltaTime * _snapSpeed);
                t.rotation = Quaternion.Lerp(t.rotation, _snapTransform.rotation, Time.deltaTime * _snapSpeed);
                distance = Vector3.Distance(t.position, _snapTransform.position);
                yield return null;
            }

            t.position = _snapTransform.position;
            t.rotation = _snapTransform.rotation;

            // Trigger events
            OnItemSnapped?.Invoke();

            // Handle Combination Logic
            if (_resultPrefab != null)
            {
                Instantiate(_resultPrefab, _snapTransform.position, _snapTransform.rotation);
                
                // Destroy the inserted item
                Destroy(grabbable.gameObject);

                if (_replaceRootObject)
                {
                    // Destroy the parent object this socket is attached to
                    Destroy(transform.root.gameObject);
                }
            }
            
            enabled = false;
        }
    }
}
