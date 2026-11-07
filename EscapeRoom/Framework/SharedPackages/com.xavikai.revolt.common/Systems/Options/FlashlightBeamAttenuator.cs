using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    /// <summary>
    /// Dims the flashlight as the surface it points at gets closer, so lighting a wall from up close
    /// doesn't blow it out to flat white.
    ///
    /// A spot light falls off with the square of the distance, which is physically right but reads
    /// badly without any exposure adaptation: at half a metre a beam tuned for a corridor is roughly
    /// forty times too bright. Real cameras and eyes stop down; this does the same by hand, scaling
    /// intensity with the distance to whatever is actually being lit.
    /// </summary>
    [RequireComponent(typeof(Light))]
    public sealed class FlashlightBeamAttenuator : MonoBehaviour
    {
        [Tooltip("Intensity used when the beam reaches far enough to need full power. Read from the Light on Awake if left at zero.")]
        [SerializeField, Min(0f)] private float _baseIntensity;
        [Tooltip("At or below this distance the beam is dimmed all the way to Minimum Factor.")]
        [SerializeField, Min(.05f)] private float _nearDistance = .6f;
        [Tooltip("At or beyond this distance the beam runs at full intensity.")]
        [SerializeField, Min(.1f)] private float _farDistance = 3.5f;
        [Tooltip("How much of the base intensity remains when pressed right up against a surface.")]
        [SerializeField, Range(.02f, 1f)] private float _minimumFactor = .16f;
        [Tooltip("How quickly the beam adapts. Low values feel like an iris opening; high values snap.")]
        [SerializeField, Min(.5f)] private float _adaptSpeed = 7f;
        [Tooltip("What counts as a surface. Leave as everything unless the beam is catching invisible helpers.")]
        [SerializeField] private LayerMask _surfaceMask = ~0;

        private Light _light;
        private Transform _ignoreRoot;
        private readonly RaycastHit[] _hits = new RaycastHit[8];

        private void Awake()
        {
            _light = GetComponent<Light>();
            if (_baseIntensity <= 0f) _baseIntensity = _light.intensity;
            _ignoreRoot = transform.root;
        }

        private void LateUpdate()
        {
            if (_light == null || !_light.enabled) return;

            float target = _baseIntensity * Mathf.Lerp(_minimumFactor, 1f, DistanceFactor());
            _light.intensity = Mathf.Lerp(_light.intensity, target, Time.deltaTime * _adaptSpeed);
        }

        /// <summary>0 when the beam is right against a surface, 1 when it reaches Far Distance or nothing at all.</summary>
        private float DistanceFactor()
        {
            int count = Physics.RaycastNonAlloc(transform.position, transform.forward, _hits,
                _farDistance, _surfaceMask, QueryTriggerInteraction.Ignore);

            float nearest = _farDistance;
            for (int i = 0; i < count; i++)
            {
                // The torch body (and the hand holding it) sit right at the origin of the ray and
                // would otherwise read as "pressed against a wall" the whole time.
                if (_ignoreRoot != null && _hits[i].transform.IsChildOf(_ignoreRoot)) continue;
                if (_hits[i].distance < nearest) nearest = _hits[i].distance;
            }

            return Mathf.InverseLerp(_nearDistance, _farDistance, nearest);
        }
    }
}
