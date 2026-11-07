using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    [RequireComponent(typeof(Collider))]
    public sealed class ChaseSafeZone : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _detectionSuppression = 3f;
        [SerializeField] private bool _oneShot;
        private bool _used;

        private void Awake() => GetComponent<Collider>().isTrigger = true;

        private void OnTriggerEnter(Collider other)
        {
            if ((_oneShot && _used) || !other.CompareTag("Player")) return;
            _used = true;
            ChaseDirector.Instance?.EndAllChases(_detectionSuppression);
            TensionDirector.Instance?.SuppressFor(_detectionSuppression);
        }
    }
}
