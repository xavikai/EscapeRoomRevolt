using UnityEngine;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Systems.Inventory;

namespace EscapeRoomRevolt.Systems.Interaction
{
    /// <summary>
    /// A unified note component.
    /// Can be a fixed note read on the wall, or a pickable note that goes to the inventory.
    /// </summary>
    public class InteractableNote : InteractableBase
    {
        [Header("Behaviour")]
        public bool IsPickable = false;

        [Header("Read In-Place Settings")]
        [TextArea(4, 12)]
        public string NoteContent = "Write your note content here...";
        public string ReadPrompt = "Read Note";
        public bool DisappearAfterRead = false;

        [Header("Pickable Settings")]
        public InventoryItemData ItemData;
        public int Quantity = 1;
        public AudioClip PickupSound;

        private bool _hasBeenRead = false;

        public override string InteractionPrompt 
        {
            get
            {
                if (IsPickable)
                {
                    return ItemData != null ? $"Pick up {ItemData.DisplayName}" : "Pick up Note";
                }
                return ReadPrompt;
            }
        }

        protected override void OnInteract()
        {
            if (IsPickable)
            {
                // Handle Pick Up
                if (ItemData == null)
                {
                    Debug.LogWarning($"[InteractableNote] {name} is pickable but has no InventoryItemData assigned!");
                    return;
                }

                if (InventoryManager.Instance != null)
                {
                    bool added = InventoryManager.Instance.AddItem(ItemData, Quantity);
                    if (added)
                    {
                        if (PickupSound != null)
                            AudioSource.PlayClipAtPoint(PickupSound, transform.position);

                        // Tell SaveManager to never spawn this again
                        EscapeRoomRevolt.Core.Save.SaveManager.Instance?.MarkAsDestroyed(SaveId);
                        gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                // Handle Read In-Place
                _hasBeenRead = true;
                EventBus.Publish(new RequestShowNoteReader { content = NoteContent });

                if (DisappearAfterRead)
                    gameObject.SetActive(false);
            }
        }

        public bool HasBeenRead => _hasBeenRead;
    }
}
