using System.Collections.Generic;
using UnityEngine;
using EscapeRoomRevolt.Core;

namespace EscapeRoomRevolt.Systems.Inventory
{
    /// <summary>
    /// Manages the player's inventory at runtime.
    /// Place one instance in the scene (on the Player or a dedicated manager GameObject).
    ///
    /// Publishes: OnItemPickedUp, OnItemUsed
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool _logActions = true;

        // itemId → quantity
        private readonly Dictionary<string, int> _items = new Dictionary<string, int>();
        // itemId → data reference
        private readonly Dictionary<string, InventoryItemData> _itemData =
            new Dictionary<string, InventoryItemData>();

        // ── Singleton ────────────────────────────────────────────────────────
        public static InventoryManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ── Public API ───────────────────────────────────────────────────────

        /// <summary>Adds an item to the inventory.</summary>
        public bool AddItem(InventoryItemData data, int quantity = 1)
        {
            if (data == null) return false;

            string id = data.ItemId;

            if (_items.ContainsKey(id))
            {
                if (!data.IsStackable) return false;
                _items[id] = Mathf.Min(_items[id] + quantity, data.MaxStack);
            }
            else
            {
                _items[id] = quantity;
                _itemData[id] = data;
            }

            Log($"Added: {data.DisplayName} (x{quantity}) — Total: {_items[id]}");

            EventBus.Publish(new OnItemPickedUp
            {
                itemId = id,
                itemName = data.DisplayName
            });

            return true;
        }

        /// <summary>Removes one unit of an item. Returns true if successful.</summary>
        public bool UseItem(string itemId)
        {
            if (!HasItem(itemId)) return false;

            _items[itemId]--;

            if (_items[itemId] <= 0)
            {
                _items.Remove(itemId);
                _itemData.Remove(itemId);
            }

            Log($"Used item: {itemId}");

            EventBus.Publish(new OnItemUsed { itemId = itemId });

            return true;
        }

        /// <summary>Returns true if the player has at least one of this item.</summary>
        public bool HasItem(string itemId) =>
            _items.ContainsKey(itemId) && _items[itemId] > 0;

        /// <summary>Returns the quantity of a given item (0 if not in inventory).</summary>
        public int GetQuantity(string itemId) =>
            _items.TryGetValue(itemId, out int qty) ? qty : 0;

        /// <summary>Returns the data for a given item (null if not in inventory).</summary>
        public InventoryItemData GetItemData(string itemId) =>
            _itemData.TryGetValue(itemId, out var data) ? data : null;

        /// <summary>Returns all item IDs currently in the inventory.</summary>
        public IEnumerable<string> GetAllItemIds() => _items.Keys;

        /// <summary>Clears the entire inventory (use on game reset).</summary>
        public void Clear()
        {
            _items.Clear();
            _itemData.Clear();
            Log("Inventory cleared.");
        }

        private void Log(string message)
        {
            if (_logActions) Debug.Log($"[Inventory] {message}");
        }
    }
}
