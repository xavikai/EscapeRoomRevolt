using UnityEngine;
using EscapeRoomRevolt.Systems.Interaction;

namespace EscapeRoomRevolt.Systems.Inventory
{
    /// <summary>
    /// Place this on any 3D object in the world to make it pickable.
    /// When the player interacts with it, the item is added to the inventory
    /// and the GameObject is deactivated (or destroyed).
    ///
    /// Requires: InventoryManager in the scene, Collider on this GameObject.
    /// </summary>
    public class PickableItem : InteractableBase
    {
        [Header("Item")]
        [SerializeField] private InventoryItemData _itemData;
        [SerializeField] private int _quantity = 1;

        [Header("On Pick Up")]
        [SerializeField] private bool _destroyOnPickup = false;
        [SerializeField] private AudioClip _pickupSound;

        public override string InteractionPrompt =>
            _itemData != null ? $"Pick up {_itemData.DisplayName}" : "Pick up";

        protected override void OnInteract()
        {
            if (_itemData == null)
            {
                Debug.LogWarning($"[PickableItem] {name} has no InventoryItemData assigned!");
                return;
            }

            var inventory = InventoryManager.Instance;
            if (inventory == null)
            {
                Debug.LogError("[PickableItem] No InventoryManager found in scene!");
                return;
            }

            bool added = inventory.AddItem(_itemData, _quantity);

            if (!added)
            {
                Debug.Log($"[PickableItem] Could not add {_itemData.DisplayName} — inventory full or not stackable.");
                return;
            }

            // Play pickup sound
            if (_pickupSound != null)
                AudioSource.PlayClipAtPoint(_pickupSound, transform.position);

            // Tell SaveManager to never spawn this again
            EscapeRoomRevolt.Core.Save.SaveManager.Instance?.MarkAsDestroyed(SaveId);

            // Remove from world
            if (_destroyOnPickup)
                Destroy(gameObject);
            else
                gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_itemData != null && string.IsNullOrEmpty(name))
                name = _itemData.DisplayName;
        }
#endif
    }
}
