using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomRevolt.Systems.Interaction
{
    /// <summary>
    /// A generic interactable that fires a UnityEvent.
    /// Use this to easily hook up buttons, switches, or triggers to lights, particles, or other scripts from the Inspector.
    /// </summary>
    public class InteractableTrigger : InteractableBase
    {
        [Header("Trigger Settings")]
        [SerializeField] private string _prompt = "Use";
        [Tooltip("If true, it can only be clicked once.")]
        [SerializeField] private bool _singleUse = false;
        [Tooltip("If true, clicking alternates between On and Off events.")]
        [SerializeField] private bool _isToggle = false;
        
        [Header("Events")]
        public UnityEvent OnInteractEvent;
        [Tooltip("Only fired if Is Toggle is checked.")]
        public UnityEvent OnInteractOffEvent;

        private bool _hasBeenUsed = false;
        private bool _isOn = false;

        public override string InteractionPrompt => _prompt;

        protected override void OnInteract()
        {
            if (_singleUse && _hasBeenUsed) return;

            _hasBeenUsed = true;

            if (_isToggle)
            {
                _isOn = !_isOn;
                if (_isOn) OnInteractEvent?.Invoke();
                else OnInteractOffEvent?.Invoke();
            }
            else
            {
                OnInteractEvent?.Invoke();
            }

            if (_singleUse)
            {
                // Optionally disable the collider so it can't be interacted with again
                var col = GetComponent<Collider>();
                if (col != null) col.enabled = false;
                
                EscapeRoomRevolt.Core.Save.SaveManager.Instance?.MarkAsDestroyed(SaveId);
            }
        }
    }
}
