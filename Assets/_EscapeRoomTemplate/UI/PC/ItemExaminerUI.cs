using System;
using EscapeRoomRevolt.Systems.Inventory;
using EscapeRoomRevolt.UI.Toolkit;
using UnityEngine;

namespace EscapeRoomRevolt.UI.PC
{
    [Obsolete("Item examination is owned by GameplayUIController (UI Toolkit).")]
    public sealed class ItemExaminerUI : MonoBehaviour
    {
        public void Show(InventoryItemData data) => GameplayUIController.Instance?.ShowItemExaminer(data);
    }
}
