using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Inventory
{
    public enum InventoryItemCategory { KeyItem, Tool, Document, Consumable, Equipment, Miscellaneous }
    public enum InventoryPrimaryAction { Automatic, Read, EquipOrHold, Consume, None }

    [System.Serializable]
    public struct ItemCombination
    {
        [Tooltip("The item this should be combined with.")]
        public InventoryItemData CombineWith;
        
        [Tooltip("The resulting item after combination.")]
        public InventoryItemData ResultItem;
        
        [Tooltip("Does this item get destroyed after combination?")]
        public bool DestroyThis;
        
        [Tooltip("Does the other item get destroyed after combination?")]
        public bool DestroyOther;
    }
    /// <summary>
    /// ScriptableObject that defines an inventory item's data.
    /// Create assets via: Right Click > Create > Escape Room Framework > Inventory > Item
    /// </summary>
    [CreateAssetMenu(
        fileName = "New Item",
        menuName = "Escape Room Framework/Inventory/Item",
        order = 0)]
    public class InventoryItemData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _itemId = "";
        [SerializeField] private string _displayName = "New Item";
        [TextArea(2, 4)]
        [SerializeField] private string _description = "";

        [Header("Visuals")]
        [SerializeField] private Sprite _icon;
        [SerializeField] private GameObject _worldPrefab;

        [Header("Behaviour")]
        [SerializeField] private InventoryItemCategory _category = InventoryItemCategory.KeyItem;
        [SerializeField] private InventoryPrimaryAction _primaryAction = InventoryPrimaryAction.Automatic;
        [SerializeField] private bool _isConsumable = true;
        [SerializeField] private bool _isStackable = false;
        [SerializeField] private int _maxStack = 1;
        [SerializeField] private bool _canDrop = true;
        [SerializeField] private bool _canExamine = true;
        
        [Header("Readable Note")]
        [Tooltip("If true, this item can be read from the inventory like a piece of paper.")]
        [SerializeField] private bool _isReadable = false;
        [TextArea(5, 10)]
        [SerializeField] private string _noteContent = "";

        [Header("Combinations")]
        [Tooltip("Recipes for combining this item with other items.")]
        [SerializeField] private List<ItemCombination> _combinations = new List<ItemCombination>();

        // ── Public API ───────────────────────────────────────────────────────
        /// <summary>Unique ID used by all systems to reference this item.</summary>
        public string ItemId => string.IsNullOrEmpty(_itemId) ? name : _itemId;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public GameObject WorldPrefab => _worldPrefab;
        public InventoryItemCategory Category => _category;
        public InventoryPrimaryAction PrimaryAction => _primaryAction;
        public bool IsConsumable => _isConsumable;
        public bool IsStackable => _isStackable;
        public int MaxStack => _maxStack;
        public bool CanDrop => _canDrop && _worldPrefab != null;
        public bool CanExamine => _canExamine && _worldPrefab != null;
        public bool CanCombine => _combinations != null && _combinations.Count > 0;
        public bool IsReadable => _isReadable;
        public string NoteContent => _noteContent;
        public IReadOnlyList<ItemCombination> Combinations => _combinations;

        private void OnValidate()
        {
            // Auto-fill itemId from asset name if empty
            if (string.IsNullOrEmpty(_itemId))
                _itemId = name.ToLower().Replace(" ", "_");
            _maxStack = _isStackable ? Mathf.Max(1, _maxStack) : 1;
        }
    }
}
