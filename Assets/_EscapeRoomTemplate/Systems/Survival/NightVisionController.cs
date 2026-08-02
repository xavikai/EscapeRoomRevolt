using System;
using EscapeRoomRevolt.Core.Input;
using EscapeRoomRevolt.Core.Save;
using EscapeRoomRevolt.Core.Settings;
using EscapeRoomRevolt.Player;
using EscapeRoomRevolt.Systems.Equipment;
using EscapeRoomRevolt.Systems.Inventory;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomRevolt.Systems.Survival
{
    public enum CamcorderBatteryState { Normal, Low, Critical, Empty }

    [Serializable]
    public sealed class NightVisionSaveState
    {
        public int version = 2;
        public float charge;
        public bool camcorderRaised;
        public bool nightVisionEnabled;
    }

    /// <summary>Equippable camcorder logic. Presentation stays replaceable and vendor-neutral.</summary>
    public sealed class NightVisionController : MonoBehaviour, ISaveable, IEquipmentLifecycle
    {
        [Header("Presentation")]
        [SerializeField] private Camera _viewCamera;
        [SerializeField] private Light _nightVisionIlluminator;
        [SerializeField] private GameObject _visualRoot;

        [Header("Battery")]
        [SerializeField, Min(1f)] private float _maxCharge = 100f;
        [SerializeField, Range(0f, 1f)] private float _startingCharge01 = 1f;
        [SerializeField, Min(0f)] private float _drainPerSecond = 6f;
        [SerializeField] private string _batteryItemId = "camcorder_battery";
        [SerializeField, Range(0f, 1f)] private float _lowThreshold = .25f;
        [SerializeField, Range(0f, 1f)] private float _criticalThreshold = .1f;

        [Header("Zoom")]
        [SerializeField, Range(10f, 80f)] private float _zoomFieldOfView = 32f;
        [SerializeField, Min(1f)] private float _zoomSpeed = 90f;

        [Header("Initial state")]
        [SerializeField] private bool _startRaised = true;
        [SerializeField] private bool _startWithNightVision;

        [Header("Presentation hooks")]
        [SerializeField] private UnityEvent _onRaised;
        [SerializeField] private UnityEvent _onLowered;
        [SerializeField] private UnityEvent _onNightVisionEnabled;
        [SerializeField] private UnityEvent _onNightVisionDisabled;
        [SerializeField] private UnityEvent _onBatteryReloaded;
        [SerializeField] private UnityEvent _onBatteryLow;
        [SerializeField] private UnityEvent _onBatteryCritical;
        [SerializeField] private UnityEvent _onBatteryEmpty;
        [SerializeField] private UnityEvent<float> _onChargeNormalizedChanged;

        private float _charge;
        private float _normalFieldOfView = 60f;
        private bool _camcorderRaised;
        private bool _nightVisionEnabled;
        private bool _isEquipped;
        private bool _isZoomed;
        private CamcorderBatteryState _batteryState;

        public float Charge => _charge;
        public float Charge01 => _maxCharge > 0f ? _charge / _maxCharge : 0f;
        public bool IsEquipped => _isEquipped;
        public bool IsCamcorderRaised => _isEquipped && _camcorderRaised;
        public bool IsNightVisionEnabled => IsCamcorderRaised && _nightVisionEnabled;
        public bool IsZoomed => IsCamcorderRaised && _isZoomed;
        public Camera ViewCamera => ResolveViewCamera();
        public CamcorderBatteryState BatteryState => _batteryState;

        public event Action StateChanged;
        public event Action<float> ChargeChanged;
        public event Action<bool> ZoomChanged;
        public event Action<CamcorderBatteryState> BatteryStateChanged;

        private void Awake()
        {
            if (!GameFeatures.IsEnabled(OptionalGameFeature.NightVision))
            {
                gameObject.SetActive(false);
                return;
            }

            _charge = _maxCharge * _startingCharge01;
            _camcorderRaised = _startRaised;
            _nightVisionEnabled = _startWithNightVision && _startRaised && _charge > 0f;
            _batteryState = EvaluateBatteryState();
            ApplyPresentation();
            SaveManager.Instance?.Register(this);
        }

        private void OnDestroy()
        {
            RestoreNormalFieldOfView();
            SaveManager.Instance?.Unregister(this);
        }

        private void Update()
        {
            if (!_isEquipped) return;
            InputRouter input = InputRouter.Instance;
            if (!IsGameplayBlocked() && input != null)
            {
                if (input.ToggleCamcorderPressed) SetCamcorderRaised(!_camcorderRaised);
                if (input.ToggleNightVisionPressed) SetNightVisionEnabled(!_nightVisionEnabled);
                if (input.ReloadCamcorderPressed) ReloadBattery();
                SetZoomed(input.CamcorderZoomHeld);
            }
            else SetZoomed(false);

            UpdateZoom();
            if (!IsNightVisionEnabled) return;
            SetCharge(_charge - _drainPerSecond * SurvivalDifficultyService.ResourceConsumption * Time.deltaTime);
            if (_charge <= 0f) SetNightVisionEnabled(false);
        }

        public void OnEquipped()
        {
            _isEquipped = true;
            ResolveViewCamera();
            ApplyPresentation();
            StateChanged?.Invoke();
        }

        public void OnUnequipped()
        {
            _isEquipped = false;
            _nightVisionEnabled = false;
            SetZoomed(false);
            RestoreNormalFieldOfView();
            ApplyPresentation();
            StateChanged?.Invoke();
        }

        public void SetCamcorderRaised(bool value)
        {
            if (!_isEquipped) value = false;
            if (_camcorderRaised == value) return;
            _camcorderRaised = value;
            if (!value)
            {
                _nightVisionEnabled = false;
                SetZoomed(false);
                RestoreNormalFieldOfView();
                _onLowered?.Invoke();
            }
            else _onRaised?.Invoke();
            ApplyPresentation();
            StateChanged?.Invoke();
        }

        public void SetNightVisionEnabled(bool value)
        {
            value = value && IsCamcorderRaised && _charge > 0f;
            if (_nightVisionEnabled == value) return;
            _nightVisionEnabled = value;
            ApplyPresentation();
            if (value) _onNightVisionEnabled?.Invoke();
            else _onNightVisionDisabled?.Invoke();
            StateChanged?.Invoke();
        }

        public void SetZoomed(bool value)
        {
            value = value && IsCamcorderRaised;
            if (_isZoomed == value) return;
            _isZoomed = value;
            ZoomChanged?.Invoke(value);
            StateChanged?.Invoke();
        }

        public bool ReloadBattery()
        {
            if (!_isEquipped || _charge >= _maxCharge - .01f) return false;
            InventoryManager inventory = InventoryManager.Instance;
            if (inventory == null || !inventory.UseItem(_batteryItemId)) return false;
            SetCharge(_maxCharge);
            _onBatteryReloaded?.Invoke();
            StateChanged?.Invoke();
            return true;
        }

        /// <summary>Testing/accessibility API; does not consume inventory.</summary>
        public void SetChargeNormalized(float value) => SetCharge(Mathf.Clamp01(value) * _maxCharge);

        private void SetCharge(float value)
        {
            float previous = _charge;
            _charge = Mathf.Clamp(value, 0f, _maxCharge);
            if (!Mathf.Approximately(previous, _charge))
            {
                ChargeChanged?.Invoke(Charge01);
                _onChargeNormalizedChanged?.Invoke(Charge01);
            }
            CamcorderBatteryState next = EvaluateBatteryState();
            if (next != _batteryState)
            {
                _batteryState = next;
                BatteryStateChanged?.Invoke(next);
                if (next == CamcorderBatteryState.Low) _onBatteryLow?.Invoke();
                else if (next == CamcorderBatteryState.Critical) _onBatteryCritical?.Invoke();
                if (next == CamcorderBatteryState.Empty) _onBatteryEmpty?.Invoke();
            }
        }

        private CamcorderBatteryState EvaluateBatteryState()
        {
            float charge01 = Charge01;
            if (charge01 <= 0f) return CamcorderBatteryState.Empty;
            if (charge01 <= _criticalThreshold) return CamcorderBatteryState.Critical;
            if (charge01 <= _lowThreshold) return CamcorderBatteryState.Low;
            return CamcorderBatteryState.Normal;
        }

        private void ApplyPresentation()
        {
            bool presented = IsCamcorderRaised;
            if (_visualRoot != null) _visualRoot.SetActive(!_isEquipped || presented);
            if (_nightVisionIlluminator != null)
                _nightVisionIlluminator.enabled = presented && _nightVisionEnabled && _charge > 0f;
        }

        private void UpdateZoom()
        {
            Camera camera = ResolveViewCamera();
            if (camera == null) return;
            float target = IsZoomed ? _zoomFieldOfView : _normalFieldOfView;
            camera.fieldOfView = Mathf.MoveTowards(camera.fieldOfView, target, _zoomSpeed * Time.unscaledDeltaTime);
        }

        private Camera ResolveViewCamera()
        {
            if (_viewCamera != null) return _viewCamera;
            Transform head = PlayerPlatformRegistry.Current?.Head;
            if (head != null) _viewCamera = head.GetComponent<Camera>();
            if (_viewCamera == null) _viewCamera = Camera.main;
            if (_viewCamera != null) _normalFieldOfView = _viewCamera.fieldOfView;
            return _viewCamera;
        }

        private void RestoreNormalFieldOfView()
        {
            if (_viewCamera != null) _viewCamera.fieldOfView = _normalFieldOfView;
        }

        private static bool IsGameplayBlocked()
        {
            return (EscapeRoomRevolt.UI.PC.UIManager.Instance != null
                    && EscapeRoomRevolt.UI.PC.UIManager.Instance.IsUIBlockingGameplay)
                || (EscapeRoomRevolt.UI.Toolkit.UIToolkitMenuController.Instance != null
                    && EscapeRoomRevolt.UI.Toolkit.UIToolkitMenuController.Instance.IsBlockingGameplay);
        }

        public string SaveId => "NightVision";

        public string SaveData() => JsonUtility.ToJson(new NightVisionSaveState
        {
            charge = _charge,
            camcorderRaised = _camcorderRaised,
            nightVisionEnabled = _nightVisionEnabled
        });

        public void LoadData(string json)
        {
            NightVisionSaveState state = JsonUtility.FromJson<NightVisionSaveState>(json);
            if (state == null) return;
            _charge = Mathf.Clamp(state.charge, 0f, _maxCharge);
            _camcorderRaised = state.camcorderRaised;
            _nightVisionEnabled = state.nightVisionEnabled && _camcorderRaised && _charge > 0f;
            _batteryState = EvaluateBatteryState();
            ApplyPresentation();
            ChargeChanged?.Invoke(Charge01);
            _onChargeNormalizedChanged?.Invoke(Charge01);
            BatteryStateChanged?.Invoke(_batteryState);
            StateChanged?.Invoke();
        }
    }
}
