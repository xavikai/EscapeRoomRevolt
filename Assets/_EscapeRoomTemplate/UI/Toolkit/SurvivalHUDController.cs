using EscapeRoomRevolt.Core.Settings;
using EscapeRoomRevolt.Core.Flow;
using EscapeRoomRevolt.Systems.Survival;
using UnityEngine;
using UnityEngine.UIElements;

namespace EscapeRoomRevolt.UI.Toolkit
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class SurvivalHUDController : MonoBehaviour
    {
        private VisualElement _vitalsHud;
        private VisualElement _healthFill;
        private VisualElement _staminaFill;
        private Label _healthPercent;
        private Label _staminaPercent;
        private VisualElement _camcorderHud;
        private VisualElement _nightVisionFill;
        private Label _camcorderState;
        private Label _nightVisionPercent;
        private Label _recordingTarget;
        private VisualElement _recordingFill;
        private PlayerVitals _vitals;
        private NightVisionController _nightVision;
        private CamcorderEvidenceRecorder _recorder;
        private Label _objectiveText;
        private ObjectiveManager _objectives;

        private void OnEnable()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            _vitalsHud = root.Q("vitals-hud");
            _healthFill = root.Q("health-fill");
            _staminaFill = root.Q("stamina-fill");
            _healthPercent = root.Q<Label>("health-percent");
            _staminaPercent = root.Q<Label>("stamina-percent");
            _camcorderHud = root.Q("camcorder-hud");
            _nightVisionFill = root.Q("nightvision-fill");
            _camcorderState = root.Q<Label>("camcorder-state");
            _nightVisionPercent = root.Q<Label>("nightvision-percent");
            _recordingTarget = root.Q<Label>("recording-target");
            _recordingFill = root.Q("recording-fill");
            _objectiveText = root.Q<Label>("objective-text");
            Invoke(nameof(BindRuntimeSystems), 0f);
        }

        private void OnDisable() => Unbind();

        private void BindRuntimeSystems()
        {
            Unbind();
            bool vitalsEnabled = GameFeatures.IsEnabled(OptionalGameFeature.PlayerVitals);
            bool cameraEnabled = GameFeatures.IsEnabled(OptionalGameFeature.NightVision);
            SetVisible(_vitalsHud, vitalsEnabled);
            SetVisible(_camcorderHud, false);

            if (vitalsEnabled)
            {
                _vitals = PlayerVitals.Instance != null ? PlayerVitals.Instance : FindAnyObjectByType<PlayerVitals>();
                if (_vitals != null)
                {
                    _vitals.HealthChanged += UpdateHealth;
                    _vitals.StaminaChanged += UpdateStamina;
                    UpdateHealth(_vitals.Health01);
                    UpdateStamina(_vitals.Stamina01);
                }
            }

            if (cameraEnabled)
            {
                _nightVision = FindAnyObjectByType<NightVisionController>();
                if (_nightVision != null)
                {
                    _nightVision.StateChanged += UpdateCamera;
                    _nightVision.ChargeChanged += UpdateNightVisionCharge;
                    _nightVision.BatteryStateChanged += UpdateNightVisionBatteryState;
                    UpdateCamera();
                    UpdateNightVisionCharge(_nightVision.Charge01);
                    UpdateNightVisionBatteryState(_nightVision.BatteryState);
                }
                _recorder = FindAnyObjectByType<CamcorderEvidenceRecorder>();
                if (_recorder != null)
                {
                    _recorder.FocusChanged += UpdateRecordingTarget;
                    _recorder.ProgressChanged += UpdateRecordingProgress;
                    _recorder.RecordingStateChanged += HandleRecordingState;
                    UpdateRecordingTarget(_recorder.CurrentTarget);
                    UpdateRecordingProgress(_recorder.Progress01);
                }
            }

            _objectives = FindAnyObjectByType<ObjectiveManager>();
            if (_objectives != null && GameFeatures.Genre == GameGenre.SurvivalHorror)
            {
                _objectives.ObjectivesChanged += UpdateObjective;
                _objectives.ObjectiveCompleted += HandleObjectiveCompleted;
                UpdateObjective();
            }
            else SetVisible(_objectiveText, false);
        }

        private void Unbind()
        {
            if (_vitals != null)
            {
                _vitals.HealthChanged -= UpdateHealth;
                _vitals.StaminaChanged -= UpdateStamina;
            }
            if (_nightVision != null)
            {
                _nightVision.StateChanged -= UpdateCamera;
                _nightVision.ChargeChanged -= UpdateNightVisionCharge;
                _nightVision.BatteryStateChanged -= UpdateNightVisionBatteryState;
            }
            if (_recorder != null)
            {
                _recorder.FocusChanged -= UpdateRecordingTarget;
                _recorder.ProgressChanged -= UpdateRecordingProgress;
                _recorder.RecordingStateChanged -= HandleRecordingState;
            }
            if (_objectives != null)
            {
                _objectives.ObjectivesChanged -= UpdateObjective;
                _objectives.ObjectiveCompleted -= HandleObjectiveCompleted;
            }
            _vitals = null;
            _nightVision = null;
            _recorder = null;
            _objectives = null;
        }

        private void UpdateHealth(float value)
        {
            SetWidth(_healthFill, value);
            if (_healthPercent != null) _healthPercent.text = $"{Mathf.RoundToInt(value * 100f)}%";
        }

        private void UpdateStamina(float value)
        {
            SetWidth(_staminaFill, value);
            if (_staminaPercent != null) _staminaPercent.text = $"{Mathf.RoundToInt(value * 100f)}%";
        }

        private void UpdateCamera()
        {
            if (_camcorderState == null || _nightVision == null) return;
            SetVisible(_camcorderHud, _nightVision.IsEquipped);
            _camcorderState.text = !_nightVision.IsCamcorderRaised ? "BAIXADA"
                : _recorder != null && _recorder.IsRecording ? "REC"
                : _nightVision.IsNightVisionEnabled ? "NV ACTIVA"
                : _nightVision.IsZoomed ? "ZOOM" : "PREPARADA";
        }

        private void UpdateNightVisionCharge(float value)
        {
            SetWidth(_nightVisionFill, value);
            if (_nightVisionPercent != null) _nightVisionPercent.text = $"{Mathf.RoundToInt(value * 100f)}%";
        }

        private void UpdateNightVisionBatteryState(CamcorderBatteryState state)
        {
            if (_nightVisionFill == null) return;
            _nightVisionFill.RemoveFromClassList("battery-fill--low");
            _nightVisionFill.RemoveFromClassList("battery-fill--critical");
            if (state == CamcorderBatteryState.Low) _nightVisionFill.AddToClassList("battery-fill--low");
            else if (state is CamcorderBatteryState.Critical or CamcorderBatteryState.Empty)
                _nightVisionFill.AddToClassList("battery-fill--critical");
        }

        private void UpdateRecordingTarget(RecordableEvidence target)
        {
            if (_recordingTarget == null) return;
            if (target == null) _recordingTarget.text = "SUBJECTE // CAP";
            else if (target.IsRecorded) _recordingTarget.text = $"ARXIVADA // {target.Definition.Title.ToUpperInvariant()}";
            else _recordingTarget.text = $"SUBJECTE // {target.Definition.Title.ToUpperInvariant()}";
        }

        private void UpdateRecordingProgress(float value) => SetWidth(_recordingFill, value);

        private void HandleRecordingState(bool _) => UpdateCamera();

        private void HandleObjectiveCompleted(ObjectiveDefinition _) => UpdateObjective();

        private void UpdateObjective()
        {
            if (_objectiveText == null || _objectives == null) return;
            ObjectiveDefinition current = null;
            foreach (ObjectiveDefinition objective in _objectives.GetVisibleObjectives())
            {
                if (objective == null || _objectives.IsComplete(objective.ObjectiveId) || !_objectives.IsAvailable(objective)) continue;
                current = objective;
                break;
            }
            SetVisible(_objectiveText, current != null);
            if (current != null) _objectiveText.text = $"OBJECTIU // {current.Title.ToUpperInvariant()}";
        }

        private static void SetWidth(VisualElement element, float value)
        {
            if (element != null) element.style.width = Length.Percent(Mathf.Clamp01(value) * 100f);
        }

        private static void SetVisible(VisualElement element, bool value)
        {
            if (element != null) element.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
