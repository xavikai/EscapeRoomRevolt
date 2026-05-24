using System.Collections.Generic;
using UnityEngine;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Systems.Inventory;
using TMPro;

namespace EscapeRoomRevolt.UI.PC
{
    /// <summary>
    /// Displays the player's inventory items.
    /// Updates automatically by listening to EventBus events.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private GameObject _itemSlotPrefab; // A prefab with an Image and TextMeshProUGUI

        private readonly List<GameObject> _spawnedSlots = new List<GameObject>();

        private void OnEnable()
        {
            EventBus.Subscribe<OnItemPickedUp>(HandleItemChanged);
            EventBus.Subscribe<OnItemUsed>(HandleItemChanged);
            RefreshUI();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<OnItemPickedUp>(HandleItemChanged);
            EventBus.Unsubscribe<OnItemUsed>(HandleItemChanged);
        }

        private void HandleItemChanged(OnItemPickedUp data) => RefreshUI();
        private void HandleItemChanged(OnItemUsed data) => RefreshUI();

        private void RefreshUI()
        {
            if (InventoryManager.Instance == null) return;

            // Clear old slots
            foreach (var slot in _spawnedSlots)
            {
                Destroy(slot);
            }
            _spawnedSlots.Clear();

            // Spawn new slots
            foreach (var itemId in InventoryManager.Instance.GetAllItemIds())
            {
                var data = InventoryManager.Instance.GetItemData(itemId);
                int quantity = InventoryManager.Instance.GetQuantity(itemId);

                if (data != null && _itemSlotPrefab != null)
                {
                    var slotGo = Instantiate(_itemSlotPrefab, _itemsContainer);
                    
                    // Assuming the prefab has a TextMeshProUGUI for the name/quantity
                    // In a real scenario, you'd have an InventorySlotUI script on the prefab to set Icon and Text cleanly
                    var textComp = slotGo.GetComponentInChildren<TextMeshProUGUI>();
                    if (textComp != null)
                    {
                        textComp.text = data.IsStackable ? $"{data.DisplayName} (x{quantity})" : data.DisplayName;
                    }

                    _spawnedSlots.Add(slotGo);
                }
            }
        }
    }
}
