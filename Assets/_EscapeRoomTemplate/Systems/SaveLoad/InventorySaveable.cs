using System.Collections.Generic;
using UnityEngine;
using EscapeRoomRevolt.Systems.Inventory;

namespace EscapeRoomRevolt.Systems.SaveLoad
{
    /// <summary>
    /// Connects the InventoryManager to the SaveManager.
    /// Attach this to the same GameObject as InventoryManager.
    /// </summary>
    [RequireComponent(typeof(InventoryManager))]
    public class InventorySaveable : MonoBehaviour, ISaveable
    {
        public string SaveId => "PlayerInventory";

        [System.Serializable]
        private class InventorySaveState
        {
            public List<string> itemIds = new List<string>();
            public List<int> quantities = new List<int>();
        }

        public object SaveState()
        {
            var inventory = GetComponent<InventoryManager>();
            var state = new InventorySaveState();

            foreach (var itemId in inventory.GetAllItemIds())
            {
                state.itemIds.Add(itemId);
                state.quantities.Add(inventory.GetQuantity(itemId));
            }

            return state;
        }

        public void LoadState(object state)
        {
            if (state is string stateJson)
            {
                var loadedState = JsonUtility.FromJson<InventorySaveState>(stateJson);
                var inventory = GetComponent<InventoryManager>();
                
                inventory.Clear(); // Empty current inventory

                for (int i = 0; i < loadedState.itemIds.Count; i++)
                {
                    string itemId = loadedState.itemIds[i];
                    int qty = loadedState.quantities[i];
                    
                    // In a real project you'd likely want to fetch the InventoryItemData from an Addressables catalog or Resources
                    // For the sake of this framework, we expect a robust item registry or Addressables to resolve strings to ScriptableObjects
                    // For now, this is a placeholder where you'd inject the loaded data logic.
                    Debug.Log($"[InventorySaveable] Need to load {qty}x of {itemId}");
                }
            }
        }
    }
}
