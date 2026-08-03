using EscapeRoomRevolt.Core.Settings;
using UnityEngine;

namespace EscapeRoomRevolt.Player.PC
{
    /// <summary>
    /// Additive procedural head bob layered on the camera in LateUpdate — PlayerMovement and mouse
    /// look assign absolute position/rotation every Update(), so adding an offset afterwards never
    /// fights over the transform (same trick as CameraShakeController). PC-only: VR players already
    /// have real physical head movement, and an artificial bob on top of that would be a much worse
    /// motion-sickness trigger than on a monitor, so this never runs on the VR rig.
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    public sealed class HeadBobController : MonoBehaviour
    {
        [Header("Frequency (cycles per second)")]
        [SerializeField, Min(0f)] private float _walkFrequency = 1.8f;
        [SerializeField, Min(0f)] private float _sprintFrequency = 2.6f;
        [SerializeField, Min(0f)] private float _crouchFrequency = 1.2f;

        [Header("Amplitude")]
        [SerializeField, Min(0f)] private float _verticalAmplitude = .045f;
        [SerializeField, Min(0f)] private float _horizontalAmplitude = .03f;
        [SerializeField, Min(0f)] private float _blendSpeed = 10f;

        [Header("Accessibility")]
        [SerializeField, Range(0f, 1f)] private float _reducedIntensityCap = .3f;

        private PlayerMovement _movement;
        private Vector3 _lastPosition;
        private float _phase;
        private float _currentWeight;
        private Vector3 _previousOffset;

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _lastPosition = transform.position;
        }

        private void LateUpdate()
        {
            Transform view = _movement.ViewTransform;
            if (view == null) return;

            Vector3 delta = transform.position - _lastPosition;
            _lastPosition = transform.position;
            float horizontalSpeed = Time.deltaTime > 0f
                ? new Vector2(delta.x, delta.z).magnitude / Time.deltaTime
                : 0f;
            bool moving = horizontalSpeed > .1f;

            float targetWeight = moving ? 1f : 0f;
            _currentWeight = Mathf.MoveTowards(_currentWeight, targetWeight, _blendSpeed * Time.deltaTime);

            if (moving)
            {
                float frequency = _movement.IsCrouching ? _crouchFrequency : _movement.IsSprinting ? _sprintFrequency : _walkFrequency;
                _phase += Time.deltaTime * frequency * Mathf.PI * 2f;
            }

            Vector3 newOffset = Vector3.zero;
            if (_currentWeight > 0f)
            {
                bool reduced = GameSettingsService.Instance != null && GameSettingsService.Instance.Data.reduceHeadBob;
                float amount = _currentWeight * (reduced ? _reducedIntensityCap : 1f);
                float verticalOffset = Mathf.Sin(_phase) * _verticalAmplitude * amount;
                float horizontalOffset = Mathf.Cos(_phase * .5f) * _horizontalAmplitude * amount;
                newOffset = new Vector3(horizontalOffset, verticalOffset, 0f);
            }

            // Undo exactly what last frame added before applying the new offset. This keeps the
            // result bounded and self-correcting even if PlayerMovement did not reassign a clean
            // base position this frame (e.g. movement frozen behind a UI panel).
            view.localPosition += newOffset - _previousOffset;
            _previousOffset = newOffset;
        }
    }
}
