using System;
using EscapeRoomRevolt.Core.Settings;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EscapeRoomRevolt.Core.Input
{
    /// <summary>Single source of player controls. Rebinding can be applied to this asset at runtime.</summary>
    [DefaultExecutionOrder(-125)]
    public sealed class InputRouter : MonoBehaviour
    {
        public static InputRouter Instance { get; private set; }
        public InputActionAsset Actions { get; private set; }
        private InputActionMap _gameplay;
        private InputAction _move, _look, _interact, _sprint, _crouch, _jump, _pause, _inventory;
        private InputAction _toggleFlashlight, _reload, _dropEquipped;
        private InputAction _toggleCamcorder, _toggleNightVision, _reloadCamcorder;
        private InputAction _recordEvidence, _camcorderZoom;
        private InputAction _primaryAction, _secondaryAction, _dropHeld;
        private InputAction _hint;
        private InputAction _quickSave, _quickLoad;
        private InputAction _quickNavigate;
        private InputAction _leanModifier, _lookBack, _slide, _carefulInteractModifier;
        private readonly InputAction[] _quickSlots = new InputAction[8];
        private InputActionRebindingExtensions.RebindingOperation _rebindOperation;

        public Vector2 Move => _move?.ReadValue<Vector2>() ?? Vector2.zero;
        public Vector2 Look => _look?.ReadValue<Vector2>() ?? Vector2.zero;
        public bool InteractPressed => _interact?.WasPressedThisFrame() ?? false;
        public bool SprintHeld => _sprint?.IsPressed() ?? false;
        public bool CrouchHeld => _crouch?.IsPressed() ?? false;
        public bool JumpPressed => _jump?.WasPressedThisFrame() ?? false;
        public bool PausePressed => _pause?.WasPressedThisFrame() ?? false;
        public bool InventoryPressed => _inventory?.WasPressedThisFrame() ?? false;
        [System.Obsolete("Use InventoryPressed. This alias will be removed in the next major version.")]
        public bool UseItemPressed => InventoryPressed;
        public bool ToggleFlashlightPressed => _toggleFlashlight?.WasPressedThisFrame() ?? false;
        public bool ReloadPressed => _reload?.WasPressedThisFrame() ?? false;
        public bool DropEquippedPressed => _dropEquipped?.WasPressedThisFrame() ?? false;
        public bool ToggleCamcorderPressed => _toggleCamcorder?.WasPressedThisFrame() ?? false;
        public bool ToggleNightVisionPressed => _toggleNightVision?.WasPressedThisFrame() ?? false;
        public bool ReloadCamcorderPressed => _reloadCamcorder?.WasPressedThisFrame() ?? false;
        public bool RecordEvidenceHeld => _recordEvidence?.IsPressed() ?? false;
        public bool CamcorderZoomHeld => _camcorderZoom?.IsPressed() ?? false;
        public bool PrimaryActionPressed => _primaryAction?.WasPressedThisFrame() ?? false;
        public bool SecondaryActionHeld => _secondaryAction?.IsPressed() ?? false;
        public bool DropHeldPressed => _dropHeld?.WasPressedThisFrame() ?? false;
        public bool HintPressed => _hint?.WasPressedThisFrame() ?? false;
        public bool QuickSavePressed => _quickSave?.WasPressedThisFrame() ?? false;
        public bool QuickLoadPressed => _quickLoad?.WasPressedThisFrame() ?? false;
        public bool QuickNavigatePerformed => _quickNavigate?.WasPerformedThisFrame() ?? false;
        public float QuickNavigate => _quickNavigate?.ReadValue<float>() ?? 0f;
        public bool LeanModifierHeld => _leanModifier?.IsPressed() ?? false;
        public bool LookBackHeld => _lookBack?.IsPressed() ?? false;
        public bool SlidePressed => _slide?.WasPressedThisFrame() ?? false;
        public bool CarefulInteractModifierHeld => _carefulInteractModifier?.IsPressed() ?? false;

        public bool TryGetQuickSlotPressed(out int index)
        {
            for (index = 0; index < _quickSlots.Length; index++)
                if (_quickSlots[index]?.WasPressedThisFrame() ?? false) return true;
            index = -1;
            return false;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InputActionAsset template = Resources.Load<InputActionAsset>("Input/EscapeRoomInputActions");
            if (template == null) { Debug.LogError("[InputRouter] Input Actions asset not found in Resources/Input."); return; }
            Actions = Instantiate(template);
            Actions.name = template.name + " (Runtime)";
            _gameplay = Actions.FindActionMap("Gameplay", true);
            _move = _gameplay.FindAction("Move", true); _look = _gameplay.FindAction("Look", true);
            _interact = _gameplay.FindAction("Interact", true); _sprint = _gameplay.FindAction("Sprint", true);
            _crouch = _gameplay.FindAction("Crouch", true); _jump = _gameplay.FindAction("Jump", true);
            _pause = _gameplay.FindAction("Pause", true); _inventory = _gameplay.FindAction("Inventory", true);
            _toggleFlashlight = _gameplay.FindAction("ToggleFlashlight", true);
            _reload = _gameplay.FindAction("Reload", true);
            _dropEquipped = _gameplay.FindAction("DropEquipped", true);
            _toggleCamcorder = _gameplay.FindAction("ToggleCamcorder", false);
            _toggleNightVision = _gameplay.FindAction("ToggleNightVision", false);
            _reloadCamcorder = _gameplay.FindAction("ReloadCamcorder", false);
            _recordEvidence = _gameplay.FindAction("RecordEvidence", false);
            _camcorderZoom = _gameplay.FindAction("CamcorderZoom", false);
            _primaryAction = _gameplay.FindAction("PrimaryAction", true);
            _secondaryAction = _gameplay.FindAction("SecondaryAction", true);
            _dropHeld = _gameplay.FindAction("DropHeld", true);
            _hint = _gameplay.FindAction("Hint", true);
            _quickSave = _gameplay.FindAction("QuickSave", true);
            _quickLoad = _gameplay.FindAction("QuickLoad", true);
            _quickNavigate = _gameplay.FindAction("QuickNavigate", false);
            _leanModifier = _gameplay.FindAction("LeanModifier", false);
            _lookBack = _gameplay.FindAction("LookBack", false);
            _slide = _gameplay.FindAction("Slide", false);
            _carefulInteractModifier = _gameplay.FindAction("CarefulInteractModifier", false);
            for (int index = 0; index < _quickSlots.Length; index++)
                _quickSlots[index] = _gameplay.FindAction($"QuickSlot{index + 1}", false);
            _gameplay.Enable();
        }

        private void Start()
        {
            ApplyBindingOverrides(GameSettingsService.Instance?.Data?.bindingOverridesJson);
        }

        public InputAction FindAction(string actionName) => _gameplay?.FindAction(actionName, false);

        public int FindKeyboardBinding(string actionName, string compositePart = null)
        {
            InputAction action = FindAction(actionName);
            if (action == null) return -1;
            for (int index = 0; index < action.bindings.Count; index++)
            {
                InputBinding binding = action.bindings[index];
                if (!string.IsNullOrWhiteSpace(compositePart)
                    && !string.Equals(binding.name, compositePart, StringComparison.OrdinalIgnoreCase)) continue;
                if (string.IsNullOrWhiteSpace(compositePart) && binding.isPartOfComposite) continue;
                string path = string.IsNullOrWhiteSpace(binding.effectivePath) ? binding.path : binding.effectivePath;
                if (!string.IsNullOrWhiteSpace(path) && path.StartsWith("<Keyboard>", StringComparison.OrdinalIgnoreCase))
                    return index;
            }
            return -1;
        }

        public string GetBindingDisplay(string actionName, int bindingIndex)
        {
            InputAction action = FindAction(actionName);
            if (action == null || bindingIndex < 0 || bindingIndex >= action.bindings.Count) return "NO ASIGNADO";
            string display = action.GetBindingDisplayString(bindingIndex, out _, out _);
            return string.IsNullOrWhiteSpace(display) ? "NO ASIGNADO" : display.ToUpperInvariant();
        }

        public bool StartInteractiveRebind(string actionName, int bindingIndex, Action<string> completed, Action cancelled = null)
        {
            InputAction action = FindAction(actionName);
            if (action == null || bindingIndex < 0 || bindingIndex >= action.bindings.Count) return false;

            CancelInteractiveRebind();
            action.Disable();
            _rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("<Mouse>/position")
                .WithControlsExcluding("<Mouse>/delta")
                .WithControlsExcluding("<XRController>")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnCancel(operation =>
                {
                    operation.Dispose();
                    _rebindOperation = null;
                    action.Enable();
                    cancelled?.Invoke();
                })
                .OnComplete(operation =>
                {
                    operation.Dispose();
                    _rebindOperation = null;
                    action.Enable();
                    completed?.Invoke(GetBindingDisplay(actionName, bindingIndex));
                });
            _rebindOperation.Start();
            return true;
        }

        public string SaveBindingOverrides() => Actions != null ? Actions.SaveBindingOverridesAsJson() : string.Empty;

        public void ApplyBindingOverrides(string json)
        {
            if (Actions == null || string.IsNullOrWhiteSpace(json)) return;
            try { Actions.LoadBindingOverridesFromJson(json); }
            catch (Exception exception) { Debug.LogWarning($"[InputRouter] Could not load control bindings: {exception.Message}"); }
        }

        public void ResetBindingOverrides()
        {
            CancelInteractiveRebind();
            Actions?.RemoveAllBindingOverrides();
        }

        public void CancelInteractiveRebind()
        {
            if (_rebindOperation == null) return;
            _rebindOperation.Cancel();
        }

        private void OnDestroy()
        {
            CancelInteractiveRebind();
            _gameplay?.Disable();
            if (Actions != null) Destroy(Actions);
            if (Instance == this) Instance = null;
        }
    }
}
