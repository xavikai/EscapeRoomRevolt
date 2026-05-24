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
        
        [Header("State")]
        private int _openPanelsCount = 0;

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
        public void ShowNoteReader()
        {
            _noteReaderPanel.SetActive(true);
            RegisterPanelOpened();
        }

        public void HideNoteReader()
        {
            _noteReaderPanel.SetActive(false);
            RegisterPanelClosed();
        }

        public void ToggleInventory()
        {
            bool isActive = _inventoryPanel.activeSelf;
            _inventoryPanel.SetActive(!isActive);

            if (!isActive) RegisterPanelOpened();
            else RegisterPanelClosed();
        }

        // ── Private Helpers ───────────────────────────────────────────────
        private void HideAllPanels()
        {
            if (_interactionPromptPanel) _interactionPromptPanel.SetActive(true); // Usually always active, just text changes
            if (_noteReaderPanel) _noteReaderPanel.SetActive(false);
            if (_inventoryPanel) _inventoryPanel.SetActive(false);
            if (_pauseMenuPanel) _pauseMenuPanel.SetActive(false);
            _openPanelsCount = 0;
        }

        private void RegisterPanelOpened()
        {
            _openPanelsCount++;
            UnlockCursor();
        }

        private void RegisterPanelClosed()
        {
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
