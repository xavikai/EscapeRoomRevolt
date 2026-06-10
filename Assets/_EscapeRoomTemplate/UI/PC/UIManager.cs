using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomRevolt.UI.PC
{
    /// <summary>
    /// Central manager for UI screens in PC mode.
    /// Handles showing/hiding different panels (Pause Menu, Inventory, etc.)
    /// and managing the cursor state.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject _interactionPromptPanel;
        [SerializeField] private GameObject _noteReaderPanel;
        [SerializeField] private GameObject _inventoryPanel;
        [SerializeField] private GameObject _pauseMenuPanel;
        [SerializeField] private GameObject _keypadPanel;
        [SerializeField] private GameObject _itemExaminerPanel;
        
        private KeypadUI _keypadUI;
        
        [Header("State")]
        private int _openPanelsCount = 0;

        // Stack to know which panel to close when Escape is pressed
        private readonly Stack<GameObject> _panelStack = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Ensure UI is clean on start
            HideAllPanels();
            LockCursor();
        }

        // ── Cursor Management ─────────────────────────────────────────────
        public void LockCursor()
        {
            if (_openPanelsCount > 0) return; // Don't lock if panels are open
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // ── Panel Toggles ─────────────────────────────────────────────────
        public void ShowKeypad(EscapeRoomRevolt.Systems.Puzzle.CodePanelPuzzle puzzle)
        {
            if (_keypadPanel == null) return;
            if (_keypadUI == null) _keypadUI = _keypadPanel.GetComponentInChildren<KeypadUI>();
            if (_keypadUI != null) _keypadUI.Setup(puzzle);
            
            _keypadPanel.SetActive(true);
            RegisterPanelOpened(_keypadPanel);
        }

        public void HideKeypad()
        {
            if (_keypadPanel == null) return;
            _keypadPanel.SetActive(false);
            RegisterPanelClosed();
        }

        public void ShowNoteReader(string content)
        {
            if (_noteReaderPanel == null) return;
            
            var reader = _noteReaderPanel.GetComponent<NoteReaderUI>();
            if (reader != null) reader.DisplayText(content);
            
            _noteReaderPanel.SetActive(true);
            RegisterPanelOpened(_noteReaderPanel);
        }

        public void HideNoteReader()
        {
            if (_noteReaderPanel == null) return;
            _noteReaderPanel.SetActive(false);
            RegisterPanelClosed();
        }

        public void ShowItemExaminer(GameObject prefabToExamine)
        {
            if (_itemExaminerPanel == null) return;

            var examiner = _itemExaminerPanel.GetComponent<ItemExaminerUI>();
            if (examiner != null) examiner.Show(prefabToExamine);

            RegisterPanelOpened(_itemExaminerPanel);
        }

        public void CloseItemExaminer()
        {
            if (_itemExaminerPanel == null) return;
            // The ItemExaminerUI script disables its own gameObject, so we just register the close
            RegisterPanelClosed();
        }

        public void ToggleInventory()
        {
            if (_inventoryPanel == null) return;

            // Prevent toggling the inventory if another panel (like the Examiner) is currently the active top panel.
            // This forces the player to press Esc to close the top panel first.
            if (_panelStack.Count > 0 && _panelStack.Peek() != _inventoryPanel)
                return;

            bool isActive = _inventoryPanel.activeSelf;
            _inventoryPanel.SetActive(!isActive);

            if (!isActive) RegisterPanelOpened(_inventoryPanel);
            else           RegisterPanelClosed();
        }

        public void TogglePauseMenu()
        {
            if (_pauseMenuPanel == null) return;
            bool isActive = _pauseMenuPanel.activeSelf;
            _pauseMenuPanel.SetActive(!isActive);

            if (!isActive) RegisterPanelOpened(_pauseMenuPanel);
            else           RegisterPanelClosed();
        }

        /// <summary>
        /// Closes the most recently opened UI panel (used by Escape key).
        /// </summary>
        public void CloseTopPanel()
        {
            if (_panelStack.Count == 0) return;
            GameObject top = _panelStack.Pop();
            top.SetActive(false);
            _openPanelsCount--;
            if (_openPanelsCount <= 0)
            {
                _openPanelsCount = 0;
                LockCursor();
            }
        }

        // ── Private Helpers ───────────────────────────────────────────────
        private void HideAllPanels()
        {
            if (_interactionPromptPanel) _interactionPromptPanel.SetActive(true); // Usually always active, just text changes
            if (_noteReaderPanel) _noteReaderPanel.SetActive(false);
            if (_inventoryPanel) _inventoryPanel.SetActive(false);
            if (_pauseMenuPanel) _pauseMenuPanel.SetActive(false);
            if (_keypadPanel) _keypadPanel.SetActive(false);
            if (_itemExaminerPanel) _itemExaminerPanel.SetActive(false);
            _openPanelsCount = 0;
            _panelStack.Clear();
        }

        private void RegisterPanelOpened(GameObject panel)
        {
            _openPanelsCount++;
            _panelStack.Push(panel);
            UnlockCursor();
        }

        private void RegisterPanelClosed()
        {
            if (_panelStack.Count > 0) _panelStack.Pop();
            _openPanelsCount--;
            if (_openPanelsCount <= 0)
            {
                _openPanelsCount = 0;
                LockCursor();
            }
        }
        
        /// <summary>
        /// True if any full-screen UI is open (meaning the player shouldn't be able to move or look around)
        /// </summary>
        public bool IsUIBlockingGameplay => _openPanelsCount > 0;
    }
}
