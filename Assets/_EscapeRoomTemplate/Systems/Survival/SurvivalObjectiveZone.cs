using EscapeRoomRevolt.Core.Flow;
using EscapeRoomRevolt.Core.Settings;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    [RequireComponent(typeof(Collider))]
    public sealed class SurvivalObjectiveZone : MonoBehaviour
    {
        [SerializeField] private string _objectiveId = "escape_facility";
        [SerializeField] private bool _oneShot = true;
        private bool _triggered;

        private void Awake()
        {
            if (!GameFeatures.IsEnabled(OptionalGameFeature.EnemyAI)) { gameObject.SetActive(false); return; }
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if ((_oneShot && _triggered) || !other.CompareTag("Player")) return;
            if (ObjectiveManager.Instance == null || !ObjectiveManager.Instance.CompleteObjective(_objectiveId)) return;
            _triggered = true;
        }
    }
}
