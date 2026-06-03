using UnityEngine;
using EscapeRoomRevolt.Systems.Puzzle;
using EscapeRoomRevolt.UI.PC;

namespace EscapeRoomRevolt.Systems.Interaction
{
    [RequireComponent(typeof(CodePanelPuzzle))]
    public class InteractableSafe : InteractableBase
    {
        private CodePanelPuzzle _puzzle;
        private bool _isOpen = false;

        private void Awake()
        {
            _puzzle = GetComponent<CodePanelPuzzle>();
        }

        private void Update()
        {
            if (!_isOpen && _puzzle.IsSolved)
            {
                OpenSafe();
            }
        }

        public override string InteractionPrompt => _isOpen ? "Open" : "Enter Code";

        protected override void OnInteract()
        {
            if (_isOpen) return; // Already open

            if (_puzzle.IsSolved)
            {
                OpenSafe();
            }
            else
            {
                UIManager.Instance.ShowKeypad(_puzzle);
            }
        }

        public void OpenSafe()
        {
            if (_isOpen) return;
            _isOpen = true;
            
            // Fallback visual feedback
            transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null) renderer.material.color = Color.green;
            
            // Note: We don't hide the keypad here anymore, 
            // so the user has 1 second to read the "OK" text.
            
            Debug.Log($"[Safe] {name} is now open!");
        }
    }
}
