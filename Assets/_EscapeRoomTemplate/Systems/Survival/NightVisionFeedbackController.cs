using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EscapeRoomRevolt.Systems.Survival
{
    /// <summary>
    /// Renders the green-tinted, grainy night-vision viewfinder look on the player's own camera
    /// while NightVisionController's night vision is active. This is a screen effect only — the
    /// illuminator light (if any) still simulates the physical IR emitter and is handled separately
    /// by NightVisionController. Add alongside NightVisionController on any camcorder prefab; no
    /// other wiring required.
    /// </summary>
    [RequireComponent(typeof(NightVisionController))]
    public sealed class NightVisionFeedbackController : MonoBehaviour
    {
        [Header("Look")]
        [SerializeField] private Color _tint = new Color(.3f, 1.2f, .35f);
        [SerializeField, Range(0f, 6f)] private float _postExposure = 3.5f;
        [SerializeField, Range(-100f, 0f)] private float _saturation = -60f;
        [SerializeField, Range(0f, 1f)] private float _grainIntensity = .7f;
        [SerializeField, Range(0f, 1f)] private float _vignetteIntensity = .35f;
        [SerializeField, Min(0f)] private float _fadeSpeed = 4f;

        private NightVisionController _camcorder;
        private Volume _volume;
        private float _targetWeight;

        private void Awake()
        {
            _camcorder = GetComponent<NightVisionController>();
            BuildVolume();
        }

        private void OnEnable()
        {
            if (_camcorder == null) _camcorder = GetComponent<NightVisionController>();
            _camcorder.StateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            if (_camcorder != null) _camcorder.StateChanged -= HandleStateChanged;
        }

        private void Update()
        {
            if (_volume == null) return;
            _volume.weight = Mathf.MoveTowards(_volume.weight, _targetWeight, _fadeSpeed * Time.deltaTime);
        }

        private void HandleStateChanged()
        {
            _targetWeight = _camcorder.IsNightVisionEnabled ? 1f : 0f;
            EnsurePostProcessingEnabled();
        }

        private void EnsurePostProcessingEnabled()
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            UniversalAdditionalCameraData camData = cam.GetUniversalAdditionalCameraData();
            if (camData != null) camData.renderPostProcessing = true;
        }

        private void BuildVolume()
        {
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();

            ColorAdjustments colorAdjustments = profile.Add<ColorAdjustments>(true);
            colorAdjustments.colorFilter.Override(_tint);
            colorAdjustments.postExposure.Override(_postExposure);
            colorAdjustments.saturation.Override(_saturation);

            FilmGrain filmGrain = profile.Add<FilmGrain>(true);
            filmGrain.type.Override(FilmGrainLookup.Medium1);
            filmGrain.intensity.Override(_grainIntensity);

            Vignette vignette = profile.Add<Vignette>(true);
            vignette.color.Override(Color.black);
            vignette.intensity.Override(_vignetteIntensity);
            vignette.smoothness.Override(.85f);
            vignette.rounded.Override(true);

            var volumeObject = new GameObject("NightVisionVolume");
            volumeObject.transform.SetParent(transform, false);
            _volume = volumeObject.AddComponent<Volume>();
            _volume.isGlobal = true;
            _volume.weight = 0f;
            _volume.profile = profile;
        }
    }
}
