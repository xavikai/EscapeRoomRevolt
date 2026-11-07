using System;
using EscapeRoomRevolt.Core.Settings;
using EscapeRoomRevolt.Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EscapeRoomRevolt.Systems.Survival
{
    [Serializable]
    public sealed class SanityStageEvent : UnityEvent<SanityStage> { }

    /// <summary>
    /// Turns SanityController stage changes into a screen vignette and muffled audio, and
    /// exposes UnityEvent hooks so buyers can layer their own stingers/particles/camera shake.
    /// Created automatically by Bootstrapper when OptionalGameFeature.Sanity is active; can also
    /// be added manually to the player prefab to tune values/hooks in the Inspector.
    /// </summary>
    public sealed class SanityFeedbackController : MonoBehaviour
    {
        [Header("Vignette per stage (URP Volume, built at runtime)")]
        [SerializeField, Range(0f, 1f)] private float _uneasyVignette = .2f;
        [SerializeField, Range(0f, 1f)] private float _distressedVignette = .4f;
        [SerializeField, Range(0f, 1f)] private float _criticalVignette = .6f;
        [SerializeField] private Color _vignetteColor = new Color(.08f, 0f, 0f);
        [SerializeField, Min(0f)] private float _vignetteLerpSpeed = .6f;

        [Header("Audio muffling per stage (AudioLowPassFilter on the listener)")]
        [SerializeField] private float _stableCutoffHz = 22000f;
        [SerializeField] private float _uneasyCutoffHz = 5000f;
        [SerializeField] private float _distressedCutoffHz = 2200f;
        [SerializeField] private float _criticalCutoffHz = 900f;
        [SerializeField, Min(0f)] private float _cutoffLerpSpeed = 4000f;

        [Header("Accessibility")]
        [Tooltip("Caps vignette intensity when the player enabled 'Reduce Flashes' in settings.")]
        [SerializeField, Range(0f, 1f)] private float _reducedFlashesCap = .3f;

        [Header("Designer Hooks")]
        [SerializeField] private SanityStageEvent _onStageChanged;

        private SanityController _sanity;
        private Vignette _vignette;
        private AudioLowPassFilter _lowPass;
        private float _targetVignette;
        private float _targetCutoff;

        private void Awake()
        {
            if (!GameFeatures.IsEnabled(OptionalGameFeature.Sanity)) { enabled = false; return; }
            _targetCutoff = _stableCutoffHz;
            BuildVolume();
            BuildAudioFilter();
        }

private void Start()
        {
            _sanity = SanityController.Instance;
            if (_sanity == null) { enabled = false; return; }
            _sanity.StageChanged += HandleStageChanged;
            HandleStageChanged(_sanity.Stage);
        }

        private void OnDisable()
        {
            if (_sanity != null) _sanity.StageChanged -= HandleStageChanged;
        }

        private void Update()
        {
            if (_vignette != null)
                _vignette.intensity.value = Mathf.MoveTowards(_vignette.intensity.value, _targetVignette, _vignetteLerpSpeed * Time.deltaTime);
            if (_lowPass != null)
                _lowPass.cutoffFrequency = Mathf.MoveTowards(_lowPass.cutoffFrequency, _targetCutoff, _cutoffLerpSpeed * Time.deltaTime);
        }

        private void HandleStageChanged(SanityStage stage)
        {
            bool reduceFlashes = GameSettingsService.Instance != null && GameSettingsService.Instance.Data.reduceFlashes;
            float cap = reduceFlashes ? _reducedFlashesCap : 1f;
            switch (stage)
            {
                case SanityStage.Uneasy:
                    _targetVignette = Mathf.Min(_uneasyVignette, cap);
                    _targetCutoff = _uneasyCutoffHz;
                    break;
                case SanityStage.Distressed:
                    _targetVignette = Mathf.Min(_distressedVignette, cap);
                    _targetCutoff = _distressedCutoffHz;
                    break;
                case SanityStage.Critical:
                    _targetVignette = Mathf.Min(_criticalVignette, cap);
                    _targetCutoff = _criticalCutoffHz;
                    CameraShakeController.Instance?.Shake(.35f);
                    break;
                default:
                    _targetVignette = 0f;
                    _targetCutoff = _stableCutoffHz;
                    break;
            }
            SendHapticForStage(stage);
            _onStageChanged?.Invoke(stage);
        }

        /// <summary>Gives VR players a physical "heartbeat" cue when sanity worsens, reusing the same stage event that already drives the vignette/audio. No-ops on platforms without haptics.</summary>
        private static void SendHapticForStage(SanityStage stage)
        {
            IPlayerPlatformAdapter platform = PlayerPlatformRegistry.Current;
            if (platform == null || !platform.SupportsHaptics) return;

            float amplitude = stage switch
            {
                SanityStage.Uneasy => .15f,
                SanityStage.Distressed => .3f,
                SanityStage.Critical => .5f,
                _ => 0f
            };
            if (amplitude <= 0f) return;

            platform.SendHaptic(PlayerHand.Left, amplitude, .2f);
            platform.SendHaptic(PlayerHand.Right, amplitude, .2f);
        }

        private void BuildVolume()
        {
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            _vignette = profile.Add<Vignette>(true);
            _vignette.color.Override(_vignetteColor);
            _vignette.intensity.Override(0f);
            _vignette.smoothness.Override(.6f);

            var volumeObject = new GameObject("SanityVignetteVolume");
            volumeObject.transform.SetParent(transform, false);
            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.weight = 1f;
            volume.profile = profile;
        }

        private void BuildAudioFilter()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            UniversalAdditionalCameraData camData = cam.GetUniversalAdditionalCameraData();
            if (camData != null) camData.renderPostProcessing = true;

            AudioListener listener = cam.GetComponent<AudioListener>();
            GameObject host = listener != null ? listener.gameObject : cam.gameObject;
            _lowPass = host.GetComponent<AudioLowPassFilter>();
            if (_lowPass == null) _lowPass = host.AddComponent<AudioLowPassFilter>();
            _lowPass.cutoffFrequency = _stableCutoffHz;
        }
    }
}
