using UnityEngine;
using EscapeRoomRevolt.Core;

namespace EscapeRoomRevolt.Systems.Interaction
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class PhysicsGrabbable : MonoBehaviour, IInteractable
    {
        [Header("Grabbable Settings")]
        [Tooltip("The text that appears when looking at this object.")]
        [SerializeField] private string _promptText = "Agafar / Grab";
        
        [Tooltip("Cursor type when hovering.")]
        [SerializeField] private CursorType _cursor = CursorType.Hand;

        private Rigidbody _rb;
        private Outline _outline;

        public bool CanInteract => true;
        public CursorType InteractionCursor => _cursor;
        public string InteractionPrompt => _promptText;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            
            // Ensure we have an outline
            _outline = GetComponent<Outline>();
            if (_outline == null)
            {
                _outline = gameObject.AddComponent<Outline>();
                _outline.enabled = false;
                _outline.OutlineMode = Outline.Mode.OutlineAll;
                _outline.OutlineColor = Color.white;
                _outline.OutlineWidth = 3f;
            }
        }

        public void Interact()
        {
            if (PhysicsGrabber.Instance != null)
            {
                PhysicsGrabber.Instance.Grab(this);
            }
            else
            {
                Debug.LogWarning("[PhysicsGrabbable] No PhysicsGrabber found in scene. Make sure the Player has one.");
            }
        }

        public void OnFocusEnter()
        {
            if (_outline != null)
                _outline.enabled = true;
        }

        public void OnFocusExit()
        {
            if (_outline != null)
                _outline.enabled = false;
        }

        public void OnDropped()
        {
            // Optional: Actions when the item is released
        }
    }
}
