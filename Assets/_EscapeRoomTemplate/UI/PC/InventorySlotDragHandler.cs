using UnityEngine;
using UnityEngine.EventSystems;
using EscapeRoomRevolt.Systems.Inventory;
using UnityEngine.UI;

namespace EscapeRoomRevolt.UI.PC
{
    /// <summary>
    /// Attach to the Inventory Slot prefab (or added dynamically) to enable Drag and Drop combinations.
    /// </summary>
    public class InventorySlotDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        private InventoryItemData _itemData;
        private Image _slotIcon;

        // Static tracking of what's being dragged
        public static GameObject DraggingIcon;
        public static InventoryItemData DraggedItemData;
        private Canvas _rootCanvas;

        public void Setup(InventoryItemData data, Image slotIcon)
        {
            _itemData = data;
            _slotIcon = slotIcon;
            _rootCanvas = GetComponentInParent<Canvas>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_itemData == null || _slotIcon == null) 
            {
                Debug.LogWarning("[DragHandler] Cannot start drag: missing data or icon reference.");
                return;
            }

            DraggedItemData = _itemData;
            Debug.Log($"[DragHandler] Started dragging {_itemData.DisplayName}");

            // Create a temporary floating icon
            DraggingIcon = new GameObject("DraggingIcon");
            DraggingIcon.transform.SetParent(_rootCanvas.transform, false);
            DraggingIcon.transform.SetAsLastSibling();

            var img = DraggingIcon.AddComponent<Image>();
            if (_slotIcon.sprite != null)
            {
                img.sprite = _slotIcon.sprite;
            }
            img.raycastTarget = false; // So we can drop through it

            var rt = DraggingIcon.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(64, 64); // Fixed size or read from original

            // Semi-transparent original slot to show it's moving
            _slotIcon.color = new Color(1, 1, 1, 0.5f);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (DraggingIcon != null)
            {
                // Follow mouse
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rootCanvas.transform as RectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint);
                
                DraggingIcon.transform.localPosition = localPoint;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (DraggingIcon != null)
            {
                Destroy(DraggingIcon);
            }

            if (_slotIcon != null)
            {
                _slotIcon.color = Color.white;
            }

            DraggedItemData = null;
        }

        public void OnDrop(PointerEventData eventData)
        {
            // If something was dropped ON THIS slot
            if (DraggedItemData != null && DraggedItemData != _itemData)
            {
                // Attempt to combine
                bool success = InventoryManager.Instance.TryCombine(DraggedItemData.ItemId, _itemData.ItemId);
                
                if (success)
                {
                    // Clean up the dragging icon immediately because this slot might be destroyed by UI refresh
                    if (DraggingIcon != null)
                    {
                        Destroy(DraggingIcon);
                        DraggingIcon = null;
                        DraggedItemData = null;
                    }
                }
            }
        }

        private void OnDisable()
        {
            // Fallback: If this slot is destroyed or disabled while dragging, clean up the floating icon
            if (DraggedItemData != null && DraggedItemData == _itemData)
            {
                if (DraggingIcon != null)
                {
                    Destroy(DraggingIcon);
                    DraggingIcon = null;
                }
                DraggedItemData = null;
            }
        }
    }
}
