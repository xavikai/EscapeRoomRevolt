using UnityEngine;
using EscapeRoomRevolt.Core.Save;

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
    public abstract class InteractableBase : MonoBehaviour, IInteractable, ISaveable
    {
        [Header("Interaction Settings")]
        [SerializeField] private string _interactionPrompt = "Interact";
        [SerializeField] private bool _canInteract = true;
        [SerializeField] private string _saveId = "";

        [SerializeField] private CursorType _cursorType = CursorType.Hand;

        [Header("Visual Feedback")]
        [SerializeField] private bool _enableOutline = true;
        private SelectionOutlineTarget _outlineTarget;

        // ── Unity Lifecycle ──────────────────────────────────────────────────
        protected virtual void Awake()
        {
            _outlineTarget = GetComponent<SelectionOutlineTarget>();
            if (_outlineTarget == null) _outlineTarget = gameObject.AddComponent<SelectionOutlineTarget>();
        }

        protected virtual void Start()
        {
            int interactableLayer = LayerMask.NameToLayer("Interactable");
            if (interactableLayer >= 0) gameObject.layer = interactableLayer;
            
            SaveManager.Instance?.Register(this);
        }

        protected virtual void OnDestroy()
        {
            SaveManager.Instance?.Unregister(this);
        }

        // ── IInteractable ────────────────────────────────────────────────────
        public virtual string InteractionPrompt => _interactionPrompt;
        public virtual CursorType InteractionCursor => _cursorType;
        public virtual bool CanInteract => this != null && _canInteract && gameObject.activeInHierarchy;

        public void Interact()
        {
            if (!CanInteract) return;
            OnInteract();
        }

        public virtual void OnFocusEnter()
        {
            if (_enableOutline) _outlineTarget?.SetHighlighted(true);
        }

        public virtual void OnFocusExit()
        {
            if (_enableOutline) _outlineTarget?.SetHighlighted(false);
        }

        // ── Abstract / Virtual ───────────────────────────────────────────────
        /// <summary>Implement the specific interaction logic here.</summary>
        protected abstract void OnInteract();

        // ── Protected Helpers ────────────────────────────────────────────────
        protected void SetInteractable(bool value) => _canInteract = value;

        /// <summary>Stable ID used by the Save system to persist state.</summary>
        public string SaveId => string.IsNullOrEmpty(_saveId) ? name : _saveId;

        public virtual string SaveData()
        {
            return "{}"; // Override in derived classes
        }

        public virtual void LoadData(string json)
        {
            // Override in derived classes
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(_saveId))
            {
                // Only generate for instances in the scene, not raw prefabs in the project
                if (!UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this) && !Application.isPlaying)
                {
                    _saveId = System.Guid.NewGuid().ToString();
                    UnityEditor.EditorUtility.SetDirty(this);
                }
            }
        }
#endif
    }
}
