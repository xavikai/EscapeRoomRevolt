using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Inventory
{
    [CreateAssetMenu(fileName = "ItemCatalog", menuName = "Escape Room Framework/Inventory/Item Catalog")]
    public sealed class ItemCatalog : ScriptableObject
    {
        [SerializeField] private List<InventoryItemData> _items = new();
        public IEnumerable<InventoryItemData> Items => _items;
    }
}
