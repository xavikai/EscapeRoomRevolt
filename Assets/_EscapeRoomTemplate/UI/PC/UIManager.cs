using EscapeRoomRevolt.Systems.Interaction;
using EscapeRoomRevolt.Systems.Inventory;
using EscapeRoomRevolt.Systems.Puzzle;
using EscapeRoomRevolt.UI.Toolkit;
using UnityEngine;

namespace EscapeRoomRevolt.UI.PC
{
    /// <summary>
    /// Compatibility facade for gameplay systems. Rendering and navigation are owned by
    /// GameplayUIController; keeping this API avoids coupling puzzles to a UI technology.
    /// </summary>
    [DefaultExecutionOrder(-40)]
    public sealed class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        public bool IsUIBlockingGameplay => GameplayUIController.Instance != null && GameplayUIController.Instance.IsBlockingGameplay;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void Start() => LockCursor();
        private void OnDestroy() { if (Instance == this) Instance = null; }

        public void LockCursor()
        {
            if (IsUIBlockingGameplay) return;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void UnlockCursor() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        public void SetCrosshair(CursorType type) => GameplayUIController.Instance?.SetCrosshair(type);
        public void ShowKeypad(CodePanelPuzzle puzzle) => GameplayUIController.Instance?.ShowKeypad(puzzle);
        public void HideKeypad() => GameplayUIController.Instance?.CloseTopPanel();
        public void ShowNoteReader(string content) => GameplayUIController.Instance?.ShowNote(content);
        public void HideNoteReader() => GameplayUIController.Instance?.CloseTopPanel();
        public void ShowItemExaminer(InventoryItemData data) => GameplayUIController.Instance?.ShowItemExaminer(data);
        public void CloseItemExaminer() => GameplayUIController.Instance?.CloseTopPanel();
        public void ShowSubtitle(string text) => GameplayUIController.Instance?.ShowSubtitle(text);
        public void HideSubtitle() => GameplayUIController.Instance?.HideSubtitle();
        public void ToggleInventory() => GameplayUIController.Instance?.ToggleInventory();
        public void CloseTopPanel() => GameplayUIController.Instance?.CloseTopPanel();
        public void TogglePauseMenu() => UIToolkitMenuController.Instance?.TogglePause();
    }
}
