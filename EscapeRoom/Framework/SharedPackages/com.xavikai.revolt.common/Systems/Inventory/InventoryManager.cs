using System;
using System.Collections.Generic;
using System.Linq;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Core.Save;
using EscapeRoomRevolt.Systems.Equipment;
using EscapeRoomRevolt.Systems.Interaction;
using EscapeRoomRevolt.Player;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Inventory
{
    [Serializable]
    public sealed class InventorySaveState
    {
        public int version = 2;
        public List<string> slotItemIds = new List<string>();
        public List<int> slotQuantities = new List<int>();
        public List<string> quickItemIds = new List<string>();
        public int activeQuickIndex;

        // Version 1 compatibility: this used to point to a storage/hotbar slot.
        public int activeSlotIndex;
    }

    [Serializable]
    public sealed class InventorySlot
    {
        public string ItemId = string.Empty;
        public int Quantity;
        public InventoryItemData Data;
        public bool IsEmpty => string.IsNullOrEmpty(ItemId) || Quantity <= 0 || Data == null;

        public void Clear()
        {
            ItemId = string.Empty;
            Quantity = 0;
            Data = null;
        }
    }

    /// <summary>
    /// Owns storage and quick access as two separate concepts. Gameplay systems query by item ID;
    /// presentation chooses storage slots, while the quick bar only stores references.
    /// </summary>
    public sealed class InventoryManager : MonoBehaviour, ISaveable
    {
        public static InventoryManager Instance { get; private set; }

        [Header("Storage")]
        [SerializeField, Min(1)] private int _maxSlots = 20;
        [SerializeField] private ItemCatalog _catalog;

        [Header("Quick Access")]
        [SerializeField, Range(1, 8)] private int _quickAccessCapacity = 4;
        [SerializeField] private bool _autoAssignQuickAccess = true;

        [Header("World Actions")]
        [SerializeField] private Transform _dropOrigin;
        [SerializeField] private bool _logActions = true;

        private InventorySlot[] _slots;
        private string[] _quickItemIds;
        private int _activeQuickIndex;
        private readonly Dictionary<string, InventoryItemData> _itemLookup = new Dictionary<string, InventoryItemData>();

        public int ActiveSlotIndex => _activeQuickIndex;
        public int ActiveQuickIndex => _activeQuickIndex;
        public int MaxSlots => _maxSlots;
        public int QuickAccessCapacity => _quickAccessCapacity;
        public InventorySlot[] Slots => _slots;
        public string[] QuickItemIds => _quickItemIds;

        public event Action<int> OnActiveSlotChanged;
        public event Action OnInventoryChanged;
        public event Action<InventoryItemUseRequest> ItemUseSelectionRequested;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _maxSlots = Mathf.Max(1, _maxSlots);
            _quickAccessCapacity = Mathf.Clamp(_quickAccessCapacity, 1, 8);
            _slots = CreateSlots(_maxSlots);
            _quickItemIds = new string[_quickAccessCapacity];
            BuildCatalog();
            ResolveDropOrigin();
            SaveManager.Instance?.Register(this);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<RequestSetActiveQuickSlot>(HandleSetActiveQuickSlotRequest);
            EventBus.Subscribe<RequestNavigateQuickAccess>(HandleNavigateQuickAccessRequest);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<RequestSetActiveQuickSlot>(HandleSetActiveQuickSlotRequest);
            EventBus.Unsubscribe<RequestNavigateQuickAccess>(HandleNavigateQuickAccessRequest);
        }

        private void HandleSetActiveQuickSlotRequest(RequestSetActiveQuickSlot evt) => SetActiveQuickSlot(evt.slot);
        private void HandleNavigateQuickAccessRequest(RequestNavigateQuickAccess evt) => NavigateQuickAccess(evt.direction);

        private void OnDestroy()
        {
            SaveManager.Instance?.Unregister(this);
            if (Instance == this) Instance = null;
        }

        public void SetActiveSlot(int index) => SetActiveQuickSlot(index);

        public void SetActiveQuickSlot(int index)
        {
            if (_quickItemIds == null || _quickItemIds.Length == 0) return;
            index = (index % _quickItemIds.Length + _quickItemIds.Length) % _quickItemIds.Length;
            if (_activeQuickIndex == index) return;
            _activeQuickIndex = index;
            OnActiveSlotChanged?.Invoke(index);
        }

        public void NavigateQuickAccess(int direction)
        {
            if (direction != 0) SetActiveQuickSlot(_activeQuickIndex + Math.Sign(direction));
        }

        public InventorySlot GetQuickSlot(int quickIndex)
        {
            if (_quickItemIds == null || quickIndex < 0 || quickIndex >= _quickItemIds.Length) return null;
            return FindFirstSlot(_quickItemIds[quickIndex]);
        }

        public InventoryItemData GetActiveItem()
        {
            InventorySlot slot = GetQuickSlot(_activeQuickIndex);
            return slot != null && !slot.IsEmpty ? slot.Data : null;
        }

        public bool AssignQuickSlot(int quickIndex, int storageIndex)
        {
            if (_quickItemIds == null || quickIndex < 0 || quickIndex >= _quickItemIds.Length) return false;
            if (_slots == null || storageIndex < 0 || storageIndex >= _slots.Length || _slots[storageIndex].IsEmpty) return false;
            _quickItemIds[quickIndex] = _slots[storageIndex].ItemId;
            OnInventoryChanged?.Invoke();
            return true;
        }

        public void ClearQuickSlot(int quickIndex)
        {
            if (_quickItemIds == null || quickIndex < 0 || quickIndex >= _quickItemIds.Length) return;
            _quickItemIds[quickIndex] = string.Empty;
            OnInventoryChanged?.Invoke();
        }

        public bool AddItem(InventoryItemData data, int quantity = 1)
        {
            if (data == null || quantity <= 0) return false;
            bool alreadyOwned = HasItem(data.ItemId);
            if (GetAvailableCapacity(data) < quantity)
            {
                Log("Storage is full.");
                return false;
            }

            int remaining = quantity;
            if (data.IsStackable)
            {
                foreach (InventorySlot slot in _slots)
                {
                    if (remaining <= 0) break;
                    if (slot.IsEmpty || slot.ItemId != data.ItemId || slot.Quantity >= data.MaxStack) continue;
                    int amount = Mathf.Min(remaining, data.MaxStack - slot.Quantity);
                    slot.Quantity += amount;
                    remaining -= amount;
                }
            }

            foreach (InventorySlot slot in _slots)
            {
                if (remaining <= 0) break;
                if (!slot.IsEmpty) continue;
                slot.ItemId = data.ItemId;
                slot.Data = data;
                slot.Quantity = data.IsStackable ? Mathf.Min(remaining, data.MaxStack) : 1;
                remaining -= slot.Quantity;
            }

            if (!alreadyOwned && _autoAssignQuickAccess) AutoAssignQuick(data.ItemId);
            NotifyChanged();
            EventBus.Publish(new OnItemPickedUp { itemId = data.ItemId, itemName = data.DisplayName });
            return remaining == 0;
        }

        public bool HasItem(string itemId) => FindFirstSlot(itemId) != null;

        public bool UseItem(string itemId)
        {
            InventorySlot slot = FindFirstSlot(itemId);
            if (slot == null) return false;
            slot.Quantity--;
            if (slot.Quantity <= 0) slot.Clear();
            if (!HasItem(itemId)) ClearQuickReferences(itemId);
            NotifyChanged();
            EventBus.Publish(new OnItemUsed { itemId = itemId });
            return true;
        }

        public bool UseActiveItem()
        {
            InventoryItemData item = GetActiveItem();
            return item != null && UseItem(item.ItemId);
        }

        public bool PerformPrimaryActionAt(int storageIndex)
        {
            InventoryItemData data = GetDataAt(storageIndex);
            if (data == null) return false;

            InventoryPrimaryAction action = data.PrimaryAction;
            if (action == InventoryPrimaryAction.Automatic)
                action = data.IsReadable ? InventoryPrimaryAction.Read
                    : data.WorldPrefab != null ? InventoryPrimaryAction.EquipOrHold
                    : data.IsConsumable ? InventoryPrimaryAction.Consume
                    : InventoryPrimaryAction.None;

            switch (action)
            {
                case InventoryPrimaryAction.Read:
                    EventBus.Publish(new RequestShowNoteReader { content = data.NoteContent });
                    EventBus.Publish(new OnNoteRead { noteId = data.ItemId, content = data.NoteContent });
                    return true;
                case InventoryPrimaryAction.EquipOrHold:
                    return SpawnAndEquipOrHold(data);
                case InventoryPrimaryAction.Consume:
                    return data.IsConsumable && UseItem(data.ItemId);
                default:
                    return false;
            }
        }

        public void PullOutActiveItem()
        {
            InventoryItemData active = GetActiveItem();
            int index = active != null ? FindSlotIndex(active.ItemId) : -1;
            if (index >= 0) PerformPrimaryActionAt(index);
        }

        public bool DropAt(int storageIndex)
        {
            InventoryItemData data = GetDataAt(storageIndex);
            if (data == null || !data.CanDrop || !ResolveDropOrigin()) return false;
            GameObject dropped = Instantiate(data.WorldPrefab, _dropOrigin.position + _dropOrigin.forward * .5f, _dropOrigin.rotation);
            Rigidbody body = dropped.GetComponent<Rigidbody>();
            if (body != null) body.AddForce(_dropOrigin.forward * 2f, ForceMode.Impulse);
            UseItem(data.ItemId);
            return true;
        }

        public void DropActiveItem()
        {
            InventoryItemData active = GetActiveItem();
            if (active != null) DropAt(FindSlotIndex(active.ItemId));
        }

        public bool TryCombine(int sourceIndex, int targetIndex)
        {
            InventoryItemData source = GetDataAt(sourceIndex);
            InventoryItemData target = GetDataAt(targetIndex);
            if (source == null || target == null || sourceIndex == targetIndex) return false;

            foreach (ItemCombination recipe in source.Combinations)
                if (recipe.CombineWith != null && recipe.CombineWith.ItemId == target.ItemId)
                    return ExecuteCombination(source, target, recipe);
            foreach (ItemCombination recipe in target.Combinations)
                if (recipe.CombineWith != null && recipe.CombineWith.ItemId == source.ItemId)
                    return ExecuteCombination(target, source, recipe);
            return false;
        }

        public bool TryCombineWithActive(string targetItemId)
        {
            InventoryItemData active = GetActiveItem();
            return active != null && TryCombine(FindSlotIndex(active.ItemId), FindSlotIndex(targetItemId));
        }

        public ItemUseResult RequestUseOnTarget(IInventoryItemTarget target)
        {
            if (target == null) return ItemUseResult.Rejected;
            if (target.UsePolicy == ItemUsePolicy.SelectedOnly)
            {
                InventoryItemData active = GetActiveItem();
                return active != null && TryApplyItem(target, active.ItemId) ? ItemUseResult.Used : ItemUseResult.NoCompatibleItem;
            }

            InventoryItemData[] candidates = GetUniqueItems().Where(target.AcceptsItem).ToArray();
            if (candidates.Length == 0) return ItemUseResult.NoCompatibleItem;
            if (target.UsePolicy == ItemUsePolicy.AutoUseSingle && candidates.Length == 1)
                return TryApplyItem(target, candidates[0].ItemId) ? ItemUseResult.Used : ItemUseResult.Rejected;

            if (ItemUseSelectionRequested == null) return ItemUseResult.Rejected;
            var request = new InventoryItemUseRequest(this, target, candidates);
            ItemUseSelectionRequested?.Invoke(request);
            return ItemUseResult.OfferedSelection;
        }

        internal bool TryApplyItem(IInventoryItemTarget target, string itemId)
        {
            InventorySlot slot = FindFirstSlot(itemId);
            if (target == null || slot == null || !target.AcceptsItem(slot.Data)) return false;
            if (!target.TryUseItem(slot.Data)) return false;
            if (target.ConsumeItemOnUse) UseItem(itemId);
            return true;
        }

        public InventoryItemData GetItemData(string itemId) => _itemLookup.TryGetValue(itemId, out InventoryItemData item) ? item : null;

        public string SaveId => "InventoryManager";

        public string SaveData()
        {
            var state = new InventorySaveState { activeQuickIndex = _activeQuickIndex };
            foreach (InventorySlot slot in _slots)
            {
                state.slotItemIds.Add(slot.ItemId);
                state.slotQuantities.Add(slot.Quantity);
            }
            state.quickItemIds.AddRange(_quickItemIds);
            return JsonUtility.ToJson(state);
        }

        public void LoadData(string json)
        {
            InventorySaveState state = JsonUtility.FromJson<InventorySaveState>(json);
            if (state == null) return;
            ClearInternal();

            int count = Mathf.Min(_slots.Length, Mathf.Min(state.slotItemIds.Count, state.slotQuantities.Count));
            for (int index = 0; index < count; index++)
            {
                string id = state.slotItemIds[index];
                int quantity = state.slotQuantities[index];
                if (string.IsNullOrWhiteSpace(id) || quantity <= 0 || !_itemLookup.TryGetValue(id, out InventoryItemData data)) continue;
                _slots[index].ItemId = id;
                _slots[index].Quantity = quantity;
                _slots[index].Data = data;
            }

            if (state.version >= 2 && state.quickItemIds != null && state.quickItemIds.Count > 0)
            {
                for (int index = 0; index < Mathf.Min(_quickItemIds.Length, state.quickItemIds.Count); index++)
                    if (HasItem(state.quickItemIds[index])) _quickItemIds[index] = state.quickItemIds[index];
                _activeQuickIndex = Mathf.Clamp(state.activeQuickIndex, 0, _quickItemIds.Length - 1);
            }
            else
            {
                MigrateLegacyQuickAccess(state.activeSlotIndex);
            }

            NotifyChanged();
            OnActiveSlotChanged?.Invoke(_activeQuickIndex);
        }

        public void Clear()
        {
            ClearInternal();
            NotifyChanged();
            OnActiveSlotChanged?.Invoke(_activeQuickIndex);
        }

        private bool ExecuteCombination(InventoryItemData primary, InventoryItemData secondary, ItemCombination recipe)
        {
            if (recipe.DestroyThis && !UseItem(primary.ItemId)) return false;
            if (recipe.DestroyOther && !UseItem(secondary.ItemId)) return false;
            if (recipe.ResultItem != null && !AddItem(recipe.ResultItem))
            {
                Debug.LogError("[Inventory] Combination result could not be stored. Increase storage capacity.");
                return false;
            }
            Log($"Combined {primary.DisplayName} with {secondary.DisplayName}.");
            return true;
        }

        private bool SpawnAndEquipOrHold(InventoryItemData data)
        {
            if (data.WorldPrefab == null || !ResolveDropOrigin()) return false;
            if (PhysicsGrabber.Instance != null && PhysicsGrabber.Instance.IsHoldingObject) return false;

            GameObject spawned = Instantiate(data.WorldPrefab, _dropOrigin.position + _dropOrigin.forward * .5f, _dropOrigin.rotation);
            EquippableItem equippable = spawned.GetComponentInChildren<EquippableItem>();
            if (equippable != null && EquipmentController.Instance != null && EquipmentController.Instance.TryEquip(equippable))
            {
                UseItem(data.ItemId);
                return true;
            }

            PhysicsGrabbable grabbable = spawned.GetComponentInChildren<PhysicsGrabbable>();
            if (grabbable != null && PhysicsGrabber.Instance != null)
            {
                PhysicsGrabber.Instance.Grab(grabbable);
                UseItem(data.ItemId);
                return true;
            }

            Destroy(spawned);
            return false;
        }

        private void MigrateLegacyQuickAccess(int oldActiveStorageIndex)
        {
            int quick = 0;
            if (oldActiveStorageIndex >= 0 && oldActiveStorageIndex < _slots.Length && !_slots[oldActiveStorageIndex].IsEmpty)
                _quickItemIds[quick++] = _slots[oldActiveStorageIndex].ItemId;
            foreach (InventorySlot slot in _slots)
            {
                if (quick >= _quickItemIds.Length) break;
                if (slot.IsEmpty || _quickItemIds.Contains(slot.ItemId)) continue;
                _quickItemIds[quick++] = slot.ItemId;
            }
            _activeQuickIndex = 0;
        }

        private void AutoAssignQuick(string itemId)
        {
            if (_quickItemIds.Contains(itemId)) return;
            for (int index = 0; index < _quickItemIds.Length; index++)
            {
                if (!string.IsNullOrEmpty(_quickItemIds[index])) continue;
                _quickItemIds[index] = itemId;
                return;
            }
        }

        private void ClearQuickReferences(string itemId)
        {
            for (int index = 0; index < _quickItemIds.Length; index++)
                if (_quickItemIds[index] == itemId) _quickItemIds[index] = string.Empty;
        }

        private IEnumerable<InventoryItemData> GetUniqueItems()
        {
            return _slots.Where(slot => !slot.IsEmpty).Select(slot => slot.Data).Distinct();
        }

        private InventoryItemData GetDataAt(int storageIndex)
        {
            return _slots != null && storageIndex >= 0 && storageIndex < _slots.Length && !_slots[storageIndex].IsEmpty
                ? _slots[storageIndex].Data
                : null;
        }

        private int FindSlotIndex(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return -1;
            for (int index = 0; index < _slots.Length; index++)
                if (!_slots[index].IsEmpty && _slots[index].ItemId == itemId) return index;
            return -1;
        }

        private InventorySlot FindFirstSlot(string itemId)
        {
            int index = FindSlotIndex(itemId);
            return index >= 0 ? _slots[index] : null;
        }

        private int GetAvailableCapacity(InventoryItemData data)
        {
            int capacity = 0;
            foreach (InventorySlot slot in _slots)
            {
                if (slot.ItemId == data.ItemId) capacity += Mathf.Max(0, data.MaxStack - slot.Quantity);
                else if (slot.IsEmpty) capacity += data.IsStackable ? data.MaxStack : 1;
            }
            return capacity;
        }

        private bool ResolveDropOrigin()
        {
            if (_dropOrigin != null) return true;
            _dropOrigin = PlayerPlatformRegistry.Current?.Head;
            if (_dropOrigin == null && Camera.main != null) _dropOrigin = Camera.main.transform;
            return _dropOrigin != null;
        }

        private void ClearInternal()
        {
            foreach (InventorySlot slot in _slots) slot.Clear();
            Array.Clear(_quickItemIds, 0, _quickItemIds.Length);
            _activeQuickIndex = 0;
        }

        private void BuildCatalog()
        {
            IEnumerable<InventoryItemData> items = _catalog != null ? _catalog.Items : Resources.LoadAll<InventoryItemData>("Items");
            foreach (InventoryItemData item in items)
                if (item != null && !string.IsNullOrWhiteSpace(item.ItemId)) _itemLookup[item.ItemId] = item;
        }

        private static InventorySlot[] CreateSlots(int count)
        {
            var slots = new InventorySlot[count];
            for (int index = 0; index < count; index++) slots[index] = new InventorySlot();
            return slots;
        }

        private void NotifyChanged() => OnInventoryChanged?.Invoke();
        private void Log(string message) { if (_logActions) Debug.Log($"[Inventory] {message}"); }
    }
}
