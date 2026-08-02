using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomRevolt.Core.Flow
{
    /// <summary>Reusable endpoint invoked by a trigger, puzzle or UnityEvent.</summary>
    public sealed class GameEndTrigger : MonoBehaviour
    {
        [SerializeField] private EndingDefinition _ending;
        [SerializeField] private bool _activateOnPlayerEnter;
        [SerializeField] private bool _oneShot = true;
        [SerializeField] private UnityEvent _onTriggered;
        private bool _triggered;

        public void Trigger()
        {
            if (_oneShot && _triggered) return;
            _triggered = true;
            _onTriggered?.Invoke();
            GameFlowManager flow = GameFlowManager.EnsureInstance();
            if (_ending == null || _ending.Outcome == GameOutcome.Victory) flow.CompleteGame(_ending);
            else flow.FailGame(_ending);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_activateOnPlayerEnter && other.CompareTag("Player")) Trigger();
        }
    }
}
