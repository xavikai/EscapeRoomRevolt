using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    /// <summary>Turns meaningful rigidbody impacts into AI-hearing stimuli.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class GameplayImpactNoiseEmitter : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _minimumImpactSpeed = 1.25f;
        [SerializeField, Min(0f)] private float _loudImpactSpeed = 7f;
        [SerializeField, Min(0f)] private float _minimumRadius = 2.5f;
        [SerializeField, Min(0f)] private float _maximumRadius = 13f;
        [SerializeField, Min(0f)] private float _cooldown = .18f;
        [SerializeField, Min(1f)] private float _thrownMultiplier = 1.35f;

        private float _nextEmission;
        private float _thrownUntil;

        public void NotifyThrown(float memorySeconds = 2.5f)
        {
            _thrownUntil = Mathf.Max(_thrownUntil, Time.time + Mathf.Max(0f, memorySeconds));
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (Time.time < _nextEmission) return;
            float speed = collision.relativeVelocity.magnitude;
            if (speed < _minimumImpactSpeed) return;

            float denominator = Mathf.Max(.01f, _loudImpactSpeed - _minimumImpactSpeed);
            float normalized = Mathf.Clamp01((speed - _minimumImpactSpeed) / denominator);
            float radius = Mathf.Lerp(_minimumRadius, _maximumRadius, normalized);
            if (Time.time <= _thrownUntil) radius *= _thrownMultiplier;

            Vector3 position = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
            GameplayNoise.Emit(position, radius, GameplayNoiseType.Impact, gameObject);
            _nextEmission = Time.time + _cooldown;
        }
    }
}
