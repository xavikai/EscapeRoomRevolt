using UnityEngine;

namespace EscapeRoomRevolt.Systems.Inventory
{
    /// <summary>
    /// ScriptableObject that defines an inventory item's data.
    /// Create assets via: Right Click > Create > EscapeRoom > Inventory Item
    /// </summary>
    [CreateAssetMenu(
        fileName = "New Item",
        menuName = "EscapeRoom/Inventory Item",
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
        [SerializeField] private bool _isConsumable = true;
        [SerializeField] private bool _isStackable = false;
        [SerializeField] private int _maxStack = 1;
        
        [Header("Readable Note")]
        [Tooltip("If true, this item can be read from the inventory like a piece of paper.")]
        [SerializeField] private bool _isReadable = false;
        [TextArea(5, 10)]
        [SerializeField] private string _noteContent = "";

        // ── Public API ───────────────────────────────────────────────────────
        /// <summary>Unique ID used by all systems to reference this item.</summary>
        public string ItemId => string.IsNullOrEmpty(_itemId) ? name : _itemId;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public GameObject WorldPrefab => _worldPrefab;
        public bool IsConsumable => _isConsumable;
        public bool IsStackable => _isStackable;
        public int MaxStack => _maxStack;
        public bool IsReadable => _isReadable;
        public string NoteContent => _noteContent;

        private void OnValidate()
        {
            // Auto-fill itemId from asset name if empty
            if (string.IsNullOrEmpty(_itemId))
                _itemId = name.ToLower().Replace(" ", "_");
        }
    }
}
