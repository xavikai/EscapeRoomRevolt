using UnityEngine;
using EscapeRoomRevolt.Core;

namespace EscapeRoomRevolt.Systems.Interaction
{
    /// <summary>
    /// A door or container that can be locked/unlocked.
    /// Supports animation via Animator or simple rotation.
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

        private bool _isOpen = false;

        public override string InteractionPrompt =>
            _isLocked ? _lockedPrompt : (_isOpen ? "Close" : _openPrompt);

        protected override void OnInteract()
        {
            if (_isLocked)
            {
                // Check if we have the required key in the inventory
                var inventory = EscapeRoomRevolt.Systems.Inventory.InventoryManager.Instance;
                if (!string.IsNullOrEmpty(_requiredItemId) && inventory != null && inventory.HasItem(_requiredItemId))
                {
                    inventory.UseItem(_requiredItemId);
                    Unlock();
                    // Proceed to open the door below
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
                // Fallback visual for primitive objects without animations
                transform.localRotation = _isOpen ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
            }

            Debug.Log($"[Door] {name} is now {(_isOpen ? "open" : "closed")}.");
        }

        /// <summary>Unlocks the door (called by the Inventory/Puzzle system).</summary>
        public void Unlock()
        {
            if (!_isLocked) return;
            _isLocked = false;

            EventBus.Publish(new OnLockStateChanged
            {
                lockableId = SaveId,
                isLocked = false
            });

            Debug.Log($"[Door] {name} unlocked!");
        }

        /// <summary>Locks the door.</summary>
        public void Lock()
        {
            _isLocked = true;

            EventBus.Publish(new OnLockStateChanged
            {
                lockableId = SaveId,
                isLocked = true
            });
        }

        public bool IsLocked => _isLocked;
        public bool IsOpen => _isOpen;
        public string RequiredItemId => _requiredItemId;
    }
}
