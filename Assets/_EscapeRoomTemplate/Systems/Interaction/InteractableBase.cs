using UnityEngine;

namespace EscapeRoomRevolt.Systems.Interaction
{
    /// <summary>
    /// Base class for all interactable objects. Inherit from this and
    /// override OnInteract() to add specific behaviour.
    ///
    /// Example:
    ///   public class Door : InteractableBase { ... }
    ///   public class Note : InteractableBase { ... }
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField] private string _interactionPrompt = "Interact";
        [SerializeField] private bool _canInteract = true;
        [SerializeField] private string _saveId = "";

        // ── IInteractable ────────────────────────────────────────────────────
        public virtual string InteractionPrompt => _interactionPrompt;
        public virtual bool CanInteract => _canInteract && gameObject.activeInHierarchy;

        public void Interact()
        {
            if (!CanInteract) return;
            OnInteract();
        }

        public virtual void OnFocusEnter()
        {
            // Override to add highlight effect, outline, etc.
        }

        public virtual void OnFocusExit()
        {
            // Override to remove highlight effect
        }

        // ── Abstract / Virtual ───────────────────────────────────────────────
        /// <summary>Implement the specific interaction logic here.</summary>
        protected abstract void OnInteract();

        // ── Protected Helpers ────────────────────────────────────────────────
        protected void SetInteractable(bool value) => _canInteract = value;

        /// <summary>Stable ID used by the Save system to persist state.</summary>
        public string SaveId => string.IsNullOrEmpty(_saveId) ? name : _saveId;
    }
}
