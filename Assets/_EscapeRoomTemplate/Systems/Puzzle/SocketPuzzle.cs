using UnityEngine;
using EscapeRoomRevolt.Systems.Inventory;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>
    /// A puzzle that requires the player to place a specific item into a receptacle.
    /// E.g. placing a key in a lock, a fuse in a fusebox, or a gem in a statue.
    /// </summary>
    public class SocketPuzzle : PuzzleController
    {
        [Header("Socket Settings")]
        [Tooltip("The ID of the inventory item required to solve this puzzle")]
        [SerializeField] private string _requiredItemId;
        [Tooltip("Whether the item should be removed from the player's inventory upon success")]
        [SerializeField] private bool _consumeItem = true;

        [Header("Visuals (Optional)")]
        [Tooltip("The point where the item's visual representation should be placed")]
        [SerializeField] private Transform _itemPlacementPoint;
        [Tooltip("The visual prefab to spawn when solved (if any)")]
        [SerializeField] private GameObject _placedItemPrefab;

        /// <summary>
        /// Attempts to insert an item into the socket.
        /// Returns true if the correct item was inserted and the puzzle is solved.
        /// </summary>
        public bool TryInsertItem(string itemId)
        {
            if (IsSolved) return false;

            SetInProgress();

            if (itemId == _requiredItemId)
            {
                if (_consumeItem && InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.UseItem(itemId);
                }

                if (_placedItemPrefab != null && _itemPlacementPoint != null)
                {
                    Instantiate(_placedItemPrefab, _itemPlacementPoint.position, _itemPlacementPoint.rotation, _itemPlacementPoint);
                }
                
                Solve();
                return true;
            }
            else
            {
                Fail("Wrong item inserted");
                return false;
            }
        }

        // Note: You can expose a method here to be called by an IInteractable
        // when the player clicks on the socket, which would check InventoryManager.Instance.HasItem(_requiredItemId)
    }
}
