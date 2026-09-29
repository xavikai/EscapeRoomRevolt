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
        [SerializeField] private string _prompt = "Usar";
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

            if (_singleUse) DisableAfterUse();
        }

        // A spent single-use trigger stays visible but stops responding. It used to be marked as
        // destroyed, which made SaveManager delete the whole object (button, lever art and all)
        // when the game was loaded.
        private void DisableAfterUse()
        {
            SetInteractable(false);
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        [System.Serializable]
        private sealed class TriggerSaveData { public bool hasBeenUsed; public bool isOn; }

        public override string SaveData() =>
            JsonUtility.ToJson(new TriggerSaveData { hasBeenUsed = _hasBeenUsed, isOn = _isOn });

        /// <summary>Restores the used/toggle state without re-firing the events: the objects they drive restore their own state.</summary>
        public override void LoadData(string json)
        {
            TriggerSaveData data = JsonUtility.FromJson<TriggerSaveData>(json);
            if (data == null) return;
            _hasBeenUsed = data.hasBeenUsed;
            _isOn = data.isOn;
            if (_singleUse && _hasBeenUsed) DisableAfterUse();
        }
    }
}
