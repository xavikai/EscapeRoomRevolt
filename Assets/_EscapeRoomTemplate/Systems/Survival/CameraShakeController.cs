using EscapeRoomRevolt.Core.Settings;
using EscapeRoomRevolt.Player;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    /// <summary>
    /// Additive trauma-based camera shake. Runs in LateUpdate so it always layers on top of
    /// whatever PlayerMovement/mouse-look assigned this frame instead of fighting over the
    /// transform — those systems set absolute position/rotation every Update(), so there is
    /// nothing to undo next frame; this only ever adds a temporary offset on top.
    /// Skips itself entirely in VR: an artificial positional/rotational shake of the HMD camera
    /// is a well-known motion-sickness trigger, unlike on a monitor.
    /// Call Shake() from any feedback controller; respects the "reduceScreenShake" accessibility
    /// setting. Auto-added to the player by Bootstrapper — no manual scene wiring required.
    /// </summary>
    public sealed class CameraShakeController : MonoBehaviour
    {
        public static CameraShakeController Instance { get; private set; }

        [SerializeField, Range(0f, 1f)] private float _reducedIntensityCap = .25f;
        [SerializeField, Min(0f)] private float _maxPositionOffset = .05f;
        [SerializeField, Min(0f)] private float _maxRotationOffset = 2f;
        [SerializeField, Min(0f)] private float _decayPerSecond = 1.5f;
        [SerializeField, Min(.1f)] private float _frequency = 18f;

        private Transform _shakeTransform;
        private float _trauma;
        private float _noiseSeed;
        private Vector3 _previousPositionOffset;
        private Quaternion _previousRotationOffset = Quaternion.identity;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            _noiseSeed = Random.Range(0f, 1000f);
        }

        private void Start()
        {
            _shakeTransform = Camera.main != null ? Camera.main.transform : null;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Adds shake trauma (0-1). Repeated calls stack up to 1; trauma decays on its own afterwards.</summary>
        public void Shake(float intensity)
        {
            bool reduced = GameSettingsService.Instance != null && GameSettingsService.Instance.Data.reduceScreenShake;
            float cap = reduced ? _reducedIntensityCap : 1f;
            _trauma = Mathf.Clamp01(Mathf.Max(_trauma, Mathf.Min(intensity, cap)));
        }

        private void LateUpdate()
        {
            if (_shakeTransform == null) return;
            if (PlayerPlatformRegistry.Current != null && PlayerPlatformRegistry.Current.Platform == PlayerPlatform.VirtualReality)
                _trauma = 0f;

            Vector3 newPositionOffset = Vector3.zero;
            Quaternion newRotationOffset = Quaternion.identity;

            if (_trauma > 0f)
            {
                float shakeAmount = _trauma * _trauma;
                float time = Time.unscaledTime * _frequency;
                float offsetX = Mathf.PerlinNoise(_noiseSeed, time) * 2f - 1f;
                float offsetY = Mathf.PerlinNoise(_noiseSeed + 100f, time) * 2f - 1f;
                float offsetZ = Mathf.PerlinNoise(_noiseSeed + 200f, time) * 2f - 1f;

                newPositionOffset = new Vector3(offsetX, offsetY, 0f) * shakeAmount * _maxPositionOffset;
                newRotationOffset = Quaternion.Euler(
                    offsetY * shakeAmount * _maxRotationOffset,
                    offsetX * shakeAmount * _maxRotationOffset,
                    offsetZ * shakeAmount * _maxRotationOffset * .5f);

                _trauma = Mathf.Max(0f, _trauma - _decayPerSecond * Time.unscaledDeltaTime);
            }

            // Undo exactly what last frame added before applying the new offset. This keeps the
            // result bounded and self-correcting even if PlayerMovement did not reassign a clean
            // base position this frame (e.g. movement frozen behind a UI panel).
            _shakeTransform.localPosition += newPositionOffset - _previousPositionOffset;
            _shakeTransform.localRotation = _shakeTransform.localRotation * Quaternion.Inverse(_previousRotationOffset) * newRotationOffset;
            _previousPositionOffset = newPositionOffset;
            _previousRotationOffset = newRotationOffset;
        }
    }
}
