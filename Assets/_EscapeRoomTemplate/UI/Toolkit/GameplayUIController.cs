using System;
using System.Collections;
using EscapeRoomRevolt.Core.Settings;
using EscapeRoomRevolt.Systems.Equipment;
using EscapeRoomRevolt.Systems.Interaction;
using EscapeRoomRevolt.Systems.Inventory;
using EscapeRoomRevolt.Systems.Puzzle;
using EscapeRoomRevolt.Systems.Survival;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace EscapeRoomRevolt.UI.Toolkit
{
    public enum GameplayModal { None, Inventory, Note, Keypad, Examiner }

    [RequireComponent(typeof(UIDocument))]
    public sealed class GameplayUIController : MonoBehaviour
    {
        public static GameplayUIController Instance { get; private set; }

        [Header("Examine render")]
        [SerializeField] private RenderTexture _examineTexture;
        private bool _ownsExamineTexture;
        [SerializeField] private float _examineRotationSpeed = .35f;
        [SerializeField] private float _examineZoomSpeed = .08f;

        private UIDocument _document;
        private VisualElement _root;
        private VisualElement _crosshair;
        private Label _interactionPrompt;
        private VisualElement _flashlightHud;
        private VisualElement _flashlightFill;
        private Label _flashlightPercent;
        private Label _flashlightState;
        private VisualElement _sanityHud;
        private VisualElement _sanityFill;
        private Label _sanityPercent;
        private Label _sanityState;
        private VisualElement _hotbar;
        private Label _subtitle;
        private VisualElement _modalLayer;
        private VisualElement _inventoryPanel;
        private VisualElement _inventoryGrid;
        private Image _inventoryDetailIcon;
        private Label _inventoryDetailName;
        private Label _inventoryDetailDescription;
        private Button _inventoryUse;
        private Button _inventoryExamine;
        private Button _inventoryCombine;
        private Button _inventoryDrop;
        private Button _inventoryQuickAssign;
        private VisualElement _notePanel;
        private Label _noteContent;
        private VisualElement _keypadPanel;
        private Label _keypadDisplay;
        private VisualElement _keypadGrid;
        private VisualElement _examinerPanel;
        private Image _examinerImage;
        private Label _examinerTitle;
        private Label _examinerDescription;

        private GameplayModal _modal;
        private CodePanelPuzzle _currentPuzzle;
        private InventoryItemData _examinedData;
        private int _selectedInventoryIndex;
        private int _combineSourceIndex = -1;
        private InventoryItemUseRequest _itemUseRequest;
        private float _hotbarVisibleUntil;
        private Coroutine _subtitleRoutine;
        private IInteractable _displayedPromptTarget;
        private PhysicsGrabbable _displayedHeldObject;

        private InventoryManager _inventory;
        private EquipmentController _equipment;
        private FlashlightController _flashlight;
        private SanityController _sanity;
        private bool _servicesBound;

        private GameObject _examineRig;
        private Transform _examineSpawn;
        private Camera _examineCamera;
        private GameObject _examinedModel;
        private Vector3 _examinedBaseScale = Vector3.one;
        private float _examinedZoom = 1f;
        private bool _examinerDragging;
        private Vector2 _lastPointerPosition;
        private Vector2 _pointerDownPosition;
        private string _examinerBaseDescription;
        private ExamineHotspot _hoveredHotspot;

        public bool IsBlockingGameplay => _modal != GameplayModal.None;
        public GameplayModal CurrentModal => _modal;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _document = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            CacheElements();
            RegisterCallbacks();
            HideAll();
        }

        private void Start()
        {
            BindServices();
            RefreshInventory();
            RefreshFlashlight();
            RefreshSanity();
        }

        private void Update()
        {
            if (!_servicesBound) BindServices();
            UpdateInteractionPrompt();
            UpdateHotbarVisibility();
            HandleKeypadKeyboard();
        }

        private void OnDisable() => UnbindServices();

        private void OnDestroy()
        {
            if (_subtitleRoutine != null) StopCoroutine(_subtitleRoutine);
            DestroyExaminedModel();
            if (_examineRig != null) Destroy(_examineRig);
            if (_ownsExamineTexture && _examineTexture != null && _examineTexture.IsCreated()) _examineTexture.Release();
            if (Instance == this) Instance = null;
        }

        private void CacheElements()
        {
            _root = _document.rootVisualElement;
            _crosshair = _root.Q<VisualElement>("crosshair");
            _interactionPrompt = _root.Q<Label>("interaction-prompt");
            _flashlightHud = _root.Q<VisualElement>("flashlight-hud");
            _flashlightFill = _root.Q<VisualElement>("flashlight-fill");
            _flashlightPercent = _root.Q<Label>("flashlight-percent");
            _flashlightState = _root.Q<Label>("flashlight-state");
            _sanityHud = _root.Q<VisualElement>("sanity-hud");
            _sanityFill = _root.Q<VisualElement>("sanity-fill");
            _sanityPercent = _root.Q<Label>("sanity-percent");
            _sanityState = _root.Q<Label>("sanity-state");
            _hotbar = _root.Q<VisualElement>("hotbar");
            _subtitle = _root.Q<Label>("subtitle");
            _modalLayer = _root.Q<VisualElement>("modal-layer");

            _inventoryPanel = _root.Q<VisualElement>("inventory-panel");
            _inventoryGrid = _root.Q<VisualElement>("inventory-grid");
            _inventoryDetailIcon = _root.Q<Image>("inventory-detail-icon");
            _inventoryDetailName = _root.Q<Label>("inventory-detail-name");
            _inventoryDetailDescription = _root.Q<Label>("inventory-detail-description");
            _inventoryUse = _root.Q<Button>("inventory-use");
            _inventoryExamine = _root.Q<Button>("inventory-examine");
            _inventoryCombine = _root.Q<Button>("inventory-combine");
            _inventoryDrop = _root.Q<Button>("inventory-drop");
            _inventoryQuickAssign = _root.Q<Button>("inventory-quick-assign");

            _notePanel = _root.Q<VisualElement>("note-panel");
            _noteContent = _root.Q<Label>("note-content");
            _keypadPanel = _root.Q<VisualElement>("keypad-panel");
            _keypadDisplay = _root.Q<Label>("keypad-display");
            _keypadGrid = _root.Q<VisualElement>("keypad-grid");
            _examinerPanel = _root.Q<VisualElement>("examiner-panel");
            _examinerImage = _root.Q<Image>("examiner-image");
            _examinerTitle = _root.Q<Label>("examiner-title");
            _examinerDescription = _root.Q<Label>("examiner-description");
        }

        private void RegisterCallbacks()
        {
            _root.Q<Button>("inventory-close").clicked += CloseTopPanel;
            _root.Q<Button>("note-close").clicked += CloseTopPanel;
            _root.Q<Button>("keypad-close").clicked += CloseTopPanel;
            _root.Q<Button>("examiner-close").clicked += CloseTopPanel;
            SetVisible(_root.Q<Button>("examiner-combine"), false);
            _inventoryUse.clicked += UseSelectedInventoryItem;
            _inventoryExamine.clicked += ExamineSelectedInventoryItem;
            _inventoryCombine.clicked += CombineSelectedInventoryItem;
            _inventoryDrop.clicked += DropSelectedInventoryItem;
            if (_inventoryQuickAssign != null) _inventoryQuickAssign.clicked += AssignSelectedToQuickAccess;

            BuildKeypadButtons();
            _examinerImage.RegisterCallback<PointerDownEvent>(OnExaminerPointerDown);
            _examinerImage.RegisterCallback<PointerMoveEvent>(OnExaminerPointerMove);
            _examinerImage.RegisterCallback<PointerUpEvent>(OnExaminerPointerUp);
            _examinerImage.RegisterCallback<WheelEvent>(OnExaminerWheel);
        }

        private void BindServices()
        {
            _inventory = InventoryManager.Instance;
            _equipment = EquipmentController.Instance;
            SanityController sanity = GameFeatures.IsEnabled(OptionalGameFeature.Sanity)
                ? SanityController.Instance
                : null;

            if (_sanity != sanity)
            {
                if (_sanity != null)
                {
                    _sanity.Changed -= OnSanityChanged;
                    _sanity.StageChanged -= OnSanityStageChanged;
                }
                _sanity = sanity;
                if (_sanity != null)
                {
                    _sanity.Changed += OnSanityChanged;
                    _sanity.StageChanged += OnSanityStageChanged;
                }
                RefreshSanity();
            }

            if (_inventory != null)
            {
                _inventory.OnInventoryChanged -= RefreshInventory;
                _inventory.OnInventoryChanged += RefreshInventory;
                _inventory.OnActiveSlotChanged -= OnActiveSlotChanged;
                _inventory.OnActiveSlotChanged += OnActiveSlotChanged;
                _inventory.ItemUseSelectionRequested -= ShowItemUseSelection;
                _inventory.ItemUseSelectionRequested += ShowItemUseSelection;
            }

            if (_equipment != null)
            {
                _equipment.EquipmentChanged -= OnEquipmentChanged;
                _equipment.EquipmentChanged += OnEquipmentChanged;
                OnEquipmentChanged(_equipment.CurrentItem);
            }

            _servicesBound = _inventory != null && _equipment != null;
        }

        private void UnbindServices()
        {
            if (_inventory != null)
            {
                _inventory.OnInventoryChanged -= RefreshInventory;
                _inventory.OnActiveSlotChanged -= OnActiveSlotChanged;
                _inventory.ItemUseSelectionRequested -= ShowItemUseSelection;
            }
            if (_equipment != null) _equipment.EquipmentChanged -= OnEquipmentChanged;
            if (_sanity != null)
            {
                _sanity.Changed -= OnSanityChanged;
                _sanity.StageChanged -= OnSanityStageChanged;
            }
            _sanity = null;
            BindFlashlight(null);
            _servicesBound = false;
        }

        public void SetCrosshair(CursorType type)
        {
            if (_crosshair == null) return;
            _crosshair.RemoveFromClassList("crosshair--hand");
            _crosshair.RemoveFromClassList("crosshair--eye");
            _crosshair.RemoveFromClassList("crosshair--puzzle");
            if (type == CursorType.Hand) _crosshair.AddToClassList("crosshair--hand");
            else if (type == CursorType.Eye) _crosshair.AddToClassList("crosshair--eye");
            else if (type == CursorType.Puzzle) _crosshair.AddToClassList("crosshair--puzzle");
        }

        private void OnSanityChanged(float _) => RefreshSanity();
        private void OnSanityStageChanged(SanityStage _) => RefreshSanity();

        private void RefreshSanity()
        {
            if (_sanityHud == null) return;
            bool visible = GameFeatures.IsEnabled(OptionalGameFeature.Sanity) && _sanity != null;
            SetVisible(_sanityHud, visible);
            if (!visible) return;

            float normalized = Mathf.Clamp01(_sanity.Normalized);
            _sanityFill.style.width = Length.Percent(normalized * 100f);
            _sanityPercent.text = $"{Mathf.RoundToInt(normalized * 100f)}%";
            _sanityState.text = _sanity.Stage switch
            {
                SanityStage.Uneasy => "INQUIETO",
                SanityStage.Distressed => "ALTERADO",
                SanityStage.Critical => "CRÍTICO",
                _ => "ESTABLE"
            };
            _sanityHud.RemoveFromClassList("sanity-hud--uneasy");
            _sanityHud.RemoveFromClassList("sanity-hud--distressed");
            _sanityHud.RemoveFromClassList("sanity-hud--critical");
            if (_sanity.Stage == SanityStage.Uneasy) _sanityHud.AddToClassList("sanity-hud--uneasy");
            else if (_sanity.Stage == SanityStage.Distressed) _sanityHud.AddToClassList("sanity-hud--distressed");
            else if (_sanity.Stage == SanityStage.Critical) _sanityHud.AddToClassList("sanity-hud--critical");
        }

        private void UpdateInteractionPrompt()
        {
            if (_interactionPrompt == null) return;
            if (IsBlockingGameplay)
            {
                SetVisible(_interactionPrompt, false);
                _displayedPromptTarget = null;
                _displayedHeldObject = null;
                return;
            }

            PhysicsGrabber grabber = PhysicsGrabber.Instance;
            if (grabber != null && grabber.IsHoldingObject)
            {
                PhysicsGrabbable heldObject = grabber.CurrentHeldObject;
                if (_displayedHeldObject != heldObject)
                {
                    bool canStore = heldObject != null && heldObject.GetComponent<PickableItem>() != null;
                    _interactionPrompt.text = canStore
                        ? "[CLIC] LANZAR   [BOTÓN DERECHO] ROTAR   [Q] SOLTAR   [E] GUARDAR"
                        : "[CLIC] LANZAR   [BOTÓN DERECHO] ROTAR   [Q] SOLTAR";
                    _displayedHeldObject = heldObject;
                    _displayedPromptTarget = null;
                }
                SetVisible(_interactionPrompt, true);
                return;
            }

            IInteractable target = InteractionManager.Instance != null ? InteractionManager.Instance.CurrentTarget : null;
            bool show = target.IsAlive() && target.CanInteract && UnityEngine.Cursor.lockState == CursorLockMode.Locked;
            if (show && !ReferenceEquals(target, _displayedPromptTarget))
            {
                _interactionPrompt.text = $"[E] {target.InteractionPrompt}";
                _displayedPromptTarget = target;
                _displayedHeldObject = null;
            }
            else if (!show)
            {
                _displayedPromptTarget = null;
                _displayedHeldObject = null;
            }
            SetVisible(_interactionPrompt, show);
        }

        private void OnEquipmentChanged(EquippableItem item)
        {
            BindFlashlight(GameFeatures.IsEnabled(OptionalGameFeature.Flashlight) && item != null
                ? item.GetComponent<FlashlightController>()
                : null);
            RefreshFlashlight();
        }

        private void BindFlashlight(FlashlightController flashlight)
        {
            if (_flashlight != null)
            {
                _flashlight.ChargeChanged -= OnFlashlightChargeChanged;
                _flashlight.StateChanged -= RefreshFlashlight;
            }
            _flashlight = flashlight;
            if (_flashlight != null)
            {
                _flashlight.ChargeChanged += OnFlashlightChargeChanged;
                _flashlight.StateChanged += RefreshFlashlight;
            }
        }

        private void OnFlashlightChargeChanged(float _) => RefreshFlashlight();

        private void RefreshFlashlight()
        {
            if (_flashlightHud == null) return;
            bool visible = GameFeatures.IsEnabled(OptionalGameFeature.Flashlight)
                && _flashlight != null && _flashlight.IsEquipped;
            SetVisible(_flashlightHud, visible);
            if (!visible) return;

            float charge = Mathf.Clamp01(_flashlight.Charge01);
            int percent = Mathf.RoundToInt(charge * 100f);
            _flashlightFill.style.width = Length.Percent(percent);
            _flashlightPercent.text = $"{percent}%";
            _flashlightFill.RemoveFromClassList("battery-fill--low");
            _flashlightFill.RemoveFromClassList("battery-fill--critical");

            if (charge <= .15f)
            {
                _flashlightFill.AddToClassList("battery-fill--critical");
                _flashlightState.text = "CRÍTICA";
            }
            else if (charge <= .35f)
            {
                _flashlightFill.AddToClassList("battery-fill--low");
                _flashlightState.text = "BAJA";
            }
            else _flashlightState.text = _flashlight.IsOn ? "ACTIVA" : "ESPERA";
        }

        public void ShowHotbarTemporarily(float seconds = 3f)
        {
            _hotbarVisibleUntil = Time.unscaledTime + seconds;
            _hotbar?.AddToClassList("hotbar--visible");
        }

        private void UpdateHotbarVisibility()
        {
            if (_hotbar == null) return;
            if (Time.unscaledTime <= _hotbarVisibleUntil || _modal == GameplayModal.Inventory)
                _hotbar.AddToClassList("hotbar--visible");
            else _hotbar.RemoveFromClassList("hotbar--visible");
        }

        private void RefreshInventory()
        {
            if (_inventory == null || _hotbar == null) return;
            _hotbar.Clear();
            for (int index = 0; index < _inventory.QuickAccessCapacity; index++)
                _hotbar.Add(CreateSlot(index, _inventory.GetQuickSlot(index), compact: true));

            if (_modal == GameplayModal.Inventory) RefreshInventoryModal();
            ShowHotbarTemporarily();
        }

        private VisualElement CreateSlot(int index, InventorySlot slot, bool compact)
        {
            var button = new Button { name = $"slot-{index}" };
            button.AddToClassList("slot");
            if (compact && index == _inventory.ActiveQuickIndex) button.AddToClassList("slot--active");
            if (!compact && index == _combineSourceIndex) button.AddToClassList("slot--combine-source");
            if (!compact && _itemUseRequest != null && slot != null && !slot.IsEmpty)
            {
                if (_itemUseRequest.IsCompatible(slot.ItemId)) button.AddToClassList("slot--compatible");
                else button.SetEnabled(false);
            }

            bool hasItem = slot != null && !slot.IsEmpty;
            var icon = new Image { name = "icon", sprite = hasItem ? slot.Data?.Icon : null };
            icon.AddToClassList("slot-icon");
            var key = new Label((index + 1).ToString()); key.AddToClassList("slot-key");
            var name = new Label(hasItem ? slot.Data?.DisplayName : string.Empty); name.AddToClassList("slot-name");
            var quantity = new Label(hasItem && slot.Quantity > 1 ? slot.Quantity.ToString() : string.Empty); quantity.AddToClassList("slot-qty");
            button.Add(icon); button.Add(key); button.Add(name); button.Add(quantity);
            button.clicked += () => SelectInventorySlot(index, compact);
            return button;
        }

        private void OnActiveSlotChanged(int index)
        {
            RefreshInventory();
        }

        private void SelectInventorySlot(int index, bool compact)
        {
            if (compact)
            {
                _inventory.SetActiveQuickSlot(index);
                return;
            }
            _selectedInventoryIndex = index;
            RefreshInventoryModal();
        }

        public void ToggleInventory()
        {
            if (_modal == GameplayModal.Inventory) { CloseTopPanel(); return; }
            _itemUseRequest = null;
            OpenModal(GameplayModal.Inventory, _inventoryPanel);
            _selectedInventoryIndex = FindFirstOccupiedStorageSlot();
            RefreshInventoryModal();
        }

        private void RefreshInventoryModal()
        {
            if (_inventory == null || _inventoryGrid == null) return;
            _inventoryGrid.Clear();
            InventorySlot[] slots = _inventory.Slots;
            for (int i = 0; i < slots.Length; i++) _inventoryGrid.Add(CreateSlot(i, slots[i], compact: false));

            if (_selectedInventoryIndex < 0 || _selectedInventoryIndex >= slots.Length) _selectedInventoryIndex = 0;
            InventorySlot selected = slots[_selectedInventoryIndex];
            InventoryItemData data = selected.IsEmpty ? null : selected.Data;
            _inventoryDetailIcon.sprite = data?.Icon;
            _inventoryDetailName.text = data != null ? data.DisplayName : "RANURA VACÍA";
            _inventoryDetailDescription.RemoveFromClassList("combine-guidance");
            _inventoryDetailDescription.RemoveFromClassList("combine-failure");
            if (_itemUseRequest != null && data != null)
            {
                _inventoryDetailDescription.text = "Este objeto es compatible. Confirma para usarlo aquí.";
                _inventoryDetailDescription.AddToClassList("combine-guidance");
            }
            else if (_combineSourceIndex >= 0)
            {
                _inventoryDetailDescription.text = _selectedInventoryIndex == _combineSourceIndex
                    ? "Selecciona un segundo objeto y pulsa COMBINAR."
                    : "Combinar este objeto con el elemento marcado.";
                _inventoryDetailDescription.AddToClassList("combine-guidance");
            }
            else _inventoryDetailDescription.text = data != null ? data.Description : "No hay ningún objeto almacenado en esta posición.";
            bool normalActions = _combineSourceIndex < 0;
            bool compatibleRequest = _itemUseRequest == null || (data != null && _itemUseRequest.IsCompatible(data.ItemId));
            _inventoryUse.SetEnabled(data != null && normalActions && compatibleRequest);
            _inventoryUse.text = GetPrimaryActionLabel(data);
            _inventoryExamine.SetEnabled(data != null && data.CanExamine && normalActions && _itemUseRequest == null);
            _inventoryDrop.SetEnabled(data != null && data.CanDrop && normalActions && _itemUseRequest == null);
            _inventoryCombine.SetEnabled(data != null && _itemUseRequest == null);
            _inventoryCombine.text = _combineSourceIndex < 0 ? "COMBINAR" : (_selectedInventoryIndex == _combineSourceIndex ? "CANCELAR COMBINACIÓN" : "COMBINAR OBJETOS");
            if (_inventoryQuickAssign != null)
            {
                _inventoryQuickAssign.SetEnabled(data != null && normalActions && _itemUseRequest == null);
                _inventoryQuickAssign.text = $"ACCESO RÁPIDO {_inventory.ActiveQuickIndex + 1}";
            }
        }

        private void UseSelectedInventoryItem()
        {
            if (_inventory == null) return;
            InventoryItemData data = GetSelectedInventoryData();
            if (data == null) return;
            if (_itemUseRequest != null)
            {
                if (_itemUseRequest.TryUse(data.ItemId)) CloseTopPanel();
                return;
            }
            _inventory.PerformPrimaryActionAt(_selectedInventoryIndex);
        }

        private void ExamineSelectedInventoryItem()
        {
            InventoryItemData data = GetSelectedInventoryData();
            if (data != null) ShowItemExaminer(data);
        }

        private void CombineSelectedInventoryItem()
        {
            if (_inventory == null || GetSelectedInventoryData() == null) return;

            if (_combineSourceIndex < 0)
            {
                _combineSourceIndex = _selectedInventoryIndex;
                RefreshInventoryModal();
                return;
            }

            if (_selectedInventoryIndex == _combineSourceIndex)
            {
                _combineSourceIndex = -1;
                RefreshInventoryModal();
                return;
            }

            InventoryItemData target = GetSelectedInventoryData();
            int sourceIndex = _combineSourceIndex;
            _combineSourceIndex = -1;
            bool combined = target != null && _inventory.TryCombine(sourceIndex, _selectedInventoryIndex);
            RefreshInventoryModal();
            _inventoryDetailDescription.text = combined
                ? "Combinación completada. El resultado se ha añadido al inventario."
                : "Estos objetos no pueden combinarse.";
            _inventoryDetailDescription.AddToClassList(combined ? "combine-guidance" : "combine-failure");
        }

        private void DropSelectedInventoryItem()
        {
            if (_inventory == null || GetSelectedInventoryData() == null) return;
            if (!_inventory.DropAt(_selectedInventoryIndex)) return;
            _selectedInventoryIndex = FindFirstOccupiedStorageSlot();
            RefreshInventoryModal();
        }

        private InventoryItemData GetSelectedInventoryData()
        {
            if (_inventory?.Slots == null || _selectedInventoryIndex < 0 || _selectedInventoryIndex >= _inventory.Slots.Length) return null;
            return _inventory.Slots[_selectedInventoryIndex].IsEmpty ? null : _inventory.Slots[_selectedInventoryIndex].Data;
        }

        private int FindFirstOccupiedStorageSlot()
        {
            if (_inventory?.Slots == null) return 0;
            for (int index = 0; index < _inventory.Slots.Length; index++)
                if (!_inventory.Slots[index].IsEmpty) return index;
            return 0;
        }

        private string GetPrimaryActionLabel(InventoryItemData data)
        {
            if (_itemUseRequest != null) return "USAR AQUÍ";
            if (data == null) return "USAR";
            if (data.PrimaryAction == InventoryPrimaryAction.Read || (data.PrimaryAction == InventoryPrimaryAction.Automatic && data.IsReadable)) return "LEER";
            if (data.PrimaryAction == InventoryPrimaryAction.Consume) return "CONSUMIR";
            if (data.PrimaryAction == InventoryPrimaryAction.EquipOrHold || data.WorldPrefab != null) return "EQUIPAR / SOSTENER";
            return "USAR";
        }

        private void AssignSelectedToQuickAccess()
        {
            if (_inventory == null) return;
            _inventory.AssignQuickSlot(_inventory.ActiveQuickIndex, _selectedInventoryIndex);
            RefreshInventoryModal();
        }

        private void ShowItemUseSelection(InventoryItemUseRequest request)
        {
            if (request == null || request.Candidates.Count == 0) return;
            _itemUseRequest = request;
            _combineSourceIndex = -1;
            OpenModal(GameplayModal.Inventory, _inventoryPanel);
            _selectedInventoryIndex = 0;
            for (int index = 0; index < _inventory.Slots.Length; index++)
            {
                InventorySlot slot = _inventory.Slots[index];
                if (!slot.IsEmpty && request.IsCompatible(slot.ItemId))
                {
                    _selectedInventoryIndex = index;
                    break;
                }
            }
            RefreshInventoryModal();
        }

        public void ShowNote(string content)
        {
            OpenModal(GameplayModal.Note, _notePanel);
            _noteContent.text = content ?? string.Empty;
        }

        public void ShowKeypad(CodePanelPuzzle puzzle)
        {
            _currentPuzzle = puzzle;
            OpenModal(GameplayModal.Keypad, _keypadPanel);
            RefreshKeypad();
        }

        public void KeypadDigit(string digit)
        {
            if (_currentPuzzle == null || _currentPuzzle.IsSolved) return;
            _currentPuzzle.InputDigit(digit);
            RefreshKeypad();
            if (_currentPuzzle.IsSolved) StartCoroutine(CloseAfterDelay(1f));
        }

        public void KeypadClear() { _currentPuzzle?.ClearInput(); RefreshKeypad(); }
        public void KeypadSubmit() { _currentPuzzle?.SubmitCode(); RefreshKeypad(); if (_currentPuzzle != null && _currentPuzzle.IsSolved) StartCoroutine(CloseAfterDelay(1f)); }

        private void BuildKeypadButtons()
        {
            _keypadGrid.Clear();
            string[] keys = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "C", "0", "OK" };
            foreach (string key in keys)
            {
                var button = new Button { text = key };
                button.AddToClassList("keypad-button");
                string captured = key;
                button.clicked += () => { if (captured == "C") KeypadClear(); else if (captured == "OK") KeypadSubmit(); else KeypadDigit(captured); };
                _keypadGrid.Add(button);
            }
        }

        private void HandleKeypadKeyboard()
        {
            if (_modal != GameplayModal.Keypad || _currentPuzzle == null) return;
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.digit0Key.wasPressedThisFrame || keyboard.numpad0Key.wasPressedThisFrame) KeypadDigit("0");
            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame) KeypadDigit("1");
            if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame) KeypadDigit("2");
            if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame) KeypadDigit("3");
            if (keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame) KeypadDigit("4");
            if (keyboard.digit5Key.wasPressedThisFrame || keyboard.numpad5Key.wasPressedThisFrame) KeypadDigit("5");
            if (keyboard.digit6Key.wasPressedThisFrame || keyboard.numpad6Key.wasPressedThisFrame) KeypadDigit("6");
            if (keyboard.digit7Key.wasPressedThisFrame || keyboard.numpad7Key.wasPressedThisFrame) KeypadDigit("7");
            if (keyboard.digit8Key.wasPressedThisFrame || keyboard.numpad8Key.wasPressedThisFrame) KeypadDigit("8");
            if (keyboard.digit9Key.wasPressedThisFrame || keyboard.numpad9Key.wasPressedThisFrame) KeypadDigit("9");
            if (keyboard.backspaceKey.wasPressedThisFrame) KeypadClear();
            if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame) KeypadSubmit();
        }

        private void RefreshKeypad()
        {
            _keypadDisplay.text = _currentPuzzle == null ? "----" : (_currentPuzzle.IsSolved ? "OK" : _currentPuzzle.CurrentInput.PadRight(4, '-'));
        }

        public void ShowItemExaminer(InventoryItemData data)
        {
            if (data == null) return;
            _examinedData = data;
            OpenModal(GameplayModal.Examiner, _examinerPanel);
            _examinerTitle.text = data.DisplayName.ToUpperInvariant();
            _examinerBaseDescription = data.Description;
            _examinerDescription.text = _examinerBaseDescription;
            _hoveredHotspot = null;
            CreateExaminedModel(data);
        }

        private void EnsureExamineRig()
        {
            if (_examineRig != null) return;
            int layer = LayerMask.NameToLayer("Examine");
            if (layer < 0) layer = 31;

            _examineRig = new GameObject("RuntimeExamineRig");
            DontDestroyOnLoad(_examineRig);
            _examineRig.transform.position = new Vector3(0f, 1000f, 0f);
            _examineSpawn = new GameObject("SpawnPoint").transform;
            _examineSpawn.SetParent(_examineRig.transform, false);

            var cameraObject = new GameObject("ExamineCamera");
            cameraObject.transform.SetParent(_examineRig.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 0f, -4f);
            cameraObject.transform.LookAt(_examineSpawn);
            _examineCamera = cameraObject.AddComponent<Camera>();
            _examineCamera.clearFlags = CameraClearFlags.SolidColor;
            _examineCamera.backgroundColor = new Color(.018f, .025f, .022f, 1f);
            _examineCamera.cullingMask = 1 << layer;
            _examineCamera.fieldOfView = 28f;

            if (_examineTexture == null)
            {
                _examineTexture = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32) { name = "GameplayExamineTexture" };
                _examineTexture.Create();
                _ownsExamineTexture = true;
            }
            _examineCamera.targetTexture = _examineTexture;
            _examinerImage.image = _examineTexture;

            var lightObject = new GameObject("ExamineKeyLight");
            lightObject.transform.SetParent(_examineRig.transform, false);
            lightObject.transform.localPosition = new Vector3(-2f, 2.5f, -2f);
            lightObject.transform.LookAt(_examineSpawn);
            Light keyLight = lightObject.AddComponent<Light>();
            keyLight.type = LightType.Spot; keyLight.range = 12f; keyLight.intensity = 5f; keyLight.spotAngle = 55f; keyLight.cullingMask = 1 << layer;

            var fillObject = new GameObject("ExamineFillLight");
            fillObject.transform.SetParent(_examineRig.transform, false);
            fillObject.transform.localPosition = new Vector3(2f, -1f, -1f);
            Light fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Point; fill.range = 8f; fill.intensity = 1.8f; fill.color = new Color(.55f, .65f, .8f); fill.cullingMask = 1 << layer;
        }

        private void CreateExaminedModel(InventoryItemData data)
        {
            DestroyExaminedModel();
            if (data.WorldPrefab == null) return;
            EnsureExamineRig();
            _examineCamera.enabled = true;
            _examinedModel = Instantiate(data.WorldPrefab, _examineSpawn.position, Quaternion.identity, _examineSpawn);
            foreach (MonoBehaviour behaviour in _examinedModel.GetComponentsInChildren<MonoBehaviour>(true))
                behaviour.enabled = behaviour is ExamineHotspot;
            foreach (Collider itemCollider in _examinedModel.GetComponentsInChildren<Collider>(true))
                itemCollider.enabled = itemCollider.GetComponent<ExamineHotspot>() != null;
            Rigidbody body = _examinedModel.GetComponent<Rigidbody>(); if (body != null) body.isKinematic = true;
            SetLayerRecursively(_examinedModel, LayerMask.NameToLayer("Examine"));

            Renderer[] renderers = _examinedModel.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                float size = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                float scale = size > .001f ? 1.65f / size : 1f;
                _examinedModel.transform.localScale *= scale;
                bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                _examinedModel.transform.position += _examineSpawn.position - bounds.center;
            }
            _examinedBaseScale = _examinedModel.transform.localScale;
            _examinedZoom = 1f;
        }

        private void DestroyExaminedModel()
        {
            if (_examinedModel != null) Destroy(_examinedModel);
            _examinedModel = null;
            if (_examineCamera != null) _examineCamera.enabled = false;
        }

        private void OnExaminerPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;
            _examinerDragging = true;
            _pointerDownPosition = new Vector2(evt.position.x, evt.position.y);
            _lastPointerPosition = _pointerDownPosition;
            _examinerImage.CapturePointer(evt.pointerId); evt.StopPropagation();
        }

        private void OnExaminerPointerMove(PointerMoveEvent evt)
        {
            if (_examinedModel == null) return;
            Vector2 pointerPosition = new Vector2(evt.position.x, evt.position.y);

            if (_examinerDragging)
            {
                Vector2 delta = pointerPosition - _lastPointerPosition; _lastPointerPosition = pointerPosition;
                _examinedModel.transform.Rotate(Vector3.up, -delta.x * _examineRotationSpeed, Space.World);
                _examinedModel.transform.Rotate(Vector3.right, delta.y * _examineRotationSpeed, Space.World);
                return;
            }

            UpdateHoveredHotspot(pointerPosition);
        }

        private void OnExaminerPointerUp(PointerUpEvent evt)
        {
            if (evt.button != 0) return;
            _examinerDragging = false;
            if (_examinerImage.HasPointerCapture(evt.pointerId)) _examinerImage.ReleasePointer(evt.pointerId);

            Vector2 pointerPosition = new Vector2(evt.position.x, evt.position.y);
            const float clickTolerance = 6f;
            if ((pointerPosition - _pointerDownPosition).sqrMagnitude <= clickTolerance * clickTolerance)
                HandleExaminerClick(pointerPosition);
        }

        /// <summary>Updates which ExamineHotspot (if any) is under the cursor and previews its prompt, without revealing it yet.</summary>
        private void UpdateHoveredHotspot(Vector2 pointerPosition)
        {
            TryRaycastHotspot(pointerPosition, out ExamineHotspot hotspot);
            if (hotspot == _hoveredHotspot) return;
            _hoveredHotspot = hotspot;

            if (hotspot == null) { _examinerDescription.text = _examinerBaseDescription; return; }
            _examinerDescription.text = hotspot.IsRevealed(_examinedData.ItemId)
                ? hotspot.RevealedDescription
                : hotspot.UnrevealedPrompt;
        }

        private void HandleExaminerClick(Vector2 pointerPosition)
        {
            if (!TryRaycastHotspot(pointerPosition, out ExamineHotspot hotspot) || _examinedData == null) return;
            hotspot.Reveal(_examinedData.ItemId);
            _examinerDescription.text = hotspot.RevealedDescription;
        }

        private bool TryRaycastHotspot(Vector2 pointerPosition, out ExamineHotspot hotspot)
        {
            hotspot = null;
            if (_examineCamera == null || !TryGetExaminerViewportPoint(pointerPosition, out Vector2 viewportPoint)) return false;

            Ray ray = _examineCamera.ViewportPointToRay(new Vector3(viewportPoint.x, viewportPoint.y, 0f));
            int examineLayer = LayerMask.NameToLayer("Examine");
            if (examineLayer < 0) examineLayer = 31;
            if (Physics.Raycast(ray, out RaycastHit hit, 50f, 1 << examineLayer, QueryTriggerInteraction.Collide))
                hotspot = hit.collider.GetComponentInParent<ExamineHotspot>();
            return hotspot != null;
        }

        /// <summary>Converts a pointer position (panel space) into a 0-1 viewport point on the examine camera, accounting for the image's scale-to-fit letterboxing. Returns false for clicks landing in the letterbox margin.</summary>
        private bool TryGetExaminerViewportPoint(Vector2 pointerPosition, out Vector2 viewportPoint)
        {
            viewportPoint = Vector2.zero;
            Rect rect = _examinerImage.contentRect;
            if (_examineTexture == null || rect.width <= 0f || rect.height <= 0f) return false;

            Vector2 local = _examinerImage.WorldToLocal(pointerPosition);
            float textureAspect = (float)_examineTexture.width / _examineTexture.height;
            float rectAspect = rect.width / rect.height;

            float displayWidth = rect.width;
            float displayHeight = rect.height;
            float offsetX = 0f;
            float offsetY = 0f;
            if (rectAspect > textureAspect)
            {
                displayWidth = rect.height * textureAspect;
                offsetX = (rect.width - displayWidth) * .5f;
            }
            else
            {
                displayHeight = rect.width / textureAspect;
                offsetY = (rect.height - displayHeight) * .5f;
            }

            float localX = local.x - offsetX;
            float localY = local.y - offsetY;
            if (localX < 0f || localY < 0f || localX > displayWidth || localY > displayHeight) return false;

            viewportPoint = new Vector2(localX / displayWidth, 1f - localY / displayHeight);
            return true;
        }

        private void OnExaminerWheel(WheelEvent evt)
        {
            if (_examinedModel == null) return;
            _examinedZoom = Mathf.Clamp(_examinedZoom - evt.delta.y * _examineZoomSpeed, .5f, 2.5f);
            _examinedModel.transform.localScale = _examinedBaseScale * _examinedZoom;
            evt.StopPropagation();
        }

        private void CombineExaminedItem()
        {
            if (_examinedData == null || _inventory == null) return;
            if (_inventory.TryCombineWithActive(_examinedData.ItemId)) CloseTopPanel();
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            if (root == null || layer < 0) return;
            root.layer = layer;
            foreach (Transform child in root.transform) SetLayerRecursively(child.gameObject, layer);
        }

        public void ShowSubtitle(string text)
        {
            if (GameSettingsService.Instance != null && !GameSettingsService.Instance.Data.subtitles) return;
            if (_subtitleRoutine != null) StopCoroutine(_subtitleRoutine);
            _subtitleRoutine = StartCoroutine(TypeSubtitle(text ?? string.Empty));
        }

        public void HideSubtitle()
        {
            if (_subtitleRoutine != null) StopCoroutine(_subtitleRoutine);
            _subtitleRoutine = null;
            SetVisible(_subtitle, false);
        }

        private IEnumerator TypeSubtitle(string text)
        {
            _subtitle.text = string.Empty;
            SetVisible(_subtitle, true);
            foreach (char character in text)
            {
                _subtitle.text += character;
                yield return new WaitForSecondsRealtime(.025f);
            }
        }

        private IEnumerator CloseAfterDelay(float seconds) { yield return new WaitForSecondsRealtime(seconds); CloseTopPanel(); }

        private void OpenModal(GameplayModal modal, VisualElement panel)
        {
            if (_modal == GameplayModal.Examiner && modal != GameplayModal.Examiner)
                DestroyExaminedModel();
            HideModalPanels();
            _modal = modal;
            SetVisible(_modalLayer, true);
            SetVisible(panel, true);
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }

        public void CloseTopPanel()
        {
            if (_modal == GameplayModal.None) return;
            if (_modal == GameplayModal.Examiner) DestroyExaminedModel();
            _modal = GameplayModal.None;
            _combineSourceIndex = -1;
            _itemUseRequest = null;
            _currentPuzzle = null;
            _examinedData = null;
            HideModalPanels();
            SetVisible(_modalLayer, false);
            if (UIToolkitMenuController.Instance == null || !UIToolkitMenuController.Instance.IsBlockingGameplay)
            {
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                UnityEngine.Cursor.visible = false;
            }
        }

        private void HideAll()
        {
            _modal = GameplayModal.None;
            HideModalPanels();
            SetVisible(_modalLayer, false);
            SetVisible(_interactionPrompt, false);
            SetVisible(_flashlightHud, false);
            SetVisible(_sanityHud, false);
            SetVisible(_subtitle, false);
        }

        private void HideModalPanels()
        {
            SetVisible(_inventoryPanel, false);
            SetVisible(_notePanel, false);
            SetVisible(_keypadPanel, false);
            SetVisible(_examinerPanel, false);
        }

        private static void SetVisible(VisualElement element, bool visible)
        {
            if (element != null) element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
