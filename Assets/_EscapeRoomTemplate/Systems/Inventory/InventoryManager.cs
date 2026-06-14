using System;
using System.Collections.Generic;
using UnityEngine;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Core.Save;

namespace EscapeRoomRevolt.Systems.Inventory
{
    [System.Serializable]
    public class InventorySaveState
    {
        public List<string> slotItemIds = new List<string>();
        public List<int> slotQuantities = new List<int>();
        public int activeSlotIndex = 0;
    }

    [System.Serializable]
    public class InventorySlot
    {
        public string ItemId = "";
        public int Quantity = 0;
        public InventoryItemData Data = null;

        public bool IsEmpty => string.IsNullOrEmpty(ItemId) || Quantity <= 0;

        public void Clear()
        {
            ItemId = "";
            Quantity = 0;
            Data = null;
        }
    }

    public class InventoryManager : MonoBehaviour, ISaveable
    {
        public static InventoryManager Instance { get; private set; }

        [Header("Hotbar Settings")]
        [SerializeField] private int _maxSlots = 6;
        [SerializeField] private bool _logActions = true;
        [Tooltip("The transform from where dropped items spawn (usually camera).")]
        [SerializeField] private Transform _dropOrigin;
        
        private InventorySlot[] _slots;
        private int _activeSlotIndex = 0;
        
        public int ActiveSlotIndex => _activeSlotIndex;
        public int MaxSlots => _maxSlots;
        public InventorySlot[] Slots => _slots;

        // Events
        public event Action<int> OnActiveSlotChanged; // Passes new active slot index
        public event Action OnInventoryChanged; // General refresh

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            _slots = new InventorySlot[_maxSlots];
            for (int i = 0; i < _maxSlots; i++) _slots[i] = new InventorySlot();

            SaveManager.Instance?.Register(this);

            if (_dropOrigin == null && Camera.main != null)
                _dropOrigin = Camera.main.transform;
        }

        private void Update()
        {
            if (EscapeRoomRevolt.UI.PC.UIManager.Instance != null && EscapeRoomRevolt.UI.PC.UIManager.Instance.IsUIBlockingGameplay)
                return;

            HandleInput();
        }

        private void HandleInput()
        {
            // Number keys 1-6
            for (int i = 0; i < _maxSlots; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SetActiveSlot(i);
                }
            }

            // Scroll wheel
            float scroll = Input.mouseScrollDelta.y;
            if (scroll > 0.1f)
            {
                SetActiveSlot(_activeSlotIndex - 1);
            }
            else if (scroll < -0.1f)
            {
                SetActiveSlot(_activeSlotIndex + 1);
            }

            // Drop Item
            if (Input.GetKeyDown(KeyCode.Q))
            {
                DropActiveItem();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            SaveManager.Instance?.Unregister(this);
        }

        public void SetActiveSlot(int index)
        {
            if (index < 0) index = _maxSlots - 1;
            if (index >= _maxSlots) index = 0;

            if (_activeSlotIndex != index)
            {
                _activeSlotIndex = index;
                OnActiveSlotChanged?.Invoke(_activeSlotIndex);
            }
        }

        public InventoryItemData GetActiveItem()
        {
            return _slots[_activeSlotIndex].Data;
        }

        // ── Save / Load ──────────────────────────────────────────────────────
        public string SaveId => "InventoryManager";

        public string SaveData()
        {
            var state = new InventorySaveState();
            state.activeSlotIndex = _activeSlotIndex;
            for (int i = 0; i < _maxSlots; i++)
            {
                state.slotItemIds.Add(_slots[i].ItemId);
                state.slotQuantities.Add(_slots[i].Quantity);
            }
            return JsonUtility.ToJson(state);
        }

        public void LoadData(string json)
        {
            var state = JsonUtility.FromJson<InventorySaveState>(json);
            if (state == null) return;

            Clear();

            InventoryItemData[] allItems = Resources.LoadAll<InventoryItemData>("Items");
            Dictionary<string, InventoryItemData> catalog = new Dictionary<string, InventoryItemData>();
            foreach (var item in allItems)
            {
                if (item != null && !string.IsNullOrEmpty(item.ItemId))
                    catalog[item.ItemId] = item;
            }

            int count = Mathf.Min(_maxSlots, state.slotItemIds.Count);
            for (int i = 0; i < count; i++)
            {
                string id = state.slotItemIds[i];
                int qty = state.slotQuantities[i];

                if (!string.IsNullOrEmpty(id) && qty > 0 && catalog.TryGetValue(id, out InventoryItemData data))
                {
                    _slots[i].ItemId = id;
                    _slots[i].Quantity = qty;
                    _slots[i].Data = data;
                }
            }

            SetActiveSlot(state.activeSlotIndex);
            OnInventoryChanged?.Invoke();
        }

        // ── Public API ───────────────────────────────────────────────────────
        public bool AddItem(InventoryItemData data, int quantity = 1)
        {
            if (data == null) return false;

            // Try to stack first
            if (data.IsStackable)
            {
                for (int i = 0; i < _maxSlots; i++)
                {
                    if (_slots[i].ItemId == data.ItemId && _slots[i].Quantity < data.MaxStack)
                    {
                        _slots[i].Quantity = Mathf.Min(_slots[i].Quantity + quantity, data.MaxStack);
                        Log($"Stacked: {data.DisplayName} (x{quantity}) in slot {i}");
                        OnInventoryChanged?.Invoke();
                        
                        EventBus.Publish(new OnItemPickedUp { itemId = data.ItemId, itemName = data.DisplayName });
                        return true;
                    }
                }
            }

            // Find empty slot
            for (int i = 0; i < _maxSlots; i++)
            {
                if (_slots[i].IsEmpty)
                {
                    _slots[i].ItemId = data.ItemId;
                    _slots[i].Data = data;
                    _slots[i].Quantity = quantity;
                    Log($"Added: {data.DisplayName} to slot {i}");
                    
                    // If hotbar was empty, maybe set this as active? Not strictly necessary but good UX
                    if (_slots[_activeSlotIndex].IsEmpty && i != _activeSlotIndex)
                        SetActiveSlot(i);

                    OnInventoryChanged?.Invoke();
                    EventBus.Publish(new OnItemPickedUp { itemId = data.ItemId, itemName = data.DisplayName });
                    return true;
                }
            }

            Log("Inventory full!");
            return false;
        }

        public bool UseActiveItem()
        {
            if (_slots[_activeSlotIndex].IsEmpty) return false;
            
            string id = _slots[_activeSlotIndex].ItemId;
            
            _slots[_activeSlotIndex].Quantity--;
            if (_slots[_activeSlotIndex].Quantity <= 0)
            {
                _slots[_activeSlotIndex].Clear();
            }

            Log($"Used active item: {id}");
            OnInventoryChanged?.Invoke();
            EventBus.Publish(new OnItemUsed { itemId = id });

            return true;
        }

        public void DropActiveItem()
        {
            if (_slots[_activeSlotIndex].IsEmpty) return;

            InventoryItemData data = _slots[_activeSlotIndex].Data;
            
            if (data.WorldPrefab != null && _dropOrigin != null)
            {
                GameObject dropped = Instantiate(data.WorldPrefab, _dropOrigin.position + _dropOrigin.forward * 0.5f, _dropOrigin.rotation);
                
                // Add tiny physical impulse if possible
                Rigidbody rb = dropped.GetComponent<Rigidbody>();
                if (rb != null) rb.AddForce(_dropOrigin.forward * 2f, ForceMode.Impulse);
                
                Log($"Dropped {data.DisplayName}");
            }
            else
            {
                Log($"Could not drop {data.DisplayName} because it has no WorldPrefab.");
            }

            UseActiveItem(); // Removes it from inventory
        }

        // Retro-compatibility methods (might be removed later as puzzles update)
        public bool UseItem(string itemId)
        {
            for (int i = 0; i < _maxSlots; i++)
            {
                if (_slots[i].ItemId == itemId)
                {
                    _slots[i].Quantity--;
                    if (_slots[i].Quantity <= 0) _slots[i].Clear();
                    
                    OnInventoryChanged?.Invoke();
                    EventBus.Publish(new OnItemUsed { itemId = itemId });
                    return true;
                }
            }
            return false;
        }

        public bool HasItem(string itemId)
        {
            for (int i = 0; i < _maxSlots; i++)
            {
                if (!_slots[i].IsEmpty && _slots[i].ItemId == itemId) return true;
            }
            return false;
        }

        // Combines an item in the inventory with the ACTIVE item (used by Option A logic)
        public bool TryCombineWithActive(string targetItemId)
        {
            if (_slots[_activeSlotIndex].IsEmpty) return false;
            
            var dataA = _slots[_activeSlotIndex].Data;
            var dataB = GetItemData(targetItemId); // The item we clicked or are examining

            if (dataB == null) return false;

            // Check A's recipes
            foreach (var combo in dataA.Combinations)
            {
                if (combo.CombineWith != null && combo.CombineWith.ItemId == dataB.ItemId)
                {
                    ExecuteCombination(dataA, dataB, combo);
                    return true;
                }
            }

            // Check B's recipes
            foreach (var combo in dataB.Combinations)
            {
                if (combo.CombineWith != null && combo.CombineWith.ItemId == dataA.ItemId)
                {
                    ExecuteCombination(dataB, dataA, combo);
                    return true;
                }
            }

            Log($"Cannot combine {dataA.DisplayName} with {dataB.DisplayName}");
            return false;
        }

        private void ExecuteCombination(InventoryItemData primary, InventoryItemData secondary, ItemCombination combo)
        {
            // Destroy requirements
            if (combo.DestroyThis) UseItem(primary.ItemId);
            if (combo.DestroyOther) UseItem(secondary.ItemId);

            if (combo.ResultItem != null)
            {
                AddItem(combo.ResultItem, 1);
            }

            Log($"Successfully combined {primary.DisplayName} and {secondary.DisplayName}");
        }

        public InventoryItemData GetItemData(string itemId)
        {
            InventoryItemData[] allItems = Resources.LoadAll<InventoryItemData>("Items");
            foreach (var item in allItems)
            {
                if (item.ItemId == itemId) return item;
            }
            return null;
        }

        public void Clear()
        {
            for (int i = 0; i < _maxSlots; i++) _slots[i].Clear();
            _activeSlotIndex = 0;
            Log("Inventory cleared.");
            OnInventoryChanged?.Invoke();
            OnActiveSlotChanged?.Invoke(_activeSlotIndex);
        }

        private void Log(string message)
        {
            if (_logActions) Debug.Log($"[Inventory] {message}");
        }
    }
}
