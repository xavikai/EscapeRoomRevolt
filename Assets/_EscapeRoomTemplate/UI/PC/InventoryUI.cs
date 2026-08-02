using System;
using EscapeRoomRevolt.UI.Toolkit;
using UnityEngine;

namespace EscapeRoomRevolt.UI.PC
{
    [Obsolete("Inventory rendering is owned by GameplayUIController (UI Toolkit).")]
    public sealed class InventoryUI : MonoBehaviour
    {
        public void ShowTemporarily() => GameplayUIController.Instance?.ShowHotbarTemporarily();
    }
}
