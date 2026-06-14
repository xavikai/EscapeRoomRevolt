using UnityEngine;
using UnityEngine.UI;
using EscapeRoomRevolt.Systems.Inventory;
using TMPro;

namespace EscapeRoomRevolt.UI.PC
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private GameObject _itemSlotPrefab; 

        [Header("Colors")]
        [SerializeField] private Color _activeColor = new Color(1f, 0.9f, 0.2f, 1f); // Yellowish
        [SerializeField] private Color _inactiveColor = new Color(0f, 0f, 0f, 0.3f); // Dark semi-transparent

        [Header("Auto-Fade")]
        [SerializeField] private float _showDuration = 3f;
        [SerializeField] private float _fadeSpeed = 5f;
        
        private GameObject[] _spawnedSlots;
        private CanvasGroup _canvasGroup;
        private RectTransform _itemsContainerRt;
        private float _timeUntilFade = 0f;
        
        private float _visibleY = 40f;
        private float _hiddenY = -150f;

        private void Update()
        {
            if (_canvasGroup == null || _itemsContainerRt == null) return;

            float dt = Time.deltaTime;
            if (_timeUntilFade > 0f)
            {
                _timeUntilFade -= dt;
                _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, 1f, dt * _fadeSpeed);
                
                var pos = _itemsContainerRt.anchoredPosition;
                pos.y = Mathf.Lerp(pos.y, _visibleY, dt * _fadeSpeed);
                _itemsContainerRt.anchoredPosition = pos;
            }
            else
            {
                _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, 0f, dt * _fadeSpeed);
                
                var pos = _itemsContainerRt.anchoredPosition;
                pos.y = Mathf.Lerp(pos.y, _hiddenY, dt * _fadeSpeed);
                _itemsContainerRt.anchoredPosition = pos;
            }
        }

        public void ShowTemporarily()
        {
            _timeUntilFade = _showDuration;
        }

        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            _canvasGroup.alpha = 0f; // Start hidden

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged += RefreshUI;
                InventoryManager.Instance.OnActiveSlotChanged += UpdateActiveHighlight;
                
                StartCoroutine(InitializeSlotsRoutine());
            }
        }

        private void OnDestroy()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged -= RefreshUI;
                InventoryManager.Instance.OnActiveSlotChanged -= UpdateActiveHighlight;
            }
        }

        private System.Collections.IEnumerator InitializeSlotsRoutine()
        {
            // Make sure the main panel fills the screen so the hotbar is centered
            var panelRt = transform.GetComponent<RectTransform>();
            if (panelRt != null)
            {
                panelRt.anchorMin = Vector2.zero;
                panelRt.anchorMax = Vector2.one;
                panelRt.offsetMin = Vector2.zero;
                panelRt.offsetMax = Vector2.zero;
            }

            // Disable background image if it exists so it doesn't cover the screen
            var panelImg = transform.GetComponent<UnityEngine.UI.Image>();
            if (panelImg != null) panelImg.enabled = false;

            // Dynamically transform the container into a bottom Hotbar
            _itemsContainerRt = _itemsContainer.GetComponent<RectTransform>();
            if (_itemsContainerRt != null)
            {
                var oldLayouts = _itemsContainer.GetComponents<UnityEngine.UI.LayoutGroup>();
                bool waitNeeded = false;
                foreach (var layout in oldLayouts)
                {
                    if (!(layout is UnityEngine.UI.GridLayoutGroup))
                    {
                        Destroy(layout);
                        waitNeeded = true;
                    }
                }

                if (waitNeeded)
                {
                    yield return new WaitForEndOfFrame();
                }

                var grid = _itemsContainer.GetComponent<UnityEngine.UI.GridLayoutGroup>();
                if (grid == null) grid = _itemsContainer.gameObject.AddComponent<UnityEngine.UI.GridLayoutGroup>();
                
                grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedRowCount;
                grid.constraintCount = 1;
                grid.childAlignment = TextAnchor.MiddleCenter;
                grid.startAxis = UnityEngine.UI.GridLayoutGroup.Axis.Horizontal;
                if (grid.cellSize.x < 10) grid.cellSize = new Vector2(80, 80);
                grid.spacing = new Vector2(10, 0);

                _itemsContainerRt.anchorMin = new Vector2(0.5f, 0f);
                _itemsContainerRt.anchorMax = new Vector2(0.5f, 0f);
                _itemsContainerRt.pivot = new Vector2(0.5f, 0f);
                _itemsContainerRt.anchoredPosition = new Vector2(0, _hiddenY); // Start hidden
                _itemsContainerRt.sizeDelta = new Vector2(600, 100);
            }

            int maxSlots = InventoryManager.Instance.MaxSlots;
            _spawnedSlots = new GameObject[maxSlots];

            // Clear old slots from editor
            foreach (Transform child in _itemsContainer)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < maxSlots; i++)
            {
                var slotGo = Instantiate(_itemSlotPrefab, _itemsContainer);
                slotGo.name = $"Slot_{i+1}";
                _spawnedSlots[i] = slotGo;

                // Make it clickable
                int index = i; 
                var btn = slotGo.GetComponent<Button>();
                if (btn == null) btn = slotGo.AddComponent<Button>();
                btn.onClick.AddListener(() => OnSlotClicked(index));
                
                var texts = slotGo.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var t in texts)
                {
                    if (t.gameObject.name == "HotkeyText")
                    {
                        t.text = (i + 1).ToString();
                    }
                    else if (t.gameObject.name == "NameText" || t.gameObject.name == "Text")
                    {
                        var trt = t.GetComponent<RectTransform>();
                        if (trt != null)
                        {
                            trt.anchorMin = new Vector2(0.5f, 0f);
                            trt.anchorMax = new Vector2(0.5f, 0f);
                            trt.pivot = new Vector2(0.5f, 1f); // Top center pivot
                            trt.anchoredPosition = new Vector2(0, -5f); // 5 units below slot
                            trt.sizeDelta = new Vector2(80, 40); // Match slot width, height for 2 lines
                            t.fontSize = 14;
                            t.alignment = TextAlignmentOptions.Top;
                            t.textWrappingMode = TextWrappingModes.Normal;
                            t.overflowMode = TextOverflowModes.Ellipsis;
                        }
                    }
                }
            }

            RefreshUI();
            UpdateActiveHighlight(InventoryManager.Instance.ActiveSlotIndex);
        }

        private void RefreshUI()
        {
            var slots = InventoryManager.Instance.Slots;
            if (slots == null || _spawnedSlots == null) return;

            ShowTemporarily();

            for (int i = 0; i < slots.Length; i++)
            {
                if (i >= _spawnedSlots.Length) break;

                var slotGo = _spawnedSlots[i];
                var data = slots[i].Data;

                var texts = slotGo.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var t in texts)
                {
                    if (t.gameObject.name == "QtyText")
                    {
                        if (!slots[i].IsEmpty && slots[i].Quantity > 1)
                            t.text = slots[i].Quantity.ToString();
                        else
                            t.text = "";
                    }
                    else if (t.gameObject.name != "HotkeyText")
                    {
                        // It's the NameText (or similar)
                        if (!slots[i].IsEmpty)
                            t.text = data.DisplayName;
                        else
                            t.text = "";
                    }
                }

                var images = slotGo.GetComponentsInChildren<Image>();
                foreach (var imgComp in images)
                {
                    if (imgComp.gameObject.name == "Icon")
                    {
                        if (!slots[i].IsEmpty && data.Icon != null)
                        {
                            imgComp.sprite = data.Icon;
                            imgComp.color = Color.white;
                        }
                        else
                        {
                            imgComp.sprite = null;
                            imgComp.color = Color.clear;
                        }
                    }
                }
            }
        }

        private void UpdateActiveHighlight(int activeIndex)
        {
            if (_spawnedSlots == null) return;

            ShowTemporarily();

            // Sleek colors for our hotbar
            Color activeColor = new Color(0.8f, 0.7f, 0.2f, 0.9f); // Elegant gold/yellow
            Color inactiveColor = new Color(0.15f, 0.15f, 0.15f, 0.8f); // Dark semi-transparent grey

            for (int i = 0; i < _spawnedSlots.Length; i++)
            {
                var slotGo = _spawnedSlots[i];
                
                // Remove the ugly outline if it exists
                var outline = slotGo.GetComponent<UnityEngine.UI.Outline>();
                if (outline != null) Destroy(outline);

                // Update background color (assuming the root has an Image)
                var bgImage = slotGo.GetComponent<Image>();
                if (bgImage != null)
                {
                    bgImage.color = (i == activeIndex) ? activeColor : inactiveColor;
                }

                // Make the active slot slightly larger for a premium feel
                slotGo.transform.localScale = (i == activeIndex) ? new Vector3(1.1f, 1.1f, 1.1f) : Vector3.one;
            }
        }

        private void OnSlotClicked(int index)
        {
            // If clicking the currently active slot, try to inspect it
            if (InventoryManager.Instance.ActiveSlotIndex == index)
            {
                var data = InventoryManager.Instance.GetActiveItem();
                if (data != null)
                {
                    if (data.IsReadable)
                        UIManager.Instance.ShowNoteReader(data.NoteContent);
                    else if (data.WorldPrefab != null)
                        UIManager.Instance.ShowItemExaminer(data);
                }
            }
            else
            {
                // Just switch active slot
                InventoryManager.Instance.SetActiveSlot(index);
            }
        }
    }
}
