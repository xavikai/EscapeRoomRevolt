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
        [Tooltip("Optional cinematic played before the results screen. Without this the results appear "
               + "instantly, which leaves no room for an ending cutscene: finishing freezes the game.")]
        [SerializeField] private CutsceneSequence _endingCutscene;
        [SerializeField] private UnityEvent _onTriggered;
        private bool _triggered;

        public void Trigger()
        {
            if (_oneShot && _triggered) return;
            _triggered = true;
            _onTriggered?.Invoke();

            if (_endingCutscene != null)
            {
                // Hand over to the cutscene and finish only once it is done, so the player actually
                // sees it. CutsceneSequence runs on unscaled time for exactly this reason.
                _endingCutscene.OnFinished.AddListener(Finish);
                _endingCutscene.Play();
                return;
            }

            Finish();
        }

        private void Finish()
        {
            if (_endingCutscene != null) _endingCutscene.OnFinished.RemoveListener(Finish);

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
