using UnityEngine;
using UnityEngine.Events;
using EscapeRoomRevolt.Systems.Inventory;

namespace EscapeRoomRevolt.Systems.Interaction
{
    /// <summary>
    /// An interactable that requires a specific item from the inventory.
    /// E.g. A keyhole requiring a key, a pedestal requiring an idol.
    /// </summary>
    public class ItemReceiver : InteractableBase
    {
        [Header("Requirement")]
        [Tooltip("The item required to interact successfully.")]
        [SerializeField] private InventoryItemData _requiredItem;
        
        [Tooltip("If true, the item is removed from the inventory when used.")]
        [SerializeField] private bool _consumeItem = true;

        [Header("Feedback")]
        [Tooltip("Optional: Where should the item's 3D model appear? If empty, it appears exactly on this object.")]
        [SerializeField] private Transform _spawnLocation;

        [Tooltip("Message shown if the player doesn't have the item.")]
        [SerializeField] private string _missingItemMessage = "Necessito un objecte per posar aquí.";
        
        [Tooltip("Message shown when the item is successfully used.")]
        [SerializeField] private string _successMessage = "Fet!";

        [Header("Events")]
        [Tooltip("How long to wait after placing the item before firing OnItemAccepted? (Useful to wait for a key turning animation to finish)")]
        [SerializeField] private float _delayBeforeAcceptEvent = 0f;

        [Tooltip("Fired when the correct item is used.")]
        public UnityEvent OnItemAccepted;

        [Tooltip("Fired when the player interacts but doesn't have the item.")]
        public UnityEvent OnItemRejected;

        private bool _alreadySolved = false;
        private string _currentPromptOverride = null;
        private float _promptOverrideTimer = 0f;

        public override string InteractionPrompt 
        {
            get 
            {
                if (_currentPromptOverride != null) return _currentPromptOverride;
                return _alreadySolved ? "" : base.InteractionPrompt;
            }
        }

        private void Update()
        {
            if (_currentPromptOverride != null)
            {
                _promptOverrideTimer -= Time.deltaTime;
                if (_promptOverrideTimer <= 0f)
                {
                    _currentPromptOverride = null;
                }
            }
        }

        protected override void OnInteract()
        {
            if (_alreadySolved)
            {
                // Can't interact again if solved (or you could allow it, depending on design)
                return;
            }

            if (_requiredItem == null)
            {
                Debug.LogWarning($"[ItemReceiver] {InteractionPrompt} has no Required Item assigned!");
                return;
            }

            if (InventoryManager.Instance.HasItem(_requiredItem.ItemId))
            {
                AcceptItem();
            }
            else
            {
                RejectItem();
            }
        }

        private void AcceptItem()
        {
            if (_consumeItem)
            {
                InventoryManager.Instance.UseItem(_requiredItem.ItemId);
            }

            _alreadySolved = true;
            
            // Auto-spawn the 3D model if it has one
            if (_requiredItem.WorldPrefab != null)
            {
                Transform spawnPoint = _spawnLocation != null ? _spawnLocation : transform;
                var spawned = Instantiate(_requiredItem.WorldPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);

                var pickable = spawned.GetComponentInChildren<PickableItem>();
                if (pickable != null) Destroy(pickable);
            }

            // Show success message temporarily
            _currentPromptOverride = _successMessage;
            _promptOverrideTimer = 2.5f;

            // Log for debugging
            Debug.Log($"[ItemReceiver] {_requiredItem.DisplayName} accepted! {_successMessage}");
            
            // Fire the event (with delay if configured)
            if (_delayBeforeAcceptEvent > 0f)
            {
                StartCoroutine(AcceptEventRoutine());
            }
            else
            {
                OnItemAccepted?.Invoke();
            }

            // Disable interaction after solving so it doesn't show the prompt anymore
            enabled = false; 
        }

        private System.Collections.IEnumerator AcceptEventRoutine()
        {
            yield return new WaitForSeconds(_delayBeforeAcceptEvent);
            OnItemAccepted?.Invoke();
        }

        private void RejectItem()
        {
            // Show missing message temporarily
            _currentPromptOverride = _missingItemMessage;
            _promptOverrideTimer = 2.5f;

            Debug.Log($"[ItemReceiver] Missing {_requiredItem.DisplayName}. {_missingItemMessage}");
            OnItemRejected?.Invoke();
        }

        // ── Save/Load ────────────────────────────────────────────────────────

        [System.Serializable]
        private class ReceiverSaveState
        {
            public bool alreadySolved;
        }

        public override string SaveData()
        {
            var state = new ReceiverSaveState
            {
                alreadySolved = _alreadySolved
            };
            return JsonUtility.ToJson(state);
        }

        public override void LoadData(string json)
        {
            var state = JsonUtility.FromJson<ReceiverSaveState>(json);
            if (state == null) return;

            if (state.alreadySolved)
            {
                _alreadySolved = true;
                enabled = false;

                // Spawn the prefab instantly without firing the OnItemAccepted event 
                // because the event targets (doors) will load their own state!
                if (_requiredItem != null && _requiredItem.WorldPrefab != null)
                {
                    Transform spawnPoint = _spawnLocation != null ? _spawnLocation : transform;
                    var spawned = Instantiate(_requiredItem.WorldPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
                    
                    var pickable = spawned.GetComponentInChildren<PickableItem>();
                    if (pickable != null) Destroy(pickable);
                }
            }
        }
    }
}
