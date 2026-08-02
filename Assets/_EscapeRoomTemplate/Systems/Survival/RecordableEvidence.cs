using EscapeRoomRevolt.Core.Settings;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    /// <summary>Authorable subject that can be recorded by the camcorder.</summary>
    [RequireComponent(typeof(Collider))]
    public sealed class RecordableEvidence : MonoBehaviour
    {
        [SerializeField] private EvidenceDefinition _definition;
        [SerializeField] private Transform _focusPoint;

        public EvidenceDefinition Definition => _definition;
        public Vector3 FocusPosition => _focusPoint != null ? _focusPoint.position : transform.position;
        public bool IsRecorded => _definition != null && EvidenceJournal.Instance != null
            && EvidenceJournal.Instance.IsRecorded(_definition.EvidenceId);

        private void Awake()
        {
            if (!GameFeatures.IsEnabled(OptionalGameFeature.EvidenceRecording)) gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (_definition != null) EvidenceJournal.EnsureInstance().RegisterDefinition(_definition);
        }

        public bool CanRecordFrom(Camera camera)
        {
            return camera != null && _definition != null
                && Vector3.Distance(camera.transform.position, FocusPosition) <= _definition.MaximumDistance;
        }

        public void Configure(EvidenceDefinition definition, Transform focusPoint = null)
        {
            _definition = definition;
            _focusPoint = focusPoint;
            if (isActiveAndEnabled && definition != null) EvidenceJournal.EnsureInstance().RegisterDefinition(definition);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(FocusPosition, .2f);
            if (_definition != null) Gizmos.DrawWireSphere(FocusPosition, _definition.MaximumDistance);
        }
    }
}
