using EscapeRoomRevolt.Core.Flow;
using EscapeRoomRevolt.Systems.Inventory;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Interaction
{
    /// <summary>
    /// Sends the player to a spawn point in another scene, optionally locked behind an inventory
    /// item like Door. Unlike Door this has no physical open/close animation — it's a plain trigger
    /// for room-to-room transitions (see GameFlowManager.TransitionToRoom).
    /// </summary>
    public class RoomPortal : InteractableBase, IInventoryItemTarget
    {
        [Header("Destination")]
        [SerializeField] private string _targetScene;
        [Tooltip("Matched against a RoomSpawnPoint's Spawn Id in the target scene. Leave empty to use wherever the target scene's Player prefab is already placed.")]
        [SerializeField] private string _targetSpawnId;
        [SerializeField] private RoomLoadMode _loadMode = RoomLoadMode.Single;

        [Header("Lock (optional)")]
        [SerializeField] private bool _isLocked;
        [SerializeField] private string _requiredItemId = "";
        [SerializeField] private string _lockedPrompt = "Locked";
        [SerializeField] private ItemUsePolicy _itemUsePolicy = ItemUsePolicy.OfferCompatible;
        [SerializeField] private bool _consumeRequiredItem = true;

        public bool IsLocked => _isLocked;

        public override string InteractionPrompt => _isLocked ? _lockedPrompt : base.InteractionPrompt;

        protected override void OnInteract()
        {
            if (!TryResolvePlayerLock()) return;
            GameFlowManager.EnsureInstance().TransitionToRoom(_targetScene, _targetSpawnId, _loadMode);
        }

        public void Unlock() => _isLocked = false;
        public void Lock() => _isLocked = true;

        private bool TryResolvePlayerLock()
        {
            if (!_isLocked) return true;
            InventoryManager inventory = InventoryManager.Instance;
            ItemUseResult result = inventory != null
                ? inventory.RequestUseOnTarget(this)
                : ItemUseResult.NoCompatibleItem;
            if (result == ItemUseResult.Used) return true;
            if (result == ItemUseResult.OfferedSelection) return false;

            Debug.Log($"[RoomPortal] {name} is locked. Required item: {_requiredItemId}");
            return false;
        }

        public ItemUsePolicy UsePolicy => _itemUsePolicy;
        public bool ConsumeItemOnUse => _consumeRequiredItem;
        public bool AcceptsItem(InventoryItemData item) => item != null
            && !string.IsNullOrWhiteSpace(_requiredItemId)
            && item.ItemId == _requiredItemId;

        public bool TryUseItem(InventoryItemData item)
        {
            if (!_isLocked || !AcceptsItem(item)) return false;
            Unlock();
            return true;
        }

        [System.Serializable]
        private sealed class RoomPortalSaveState
        {
            public bool isLocked;
        }

        public override string SaveData() => JsonUtility.ToJson(new RoomPortalSaveState { isLocked = _isLocked });

        public override void LoadData(string json)
        {
            RoomPortalSaveState state = JsonUtility.FromJson<RoomPortalSaveState>(json);
            if (state != null) _isLocked = state.isLocked;
        }
    }
}
