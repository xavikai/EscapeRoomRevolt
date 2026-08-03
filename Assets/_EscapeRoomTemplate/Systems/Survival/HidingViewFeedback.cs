using EscapeRoomRevolt.Core.Settings;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EscapeRoomRevolt.Systems.Survival
{
    /// <summary>
    /// Turns being inside a HidingSpot into a screen vignette that tightens with the spot's
    /// breathing intensity, selling a "peering through a gap" sensation without touching the
    /// camera's field of view (which would be uncomfortable in VR). Uses a higher Volume
    /// priority than SanityFeedbackController and fades in/out via weight rather than intensity,
    /// so it overlays the sanity vignette while hidden and cleanly reveals it again on exit
    /// instead of permanently forcing the shared Vignette parameter to zero.
    /// Created automatically by Bootstrapper when OptionalGameFeature.Hiding is active.
    /// </summary>
    public sealed class HidingViewFeedback : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float _baseVignette = .55f;
        [SerializeField, Range(0f, 1f)] private float _breathingVignetteBoost = .3f;
        [SerializeField] private Color _vignetteColor = Color.black;
        [SerializeField, Min(0f)] private float _fadeSpeed = 3.5f;
        [Tooltip("Caps intensity when the player enabled 'Reduce Flashes' in settings.")]
        [SerializeField, Range(0f, 1f)] private float _reducedFlashesCap = .35f;

        private Volume _volume;
        private Vignette _vignette;

        private void Awake()
        {
            if (!GameFeatures.IsEnabled(OptionalGameFeature.Hiding)) { enabled = false; return; }
            BuildVolume();
        }

        private void Update()
        {
            HidingSpot active = HidingSpot.ActiveForPlayer;
            float targetWeight = active != null ? 1f : 0f;
            _volume.weight = Mathf.MoveTowards(_volume.weight, targetWeight, _fadeSpeed * Time.deltaTime);

            if (active == null) return;
            bool reduceFlashes = GameSettingsService.Instance != null && GameSettingsService.Instance.Data.reduceFlashes;
            float cap = reduceFlashes ? _reducedFlashesCap : 1f;
            _vignette.intensity.value = Mathf.Min(_baseVignette + active.BreathingIntensity * _breathingVignetteBoost, cap);
        }

        private void BuildVolume()
        {
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            _vignette = profile.Add<Vignette>(true);
            _vignette.color.Override(_vignetteColor);
            _vignette.intensity.Override(0f);
            _vignette.smoothness.Override(.3f);
            _vignette.rounded.Override(true);

            var volumeObject = new GameObject("HidingVignetteVolume");
            volumeObject.transform.SetParent(transform, false);
            _volume = volumeObject.AddComponent<Volume>();
            _volume.isGlobal = true;
            _volume.priority = 10f;
            _volume.weight = 0f;
            _volume.profile = profile;
        }
    }
}
