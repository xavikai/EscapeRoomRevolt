using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    /// <summary>Authorable dark/bright region. It does not depend on the rendering pipeline.</summary>
    [RequireComponent(typeof(Collider))]
    public sealed class VisibilityZone : MonoBehaviour
    {
        [SerializeField, Range(.05f, 2f)] private float _visibilityMultiplier = .45f;
        public float VisibilityMultiplier => _visibilityMultiplier;

        private void Awake() => GetComponent<Collider>().isTrigger = true;
        private void OnTriggerEnter(Collider other) => other.GetComponentInParent<PlayerVisibility>()?.Enter(this);
        private void OnTriggerExit(Collider other) => other.GetComponentInParent<PlayerVisibility>()?.Exit(this);

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _visibilityMultiplier < 1f
                ? new Color(.15f, .25f, .7f, .25f)
                : new Color(1f, .85f, .2f, .25f);
            Collider volume = GetComponent<Collider>();
            if (volume != null) Gizmos.DrawWireCube(volume.bounds.center, volume.bounds.size);
        }
    }
}
