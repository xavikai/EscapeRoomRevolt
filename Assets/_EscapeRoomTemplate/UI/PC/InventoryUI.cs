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
                    var textComp = slotGo.GetComponentInChildren<TextMeshProUGUI>();
                    if (textComp != null)
                    {
                        textComp.text = data.IsStackable ? $"{data.DisplayName} (x{quantity})" : data.DisplayName;
                    }

                    // Setup Icon if available
                    var images = slotGo.GetComponentsInChildren<UnityEngine.UI.Image>();
                    foreach (var imgComp in images)
                    {
                        if (imgComp.gameObject.name == "Icon")
                        {
                            if (data.Icon != null)
                            {
                                imgComp.sprite = data.Icon;
                                imgComp.color = Color.white;
                            }
                            else
                            {
                                imgComp.color = Color.clear; // Hide if no icon
                            }
                        }
                    }

                    // Make it interactive
                    var btn = slotGo.GetComponent<UnityEngine.UI.Button>();
                    if (btn == null) btn = slotGo.AddComponent<UnityEngine.UI.Button>();
                    
                    // Add some quick visual feedback for the button (Unity default)
                    var img = slotGo.GetComponent<UnityEngine.UI.Image>();
                    if (img != null)
                    {
                        btn.targetGraphic = img;
                        var colors = btn.colors;
                        colors.highlightedColor = new Color(0.8f, 0.8f, 0.8f);
                        colors.pressedColor = new Color(0.6f, 0.6f, 0.6f);
                        btn.colors = colors;
                    }

                    btn.onClick.AddListener(() => OnSlotClicked(data));

                    _spawnedSlots.Add(slotGo);
                }
            }
        }

        private void OnSlotClicked(InventoryItemData data)
        {
            if (data == null) return;

            // In the future, this could open a context menu (Read, Drop, Use).
            // For now, if it's a note, directly read it!
            if (data.IsReadable)
            {
                UIManager.Instance.ShowNoteReader(data.NoteContent);
            }
        }
    }
}
