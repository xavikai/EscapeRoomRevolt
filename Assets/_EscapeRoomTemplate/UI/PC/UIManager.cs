using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
        [SerializeField] private GameObject _subtitlePanel;
        [SerializeField] private TextMeshProUGUI _subtitleText;
        
        [Header("Crosshair Settings")]
        [SerializeField] private UnityEngine.UI.Image _crosshairImage;
        [SerializeField] private Sprite _cursorDefault;
        [SerializeField] private Sprite _cursorHand;
        [SerializeField] private Sprite _cursorEye;
        [SerializeField] private Sprite _cursorPuzzle;

        private KeypadUI _keypadUI;
        
        [Header("State")]
        private int _openPanelsCount = 0;

        // Stack to know which panel to close when Escape is pressed
        private readonly Stack<GameObject> _panelStack = new();

        // Subtitle Animation State
        private Coroutine _subtitleAnimCoroutine;
        private Coroutine _typewriterCoroutine;
        private float _subtitleVisibleY;
        private float _subtitleHiddenY;
        private bool _hasInitializedSubtitleY = false;

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

        public void SetCrosshair(EscapeRoomRevolt.Systems.Interaction.CursorType type)
        {
            if (_crosshairImage == null) return;

            Sprite targetSprite = null;
            Color targetColor = Color.white;
            Vector2 targetSize = new Vector2(5, 5); // Default size

            switch (type)
            {
                case EscapeRoomRevolt.Systems.Interaction.CursorType.Hand:
                    targetSprite = _cursorHand;
                    targetColor = Color.yellow; // Fallback
                    targetSize = new Vector2(8, 8);
                    break;
                case EscapeRoomRevolt.Systems.Interaction.CursorType.Eye:
                    targetSprite = _cursorEye;
                    targetColor = new Color(0.4f, 0.8f, 1f); // Light blue fallback
                    targetSize = new Vector2(8, 8);
                    break;
                case EscapeRoomRevolt.Systems.Interaction.CursorType.Puzzle:
                    targetSprite = _cursorPuzzle;
                    targetColor = new Color(1f, 0.4f, 0.4f); // Red fallback
                    targetSize = new Vector2(8, 8);
                    break;
                case EscapeRoomRevolt.Systems.Interaction.CursorType.Default:
                default:
                    targetSprite = _cursorDefault;
                    targetColor = Color.white;
                    targetSize = new Vector2(5, 5);
                    break;
            }

            if (targetSprite != null)
            {
                _crosshairImage.sprite = targetSprite;
                _crosshairImage.color = Color.white; // If we have a sprite, use its normal color
                // You might want to adjust the size to native sprite size here if desired
                _crosshairImage.rectTransform.sizeDelta = new Vector2(32, 32); 
            }
            else
            {
                // Fallback to colored dot
                _crosshairImage.sprite = null;
                _crosshairImage.color = targetColor;
                _crosshairImage.rectTransform.sizeDelta = targetSize;
            }
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

        public void ShowItemExaminer(EscapeRoomRevolt.Systems.Inventory.InventoryItemData dataToExamine)
        {
            if (_itemExaminerPanel == null) return;

            var examiner = _itemExaminerPanel.GetComponent<ItemExaminerUI>();
            if (examiner != null) examiner.Show(dataToExamine);

            RegisterPanelOpened(_itemExaminerPanel);
        }

        public void CloseItemExaminer()
        {
            if (_itemExaminerPanel == null) return;
            // The ItemExaminerUI script disables its own gameObject, so we just register the close
            RegisterPanelClosed();
        }

        public void ShowSubtitle(string text)
        {
            if (_subtitlePanel == null || _subtitleText == null) return;
            
            RectTransform rt = _subtitlePanel.GetComponent<RectTransform>();
            if (rt != null && !_hasInitializedSubtitleY)
            {
                _subtitleVisibleY = rt.anchoredPosition.y;
                _subtitleHiddenY = _subtitleVisibleY - rt.rect.height - 50f;
                _hasInitializedSubtitleY = true;
            }

            if (!_subtitlePanel.activeSelf)
            {
                if (rt != null) rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, _subtitleHiddenY);
                _subtitlePanel.SetActive(true);
                if (_subtitleAnimCoroutine != null) StopCoroutine(_subtitleAnimCoroutine);
                _subtitleAnimCoroutine = StartCoroutine(SlideSubtitlePanel(true));
            }

            if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = StartCoroutine(TypewriterEffect(text));
        }

        public void HideSubtitle()
        {
            if (_subtitlePanel == null || !_subtitlePanel.activeSelf) return;
            
            if (_subtitleAnimCoroutine != null) StopCoroutine(_subtitleAnimCoroutine);
            _subtitleAnimCoroutine = StartCoroutine(SlideSubtitlePanel(false));
        }

        private System.Collections.IEnumerator SlideSubtitlePanel(bool show)
        {
            RectTransform rt = _subtitlePanel.GetComponent<RectTransform>();
            if (rt == null) yield break;

            float duration = 0.3f;
            float time = 0;
            float targetY = show ? _subtitleVisibleY : _subtitleHiddenY;
            float startY = rt.anchoredPosition.y;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                t = t * t * (3f - 2f * t); // Smoothstep
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, Mathf.Lerp(startY, targetY, t));
                yield return null;
            }

            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, targetY);
            if (!show) _subtitlePanel.SetActive(false);
        }

        private System.Collections.IEnumerator TypewriterEffect(string fullText)
        {
            _subtitleText.text = "";
            float timePerChar = 0.03f; // Faster typing
            foreach (char c in fullText)
            {
                _subtitleText.text += c;
                yield return new WaitForSeconds(timePerChar);
            }
        }

        public void ToggleInventory()
        {
            // Hotbar is always visible, no toggling
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
            if (_inventoryPanel) _inventoryPanel.SetActive(true);
            if (_pauseMenuPanel) _pauseMenuPanel.SetActive(false);
            if (_keypadPanel) _keypadPanel.SetActive(false);
            if (_itemExaminerPanel) _itemExaminerPanel.SetActive(false);
            if (_subtitlePanel) _subtitlePanel.SetActive(false);
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
