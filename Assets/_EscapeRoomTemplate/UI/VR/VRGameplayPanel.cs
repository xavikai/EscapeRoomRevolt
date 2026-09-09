using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Player;
using EscapeRoomRevolt.Systems.Interaction;
using EscapeRoomRevolt.Systems.Inventory;
using EscapeRoomRevolt.Systems.Puzzle;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

namespace EscapeRoomRevolt.UI.VR
{
    /// <summary>
    /// Native 3D Quest UI. It deliberately avoids screen-space UI Toolkit: every control is a
    /// collider-backed IInteractable, so the same hardware rays used for world mechanics can
    /// operate inventory, readable notes and numeric keypads.
    /// </summary>
    public sealed class VRGameplayPanel : MonoBehaviour
    {
        private enum PanelMode { None, Help, Inventory, Note, Keypad, NumberWheels }

        private const int InventoryPageSize = 5;
        private const int NoteLinesPerPage = 12;

        private Transform _head;
        private GameObject _panel;
        private Transform _content;
        private Material _surfaceMaterial;
        private TMP_FontAsset _font;
        private GameObject _subtitlePanel;
        private TextMeshPro _subtitleText;
        private Coroutine _subtitleHideRoutine;
        private PanelMode _mode;
        private InventoryManager _inventory;
        private InventoryItemUseRequest _itemUseRequest;
        private CodePanelPuzzle _keypad;
        private PuzzleController _numberWheelsPuzzle;
        private readonly List<NumberWheelInteractable> _numberWheels = new List<NumberWheelInteractable>();
        private int _inventoryPage;
        private int _selectedStorageIndex = -1;
        private int _combineSourceIndex = -1;
        private readonly List<string> _notePages = new List<string>();
        private int _notePage;
        private bool _wasInventoryPressed;
        private bool _wasBackPressed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (FindAnyObjectByType<VRGameplayPanel>() != null) return;
            GameObject host = new GameObject("VR Gameplay Panel");
            DontDestroyOnLoad(host);
            host.AddComponent<VRGameplayPanel>();
#endif
        }

        private void Awake()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            if (shader != null) _surfaceMaterial = new Material(shader) { name = "VR Panel Runtime Material" };
            _font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            CreatePanelRoot();
            CreateSubtitleRoot();
        }

        private IEnumerator Start()
        {
            // GameContext clears EventBus in Awake. Subscribing from Start ensures the VR listener
            // survives that reset regardless of script execution order.
            EventBus.Subscribe<RequestToggleInventory>(HandleToggleInventory);
            EventBus.Subscribe<RequestCloseTopPanel>(HandleClosePanel);
            EventBus.Subscribe<RequestShowNoteReader>(HandleShowNote);
            EventBus.Subscribe<RequestShowKeypad>(HandleShowKeypad);
            EventBus.Subscribe<RequestShowNumberWheels>(HandleShowNumberWheels);
            EventBus.Subscribe<RequestShowSubtitle>(HandleShowSubtitle);
            EventBus.Subscribe<RequestHideSubtitle>(HandleHideSubtitle);

            BindInventory();
            yield return null;
            ResolveHead();
            if (_head != null) OpenHelp();
            Debug.Log("[VR Panel] Ready. Y=inventory, B=close, trigger=select.");
        }

        private void Update()
        {
            if (_head == null) ResolveHead();
            if (_inventory == null || InventoryManager.Instance != _inventory) BindInventory();

            InputDevice left = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            InputDevice right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            bool inventoryPressed = ReadButton(left, CommonUsages.secondaryButton); // Y
            bool backPressed = ReadButton(right, CommonUsages.secondaryButton); // B

            if (inventoryPressed && !_wasInventoryPressed)
            {
                if (_mode == PanelMode.Inventory) ClosePanel();
                else OpenInventory();
            }
            if (backPressed && !_wasBackPressed && _mode != PanelMode.None) ClosePanel();

            _wasInventoryPressed = inventoryPressed;
            _wasBackPressed = backPressed;
        }

        private static bool ReadButton(InputDevice device, InputFeatureUsage<bool> usage) =>
            device.isValid && device.TryGetFeatureValue(usage, out bool value) && value;

        private void OnDestroy()
        {
            EventBus.Unsubscribe<RequestToggleInventory>(HandleToggleInventory);
            EventBus.Unsubscribe<RequestCloseTopPanel>(HandleClosePanel);
            EventBus.Unsubscribe<RequestShowNoteReader>(HandleShowNote);
            EventBus.Unsubscribe<RequestShowKeypad>(HandleShowKeypad);
            EventBus.Unsubscribe<RequestShowNumberWheels>(HandleShowNumberWheels);
            EventBus.Unsubscribe<RequestShowSubtitle>(HandleShowSubtitle);
            EventBus.Unsubscribe<RequestHideSubtitle>(HandleHideSubtitle);
            UnbindInventory();
            if (_surfaceMaterial != null) Destroy(_surfaceMaterial);
        }

        private void HandleToggleInventory(RequestToggleInventory evt)
        {
            if (_mode == PanelMode.Inventory) ClosePanel(); else OpenInventory();
        }

        private void HandleClosePanel(RequestCloseTopPanel evt) => ClosePanel();
        private void HandleShowNote(RequestShowNoteReader evt) => OpenNote(evt.content);
        private void HandleShowKeypad(RequestShowKeypad evt)
        {
            if (evt.puzzle is CodePanelPuzzle keypad) OpenKeypad(keypad);
        }

        private void HandleShowNumberWheels(RequestShowNumberWheels evt)
        {
            if (evt.puzzle is PuzzleController puzzle) OpenNumberWheels(puzzle);
        }

        private void HandleShowSubtitle(RequestShowSubtitle evt)
        {
            if (_subtitleHideRoutine != null) StopCoroutine(_subtitleHideRoutine);
            if (_subtitleText == null || _subtitlePanel == null) return;
            ResolveHead();
            PlaceSubtitle();
            _subtitleText.text = evt.text ?? string.Empty;
            _subtitlePanel.SetActive(true);
            if (evt.holdSeconds > 0f) _subtitleHideRoutine = StartCoroutine(HideSubtitleAfter(evt.holdSeconds));
        }

        private void HandleHideSubtitle(RequestHideSubtitle evt) => HideSubtitle();

        private IEnumerator HideSubtitleAfter(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            HideSubtitle();
        }

        private void HideSubtitle()
        {
            if (_subtitleHideRoutine != null)
            {
                StopCoroutine(_subtitleHideRoutine);
                _subtitleHideRoutine = null;
            }
            if (_subtitlePanel != null) _subtitlePanel.SetActive(false);
        }

        private void BindInventory()
        {
            UnbindInventory();
            _inventory = InventoryManager.Instance;
            if (_inventory == null) return;
            _inventory.OnInventoryChanged += HandleInventoryChanged;
            _inventory.ItemUseSelectionRequested += HandleItemUseSelection;
        }

        private void UnbindInventory()
        {
            if (_inventory == null) return;
            _inventory.OnInventoryChanged -= HandleInventoryChanged;
            _inventory.ItemUseSelectionRequested -= HandleItemUseSelection;
            _inventory = null;
        }

        private void HandleInventoryChanged()
        {
            if (_mode == PanelMode.Inventory) BuildInventory();
        }

        private void HandleItemUseSelection(InventoryItemUseRequest request)
        {
            _itemUseRequest = request;
            _selectedStorageIndex = FindFirstInventoryIndex(request);
            _inventoryPage = 0;
            OpenPanel(PanelMode.Inventory);
            BuildInventory();
        }

        private void ResolveHead()
        {
            _head = PlayerPlatformRegistry.Current?.Head;
            if (_head == null && Camera.main != null) _head = Camera.main.transform;
        }

        private void CreatePanelRoot()
        {
            _panel = new GameObject("Quest 3D Gameplay Panel");
            _panel.transform.SetParent(transform, false);
            _content = new GameObject("Content").transform;
            _content.SetParent(_panel.transform, false);

            GameObject background = GameObject.CreatePrimitive(PrimitiveType.Cube);
            background.name = "Panel Background";
            background.transform.SetParent(_panel.transform, false);
            background.transform.localScale = new Vector3(1.35f, .90f, .025f);
            Collider backgroundCollider = background.GetComponent<Collider>();
            if (backgroundCollider != null) Destroy(backgroundCollider);
            SetSurface(background.GetComponent<Renderer>(), new Color(.018f, .025f, .04f, 1f));
            _panel.SetActive(false);
        }

        private void CreateSubtitleRoot()
        {
            _subtitlePanel = new GameObject("Quest 3D Subtitles");
            _subtitlePanel.transform.SetParent(transform, false);

            GameObject background = GameObject.CreatePrimitive(PrimitiveType.Cube);
            background.name = "Subtitle Background";
            background.transform.SetParent(_subtitlePanel.transform, false);
            background.transform.localScale = new Vector3(1.15f, .19f, .018f);
            Collider collider = background.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            SetSurface(background.GetComponent<Renderer>(), new Color(.01f, .015f, .025f, .92f));

            _subtitleText = CreateTMPText(_subtitlePanel.transform, string.Empty,
                new Vector3(0f, 0f, -.018f), .034f, TextAnchor.MiddleCenter, Color.white,
                new Vector2(1.06f, .14f), true);
            _subtitlePanel.SetActive(false);
        }

        private void PlacePanel()
        {
            if (_head == null) ResolveHead();
            if (_head == null) return;
            Vector3 forward = Vector3.ProjectOnPlane(_head.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < .01f) forward = Vector3.forward;
            _panel.transform.SetPositionAndRotation(_head.position + forward * 1.05f - Vector3.up * .03f,
                Quaternion.LookRotation(forward, Vector3.up));
        }

        private void PlaceSubtitle()
        {
            if (_head == null || _subtitlePanel == null) return;
            Vector3 forward = Vector3.ProjectOnPlane(_head.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < .01f) forward = Vector3.forward;
            _subtitlePanel.transform.SetPositionAndRotation(
                _head.position + forward * 1.15f - Vector3.up * .34f,
                Quaternion.LookRotation(forward, Vector3.up));
        }

        private void OpenPanel(PanelMode mode)
        {
            _mode = mode;
            PlacePanel();
            _panel.SetActive(true);
            EventBus.Publish(new OnGameplayUIBlockingChanged { isBlocking = true });
        }

        private void ClosePanel()
        {
            _mode = PanelMode.None;
            _keypad = null;
            _numberWheelsPuzzle = null;
            _numberWheels.Clear();
            _itemUseRequest = null;
            _combineSourceIndex = -1;
            if (_panel != null) _panel.SetActive(false);
            EventBus.Publish(new OnGameplayUIBlockingChanged { isBlocking = false });
        }

        private void OpenHelp()
        {
            OpenPanel(PanelMode.Help);
            ClearContent();
            CreateLabel("PROVA VR · SHOWCASE MUSEUM", new Vector2(0f, .34f), .055f, TextAnchor.MiddleCenter, Color.white);
            CreateLabel("Y  ·  OBRIR INVENTARI\nB  ·  TANCAR / TORNAR\nGATELL  ·  INTERACTUAR\nJOYSTICK DRET ENDAVANT  ·  APUNTAR TELEPORT\nDEIXAR-LO ANAR  ·  TELEPORTAR\n\nLes notes i els keypads s'obren en aquest panell 3D.",
                new Vector2(-.48f, .19f), .036f, TextAnchor.UpperLeft, new Color(.82f, .9f, 1f));
            CreateButton("COMENÇAR", new Vector2(0f, -.31f), new Vector2(.48f, .11f), ClosePanel);
        }

        private void OpenInventory()
        {
            _itemUseRequest = null;
            _combineSourceIndex = -1;
            _selectedStorageIndex = FindFirstInventoryIndex(null);
            _inventoryPage = 0;
            OpenPanel(PanelMode.Inventory);
            BuildInventory();
        }

        private void BuildInventory()
        {
            if (_mode != PanelMode.Inventory) return;
            ClearContent();
            string title = _itemUseRequest == null ? "INVENTARI" : "TRIA UN OBJECTE PER UTILITZAR AQUÍ";
            CreateLabel(title, new Vector2(0f, .37f), .045f, TextAnchor.MiddleCenter, Color.white);
            CreateLabel("Y / B: tancar", new Vector2(.54f, .37f), .022f, TextAnchor.MiddleRight, new Color(.55f, .7f, .82f));

            if (_inventory?.Slots == null)
            {
                CreateLabel("L'inventari encara no està disponible.", Vector2.zero, .04f, TextAnchor.MiddleCenter, Color.white);
                return;
            }

            List<int> occupied = GetVisibleInventoryIndices();
            int pageCount = Mathf.Max(1, Mathf.CeilToInt(occupied.Count / (float)InventoryPageSize));
            _inventoryPage = Mathf.Clamp(_inventoryPage, 0, pageCount - 1);
            int start = _inventoryPage * InventoryPageSize;

            for (int row = 0; row < InventoryPageSize; row++)
            {
                int listIndex = start + row;
                if (listIndex >= occupied.Count) break;
                int storageIndex = occupied[listIndex];
                InventorySlot slot = _inventory.Slots[storageIndex];
                string marker = storageIndex == _selectedStorageIndex ? "▶ " : string.Empty;
                string quantity = slot.Quantity > 1 ? $"  ×{slot.Quantity}" : string.Empty;
                int captured = storageIndex;
                CreateButton(Shorten(marker + slot.Data.DisplayName + quantity, 25),
                    new Vector2(-.36f, .24f - row * .115f), new Vector2(.55f, .09f), () => SelectInventory(captured),
                    storageIndex == _selectedStorageIndex ? new Color(.12f, .42f, .56f) : new Color(.08f, .16f, .23f));
            }

            if (occupied.Count == 0)
                CreateLabel(_itemUseRequest == null ? "L'inventari és buit." : "No portes cap objecte compatible.",
                    new Vector2(-.36f, .08f), .035f, TextAnchor.MiddleCenter, Color.white);

            InventoryItemData selected = GetSelectedData();
            string selectedName = selected != null ? selected.DisplayName.ToUpperInvariant() : "CAP OBJECTE SELECCIONAT";
            string selectedDescription = selected != null ? selected.Description : "Recull un objecte i apareixerà aquí.";
            if (_combineSourceIndex >= 0) selectedDescription = "Selecciona un segon objecte i prem COMBINAR.";
            CreateLabel(Shorten(selectedName, 30), new Vector2(.29f, .25f), .035f, TextAnchor.MiddleCenter, new Color(1f, .78f, .28f));
            CreateLabel(Wrap(selectedDescription, 34), new Vector2(.04f, .18f), .024f, TextAnchor.UpperLeft, new Color(.86f, .9f, .94f));

            CreateButton(_itemUseRequest == null ? PrimaryActionLabel(selected) : "UTILITZAR AQUÍ", new Vector2(.17f, -.12f), new Vector2(.30f, .09f), UseSelected);
            CreateButton("ACTIVAR", new Vector2(.49f, -.12f), new Vector2(.27f, .09f), ActivateSelected);
            CreateButton(_combineSourceIndex < 0 ? "COMBINAR" : "CONFIRMAR", new Vector2(.17f, -.24f), new Vector2(.30f, .09f), CombineSelected);
            CreateButton($"RÀPID {_inventory.ActiveQuickIndex + 1}", new Vector2(.49f, -.24f), new Vector2(.27f, .09f), CycleQuickSlot);

            CreateButton("◀", new Vector2(-.53f, -.35f), new Vector2(.13f, .075f), () => ChangeInventoryPage(-1));
            CreateLabel($"{_inventoryPage + 1} / {pageCount}", new Vector2(-.36f, -.35f), .025f, TextAnchor.MiddleCenter, Color.white);
            CreateButton("▶", new Vector2(-.19f, -.35f), new Vector2(.13f, .075f), () => ChangeInventoryPage(1));
            CreateButton("TANCAR", new Vector2(.42f, -.35f), new Vector2(.38f, .075f), ClosePanel, new Color(.32f, .10f, .12f));
        }

        private List<int> GetVisibleInventoryIndices()
        {
            var indices = new List<int>();
            if (_inventory?.Slots == null) return indices;
            for (int index = 0; index < _inventory.Slots.Length; index++)
            {
                InventorySlot slot = _inventory.Slots[index];
                if (slot.IsEmpty) continue;
                if (_itemUseRequest != null && !_itemUseRequest.IsCompatible(slot.ItemId)) continue;
                indices.Add(index);
            }
            return indices;
        }

        private int FindFirstInventoryIndex(InventoryItemUseRequest request)
        {
            if (_inventory?.Slots == null) return -1;
            for (int index = 0; index < _inventory.Slots.Length; index++)
            {
                InventorySlot slot = _inventory.Slots[index];
                if (!slot.IsEmpty && (request == null || request.IsCompatible(slot.ItemId))) return index;
            }
            return -1;
        }

        private void SelectInventory(int storageIndex)
        {
            _selectedStorageIndex = storageIndex;
            BuildInventory();
        }

        private void ChangeInventoryPage(int direction)
        {
            int count = GetVisibleInventoryIndices().Count;
            int pages = Mathf.Max(1, Mathf.CeilToInt(count / (float)InventoryPageSize));
            _inventoryPage = (_inventoryPage + direction + pages) % pages;
            BuildInventory();
        }

        private InventoryItemData GetSelectedData()
        {
            if (_inventory?.Slots == null || _selectedStorageIndex < 0 || _selectedStorageIndex >= _inventory.Slots.Length) return null;
            InventorySlot slot = _inventory.Slots[_selectedStorageIndex];
            return slot.IsEmpty ? null : slot.Data;
        }

        private void UseSelected()
        {
            InventoryItemData data = GetSelectedData();
            if (data == null || _inventory == null) return;
            if (_itemUseRequest != null)
            {
                if (_itemUseRequest.TryUse(data.ItemId)) ClosePanel();
                return;
            }
            _inventory.PerformPrimaryActionAt(_selectedStorageIndex);
            BuildInventory();
        }

        private void ActivateSelected()
        {
            if (_inventory == null || GetSelectedData() == null) return;
            if (_inventory.AssignQuickSlot(_inventory.ActiveQuickIndex, _selectedStorageIndex)) ClosePanel();
        }

        private void CycleQuickSlot()
        {
            if (_inventory == null) return;
            _inventory.SetActiveQuickSlot(_inventory.ActiveQuickIndex + 1);
            BuildInventory();
        }

        private void CombineSelected()
        {
            if (_inventory == null || GetSelectedData() == null) return;
            if (_combineSourceIndex < 0)
            {
                _combineSourceIndex = _selectedStorageIndex;
                BuildInventory();
                return;
            }

            if (_combineSourceIndex == _selectedStorageIndex)
            {
                _combineSourceIndex = -1;
                BuildInventory();
                return;
            }

            int source = _combineSourceIndex;
            _combineSourceIndex = -1;
            bool success = _inventory.TryCombine(source, _selectedStorageIndex);
            Debug.Log($"[VR Panel] Inventory combination result: {success}.");
            _selectedStorageIndex = FindFirstInventoryIndex(null);
            BuildInventory();
        }

        private static string PrimaryActionLabel(InventoryItemData data)
        {
            if (data == null) return "USAR";
            if (data.IsReadable || data.PrimaryAction == InventoryPrimaryAction.Read) return "LLEGIR";
            if (data.PrimaryAction == InventoryPrimaryAction.Consume) return "CONSUMIR";
            if (data.WorldPrefab != null) return "SOSTENIR";
            return "USAR";
        }

        private void OpenNote(string content)
        {
            _notePages.Clear();
            string wrapped = Wrap(string.IsNullOrWhiteSpace(content) ? "(Nota buida)" : content, 58);
            string[] lines = wrapped.Split('\n');
            for (int start = 0; start < lines.Length; start += NoteLinesPerPage)
                _notePages.Add(string.Join("\n", lines.Skip(start).Take(NoteLinesPerPage)));
            if (_notePages.Count == 0) _notePages.Add("(Nota buida)");
            _notePage = 0;
            OpenPanel(PanelMode.Note);
            BuildNote();
        }

        private void BuildNote()
        {
            ClearContent();
            CreateLabel("NOTA", new Vector2(0f, .36f), .05f, TextAnchor.MiddleCenter, new Color(1f, .8f, .35f));
            CreateLabel(_notePages[_notePage], new Vector2(-.56f, .27f), .027f, TextAnchor.UpperLeft, Color.white);
            CreateButton("◀", new Vector2(-.42f, -.35f), new Vector2(.16f, .08f), () => ChangeNotePage(-1));
            CreateLabel($"{_notePage + 1} / {_notePages.Count}", new Vector2(-.20f, -.35f), .025f, TextAnchor.MiddleCenter, Color.white);
            CreateButton("▶", new Vector2(.02f, -.35f), new Vector2(.16f, .08f), () => ChangeNotePage(1));
            CreateButton("TANCAR", new Vector2(.39f, -.35f), new Vector2(.38f, .08f), ClosePanel, new Color(.32f, .10f, .12f));
        }

        private void ChangeNotePage(int direction)
        {
            _notePage = (_notePage + direction + _notePages.Count) % _notePages.Count;
            BuildNote();
        }

        private void OpenKeypad(CodePanelPuzzle keypad)
        {
            _keypad = keypad;
            OpenPanel(PanelMode.Keypad);
            BuildKeypad();
        }

        private void BuildKeypad()
        {
            ClearContent();
            string display = _keypad == null ? "----" : (_keypad.IsSolved ? "OK" : _keypad.CurrentInput.PadRight(4, '-'));
            CreateLabel("TECLAT NUMÈRIC", new Vector2(0f, .37f), .045f, TextAnchor.MiddleCenter, Color.white);
            CreateLabel(display, new Vector2(0f, .27f), .065f, TextAnchor.MiddleCenter, new Color(.35f, 1f, .55f));
            string[] keys = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "C", "0", "OK" };
            for (int index = 0; index < keys.Length; index++)
            {
                string key = keys[index];
                int row = index / 3;
                int column = index % 3;
                float x = (column - 1) * .23f;
                float y = .13f - row * .14f;
                CreateButton(key, new Vector2(x, y), new Vector2(.18f, .10f), () => KeypadPress(key),
                    key == "OK" ? new Color(.08f, .34f, .17f) : key == "C" ? new Color(.36f, .11f, .10f) : new Color(.08f, .16f, .23f));
            }
            CreateButton("TANCAR", new Vector2(.45f, -.35f), new Vector2(.30f, .08f), ClosePanel, new Color(.32f, .10f, .12f));
        }

        private void KeypadPress(string key)
        {
            if (_keypad == null || _keypad.IsSolved) return;
            if (key == "C") _keypad.ClearInput();
            else if (key == "OK") _keypad.SubmitCode();
            else _keypad.InputDigit(key);
            BuildKeypad();
            if (_keypad.IsSolved) StartCoroutine(CloseSolvedKeypad());
        }

        private IEnumerator CloseSolvedKeypad()
        {
            yield return new WaitForSecondsRealtime(.8f);
            ClosePanel();
        }

        private void OpenNumberWheels(PuzzleController puzzle)
        {
            _numberWheelsPuzzle = puzzle;
            _numberWheels.Clear();
            _numberWheels.AddRange(puzzle.GetComponentsInChildren<NumberWheelInteractable>(true)
                .OrderBy(wheel => wheel.name));
            if (_numberWheels.Count == 0)
            {
                Debug.LogWarning($"[VR Panel] '{puzzle.name}' has no number wheels to display.", puzzle);
                return;
            }

            OpenPanel(PanelMode.NumberWheels);
            BuildNumberWheels();
        }

        private void BuildNumberWheels()
        {
            if (_mode != PanelMode.NumberWheels) return;
            ClearContent();
            CreateLabel("COMBINACIÓ DE RODETS", new Vector2(0f, .35f), .043f,
                TextAnchor.MiddleCenter, Color.white);
            CreateLabel("Selecciona ▲ o ▼ per girar cada rodet", new Vector2(0f, .27f), .024f,
                TextAnchor.MiddleCenter, new Color(.7f, .82f, .94f));

            float spacing = Mathf.Min(.25f, 1.02f / Mathf.Max(1, _numberWheels.Count));
            float buttonWidth = Mathf.Min(.18f, spacing * .72f);
            float firstX = -spacing * (_numberWheels.Count - 1) * .5f;
            for (int index = 0; index < _numberWheels.Count; index++)
            {
                NumberWheelInteractable wheel = _numberWheels[index];
                int captured = index;
                float x = firstX + index * spacing;
                CreateButton("▲", new Vector2(x, .13f), new Vector2(buttonWidth, .09f),
                    () => StepNumberWheel(captured, 1));
                CreateLabel(wheel.CurrentDigit.ToString(), new Vector2(x, 0f), .07f,
                    TextAnchor.MiddleCenter, new Color(.35f, 1f, .55f));
                CreateButton("▼", new Vector2(x, -.13f), new Vector2(buttonWidth, .09f),
                    () => StepNumberWheel(captured, -1));
            }

            CreateButton("TANCAR", new Vector2(0f, -.35f), new Vector2(.38f, .08f), ClosePanel,
                new Color(.32f, .10f, .12f));
        }

        private void StepNumberWheel(int index, int direction)
        {
            if (index < 0 || index >= _numberWheels.Count) return;
            if (!_numberWheels[index].TryStep(direction)) return;
            BuildNumberWheels();
            if (_numberWheelsPuzzle != null && _numberWheelsPuzzle.IsSolved)
                StartCoroutine(CloseSolvedNumberWheels());
        }

        private IEnumerator CloseSolvedNumberWheels()
        {
            yield return new WaitForSecondsRealtime(.8f);
            if (_mode == PanelMode.NumberWheels) ClosePanel();
        }

        private void ClearContent()
        {
            for (int index = _content.childCount - 1; index >= 0; index--)
            {
                GameObject child = _content.GetChild(index).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
        }

        private void CreateLabel(string text, Vector2 position, float characterSize, TextAnchor anchor, Color color)
        {
            bool leftAligned = anchor == TextAnchor.UpperLeft || anchor == TextAnchor.MiddleLeft;
            int lineCount = 1;
            if (!string.IsNullOrEmpty(text))
                foreach (char value in text) if (value == '\n') lineCount++;
            float height = Mathf.Clamp(lineCount * characterSize * 1.45f, .10f, .56f);
            CreateTMPText(_content, text, new Vector3(position.x, position.y, -.052f), characterSize,
                anchor, color, new Vector2(leftAligned ? .62f : 1.15f, height), false);
        }

        private TextMeshPro CreateTMPText(Transform parent, string text, Vector3 localPosition,
            float characterSize, TextAnchor anchor, Color color, Vector2 size, bool richText)
        {
            GameObject label = new GameObject("Text", typeof(RectTransform));
            RectTransform rect = label.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.localPosition = localPosition;
            rect.localRotation = Quaternion.identity;
            rect.sizeDelta = size;

            TextMeshPro mesh = label.AddComponent<TextMeshPro>();
            if (_font != null) mesh.font = _font;
            mesh.text = text ?? string.Empty;
            mesh.enableAutoSizing = true;
            mesh.fontSizeMin = Mathf.Max(.035f, characterSize * 1.5f);
            mesh.fontSizeMax = Mathf.Max(mesh.fontSizeMin, characterSize * 4.5f);
            mesh.fontSize = mesh.fontSizeMax;
            mesh.color = color;
            mesh.richText = richText;
            mesh.textWrappingMode = TextWrappingModes.Normal;
            mesh.overflowMode = TextOverflowModes.Truncate;
            mesh.raycastTarget = false;
            mesh.alignment = anchor switch
            {
                TextAnchor.UpperLeft => TextAlignmentOptions.TopLeft,
                TextAnchor.MiddleLeft => TextAlignmentOptions.Left,
                TextAnchor.UpperCenter => TextAlignmentOptions.Top,
                TextAnchor.LowerCenter => TextAlignmentOptions.Bottom,
                _ => TextAlignmentOptions.Center
            };
            return mesh;
        }

        private void CreateButton(string label, Vector2 position, Vector2 size, Action action, Color? color = null)
        {
            GameObject button = GameObject.CreatePrimitive(PrimitiveType.Cube);
            button.name = "VR Button " + label;
            button.transform.SetParent(_content, false);
            button.transform.localPosition = new Vector3(position.x, position.y, -.035f);
            button.transform.localScale = new Vector3(size.x, size.y, .028f);
            Renderer renderer = button.GetComponent<Renderer>();
            SetSurface(renderer, color ?? new Color(.08f, .16f, .23f));
            button.AddComponent<VRPanelButton>().Configure(action, renderer, color ?? new Color(.08f, .16f, .23f));
            CreateLabel(label, position, Mathf.Min(.032f, size.y * .32f), TextAnchor.MiddleCenter, Color.white);
        }

        private void SetSurface(Renderer renderer, Color color)
        {
            if (renderer == null) return;
            if (_surfaceMaterial != null) renderer.sharedMaterial = _surfaceMaterial;
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", color);
            block.SetColor("_Color", color);
            renderer.SetPropertyBlock(block);
        }

        private static string Shorten(string value, int maximum)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maximum) return value ?? string.Empty;
            return value.Substring(0, Mathf.Max(1, maximum - 1)) + "…";
        }

        private static string Wrap(string value, int width)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            var output = new System.Text.StringBuilder();
            foreach (string paragraph in value.Replace("\r", string.Empty).Split('\n'))
            {
                int column = 0;
                foreach (string word in paragraph.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (column > 0 && column + word.Length + 1 > width)
                    {
                        output.Append('\n');
                        column = 0;
                    }
                    else if (column > 0)
                    {
                        output.Append(' ');
                        column++;
                    }
                    output.Append(word);
                    column += word.Length;
                }
                output.Append('\n');
            }
            return output.ToString().TrimEnd();
        }
    }

    public sealed class VRPanelButton : MonoBehaviour, IInteractable
    {
        private Action _action;
        private Renderer _renderer;
        private Color _baseColor;

        public string InteractionPrompt => name;
        public CursorType InteractionCursor => CursorType.Hand;
        public bool CanInteract => isActiveAndEnabled;

        public void Configure(Action action, Renderer renderer, Color baseColor)
        {
            _action = action;
            _renderer = renderer;
            _baseColor = baseColor;
        }

        public void Interact() => _action?.Invoke();
        public void OnFocusEnter() => SetColor(Color.Lerp(_baseColor, new Color(1f, .68f, .12f), .72f));
        public void OnFocusExit() => SetColor(_baseColor);

        private void SetColor(Color color)
        {
            if (_renderer == null) return;
            var block = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", color);
            block.SetColor("_Color", color);
            _renderer.SetPropertyBlock(block);
        }
    }
}
