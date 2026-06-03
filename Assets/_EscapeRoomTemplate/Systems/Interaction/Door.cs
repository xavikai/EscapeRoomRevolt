using UnityEngine;
using EscapeRoomRevolt.Core;

namespace EscapeRoomRevolt.Systems.Interaction
{
    public enum DoorMovementType
    {
        Pivot,
        Slide
    }

    /// <summary>
    /// A door or container that can be locked/unlocked.
    /// Supports animation via Animator, or smooth programmatic rotation/sliding.
    ///
    /// Publishes: OnLockStateChanged
    /// </summary>
    public class Door : InteractableBase
    {
        [Header("Door Settings")]
        [SerializeField] private bool _isLocked = false;
        [SerializeField] private string _requiredItemId = "";
        [SerializeField] private string _lockedPrompt = "Locked";
        [SerializeField] private string _openPrompt = "Open";

        [Header("Animation")]
        [SerializeField] private Animator _animator;
        [SerializeField] private string _openTrigger = "Open";
        [SerializeField] private string _closeTrigger = "Close";

        [Header("Movement Settings")]
        [SerializeField] private DoorMovementType _movementType = DoorMovementType.Pivot;
        [Tooltip("Time in seconds to smoothly open the door.")]
        [SerializeField] private float _openDuration = 0.8f;

        [Header("Pivot Settings (Rotation)")]
        [Tooltip("The object whose position defines the hinge. If null, the center of this object is used.")]
        [SerializeField] private Transform _customPivot;
        [SerializeField] private float _openAngle = 90f;
        [Tooltip("If true, the door will swing away from the player based on their position.")]
        [SerializeField] private bool _openAwayFromPlayer = true;

        [Header("Slide Settings (Translation)")]
        [Tooltip("The local offset to move the door when open.")]
        [SerializeField] private Vector3 _slideOffset = new Vector3(1.5f, 0, 0);

        private bool _isOpen = false;
        private Coroutine _movementCoroutine;
        
        private Quaternion _closedRotation;
        private Vector3 _closedPosition;
        private int _openDirection = 1;

        private Vector3 _worldHingePoint;
        private Vector3 _localPivotOffset;

        public override string InteractionPrompt =>
            _isLocked ? _lockedPrompt : (_isOpen ? "Close" : _openPrompt);

        private void Start()
        {
            _closedRotation = transform.rotation;
            _closedPosition = transform.position;

            if (_customPivot != null)
            {
                _worldHingePoint = _customPivot.position;
                _localPivotOffset = transform.InverseTransformPoint(_customPivot.position);
            }
            else
            {
                _worldHingePoint = transform.position;
                _localPivotOffset = Vector3.zero;
            }
        }

        protected override void OnInteract()
        {
            if (_isLocked)
            {
                var inventory = EscapeRoomRevolt.Systems.Inventory.InventoryManager.Instance;
                if (!string.IsNullOrEmpty(_requiredItemId) && inventory != null && inventory.HasItem(_requiredItemId))
                {
                    inventory.UseItem(_requiredItemId);
                    Unlock();
                }
                else
                {
                    Debug.Log($"[Door] {name} is locked. Required item: {_requiredItemId}");
                    return;
                }
            }

            _isOpen = !_isOpen;

            if (_animator != null)
            {
                _animator.SetTrigger(_isOpen ? _openTrigger : _closeTrigger);
            }
            else
            {
                if (_movementCoroutine != null) StopCoroutine(_movementCoroutine);

                if (_movementType == DoorMovementType.Pivot)
                {
                    if (_isOpen && _openAwayFromPlayer && Camera.main != null)
                    {
                        Vector3 dirToPlayer = (Camera.main.transform.position - transform.position).normalized;
                        float dot = Vector3.Dot(transform.forward, dirToPlayer);
                        _openDirection = dot > 0 ? -1 : 1;
                    }

                    Quaternion targetRotation = _closedRotation;

                    if (_isOpen)
                    {
                        Quaternion rotationDelta = Quaternion.Euler(0, _openAngle * _openDirection, 0);
                        targetRotation = _closedRotation * rotationDelta;
                    }

                    _movementCoroutine = StartCoroutine(SmoothPivot(transform.rotation, targetRotation, _openDuration));
                }
                else if (_movementType == DoorMovementType.Slide)
                {
                    // For Slide, _closedPosition is treated as localPosition to respect parent transforms
                    Vector3 localClosed = transform.parent != null ? transform.parent.InverseTransformPoint(_closedPosition) : _closedPosition;
                    Vector3 targetLocalPosition = _isOpen ? localClosed + _slideOffset : localClosed;
                    
                    _movementCoroutine = StartCoroutine(SmoothSlide(transform, transform.localPosition, targetLocalPosition, _openDuration));
                }
            }

            Debug.Log($"[Door] {name} is now {(_isOpen ? "open" : "closed")}.");
        }

        private System.Collections.IEnumerator SmoothPivot(Quaternion startRot, Quaternion endRot, float duration)
        {
            float time = 0;
            while (time < duration)
            {
                float t = time / duration;
                // Slerp garanteix una rotació suau circular
                Quaternion currentRot = Quaternion.Slerp(startRot, endRot, t);
                transform.rotation = currentRot;
                // La posició sempre ha de mantenir el pivot al mateix lloc del món
                transform.position = _worldHingePoint - (currentRot * _localPivotOffset);
                
                time += Time.deltaTime;
                yield return null;
            }
            transform.rotation = endRot;
            transform.position = _worldHingePoint - (endRot * _localPivotOffset);
        }
        
        private System.Collections.IEnumerator SmoothSlide(Transform target, Vector3 startPos, Vector3 endPos, float duration)
        {
            float time = 0;
            while (time < duration)
            {
                target.localPosition = Vector3.Lerp(startPos, endPos, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            target.localPosition = endPos;
        }

        public void Unlock()
        {
            if (!_isLocked) return;
            _isLocked = false;
            EventBus.Publish(new OnLockStateChanged { lockableId = SaveId, isLocked = false });
        }

        public void Lock()
        {
            _isLocked = true;
            EventBus.Publish(new OnLockStateChanged { lockableId = SaveId, isLocked = true });
        }

        public bool IsLocked => _isLocked;
        public bool IsOpen => _isOpen;
        public string RequiredItemId => _requiredItemId;
    }
}
