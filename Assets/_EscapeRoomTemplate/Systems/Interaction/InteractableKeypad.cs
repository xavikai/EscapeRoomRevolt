using UnityEngine;
using EscapeRoomRevolt.Systems.Puzzle;
using EscapeRoomRevolt.UI.PC;

namespace EscapeRoomRevolt.Systems.Interaction
{
    [RequireComponent(typeof(CodePanelPuzzle))]
    public class InteractableKeypad : InteractableBase
    {
        [Header("Cameras")]
        [Tooltip("Camera pointing at the keypad. If assigned, the view will snap to this camera when interacting.")]
        [SerializeField] private Camera _focusCamera;

        private CodePanelPuzzle _puzzle;
        private bool _isFocused = false;
        
        public bool IsFocused => _isFocused;

        protected override void Awake()
        {
            base.Awake();
            _puzzle = GetComponent<CodePanelPuzzle>();
        }

        private void Update()
        {
            if (_isFocused)
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
                {
                    ExitFocusMode();
                }
            }
        }

        public override string InteractionPrompt => _puzzle.IsSolved ? "Solved" : "Use Keypad";

        protected override void OnInteract()
        {
            EnterFocusMode();
        }

        private void EnterFocusMode()
        {
            if (_isFocused) return;
            _isFocused = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (_focusCamera != null)
            {
                _focusCamera.gameObject.SetActive(true);
                _focusCamera.depth = 1; // Render over main camera
                
                // Tell InteractionManager to raycast from this new camera
                var interactionManager = FindAnyObjectByType<InteractionManager>();
                if (interactionManager != null)
                {
                    interactionManager.SetOverrideCamera(_focusCamera);
                }
            }

            Debug.Log($"[Keypad] {name} entered Focus Mode. Right-click or Esc to exit.");
        }

        private void ExitFocusMode()
        {
            if (!_isFocused) return;
            _isFocused = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (_focusCamera != null)
            {
                _focusCamera.gameObject.SetActive(false);
                
                // Restore InteractionManager
                var interactionManager = FindAnyObjectByType<InteractionManager>();
                if (interactionManager != null)
                {
                    interactionManager.ClearOverrideCamera();
                }
            }

            Debug.Log($"[Keypad] {name} exited Focus Mode.");
        }
    }
}
