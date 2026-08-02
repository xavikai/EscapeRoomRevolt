using System;
using EscapeRoomRevolt.Core.Save;
using EscapeRoomRevolt.Core.Input;
using EscapeRoomRevolt.Core.Settings;
using EscapeRoomRevolt.Systems.Equipment;
using EscapeRoomRevolt.Systems.Inventory;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    [Serializable] public class FlashlightState { public float charge; public bool enabled; }

    /// <summary>Configurable torch with persistent charge and inventory-driven battery reloads.</summary>
    public sealed class FlashlightController : MonoBehaviour, ISaveable, IEquipmentLifecycle
    {
        [SerializeField] private Light _light;
        [SerializeField, Range(0f, 1f)] private float _charge = 1f;
        [SerializeField, Min(0f)] private float _drainPerSecond = .006f;
        [SerializeField] private string _batteryItemId = "batteries";
        [SerializeField] private bool _startEnabled;
        private bool _isEquipped;
        private bool _requestedOn;
        private bool _featureEnabled;
        public float Charge01 => _charge;
        public bool IsOn => _light != null && _light.enabled;
        public bool IsEquipped => _isEquipped;
        public event Action<float> ChargeChanged;
        public event Action StateChanged;

        private void Awake()
        {
            _featureEnabled = GameFeatures.IsEnabled(OptionalGameFeature.Flashlight);
            if (_light == null) _light = GetComponentInChildren<Light>();
            _requestedOn = _startEnabled;
            if (_featureEnabled) SaveManager.Instance?.Register(this);
            Apply();
            if (!_featureEnabled) enabled = false;
        }
        private void OnDestroy() { if (_featureEnabled) SaveManager.Instance?.Unregister(this); }
        private void Update()
        {
            if (_light == null) return;
            if (_isEquipped)
            {
                bool togglePressed = InputRouter.Instance != null && InputRouter.Instance.ToggleFlashlightPressed;
                bool reloadPressed = InputRouter.Instance != null && InputRouter.Instance.ReloadPressed;
                if (togglePressed) Toggle();
                if (reloadPressed) TryReload();
            }
            if (!IsOn) return;
            _charge = Mathf.Max(0f, _charge - _drainPerSecond * SurvivalDifficultyService.ResourceConsumption * Time.deltaTime);
            if (_charge <= 0f) { _requestedOn = false; Apply(); }
            ChargeChanged?.Invoke(_charge);
        }
        public void Toggle()
        {
            if (!_featureEnabled || !_isEquipped || _charge <= 0f) return;
            _requestedOn = !_requestedOn;
            Apply();
        }
        public bool TryReload()
        {
            if (!_featureEnabled || !_isEquipped || _charge >= 1f || InventoryManager.Instance == null || !InventoryManager.Instance.UseItem(_batteryItemId)) return false;
            _charge = 1f; ChargeChanged?.Invoke(_charge); Apply(); return true;
        }
        public void OnEquipped() { _isEquipped = true; Apply(); }
        public void OnUnequipped() { _isEquipped = false; _requestedOn = false; Apply(); }
        private void Apply() { if (_light != null) _light.enabled = _featureEnabled && _isEquipped && _requestedOn && _charge > 0f; StateChanged?.Invoke(); }
        public string SaveId => "Flashlight";
        public string SaveData() => JsonUtility.ToJson(new FlashlightState { charge = _charge, enabled = _requestedOn });
        public void LoadData(string json) { var state = JsonUtility.FromJson<FlashlightState>(json); if (state == null) return; _charge = Mathf.Clamp01(state.charge); _requestedOn = state.enabled; Apply(); }
    }
}
