using UnityEngine;
using EscapeRoomRevolt.Systems.Puzzle;
using System.Collections;

namespace EscapeRoomRevolt.Systems.Interaction
{
    public enum KeypadButtonAction
    {
        Digit,
        Submit,
        Clear
    }

    /// <summary>
    /// A physical 3D button that can be pressed in the world to interact with a CodePanelPuzzle.
    /// Simulates a physical press animation.
    /// </summary>
    public class KeypadButton3D : InteractableBase
    {
        [Header("Keypad Settings")]
        [SerializeField] private CodePanelPuzzle _targetPuzzle;
        [SerializeField] private KeypadButtonAction _action = KeypadButtonAction.Digit;
        [SerializeField] private string _value = "1";
        
        [Header("Animation")]
        [SerializeField] private float _pressDepth = 0.02f;
        [SerializeField] private float _pressSpeed = 10f;
        
        private Vector3 _originalLocalPos;
        private bool _isPressing = false;

        protected override void Awake()
        {
            base.Awake();
            _originalLocalPos = transform.localPosition;
        }

        protected override void OnInteract()
        {
            var keypad = GetComponentInParent<InteractableKeypad>();
            if (keypad != null && !keypad.IsFocused)
            {
                // If not zoomed in, route interaction to the keypad to enter focus mode
                keypad.Interact();
                return;
            }

            if (_targetPuzzle == null)
            {
                Debug.LogWarning($"[KeypadButton3D] No puzzle assigned to {name}!");
                return;
            }

            if (!_isPressing)
            {
                StartCoroutine(PressAnimation());
            }

            switch (_action)
            {
                case KeypadButtonAction.Digit:
                    _targetPuzzle.InputDigit(_value);
                    break;
                case KeypadButtonAction.Submit:
                    _targetPuzzle.SubmitCode();
                    break;
                case KeypadButtonAction.Clear:
                    _targetPuzzle.ClearInput();
                    break;
            }
        }

        private IEnumerator PressAnimation()
        {
            _isPressing = true;
            
            // Assume the button's local Z axis points outwards from the safe.
            // Pressing it pushes it backwards (-Z).
            Vector3 pressedPos = _originalLocalPos + Vector3.back * _pressDepth; 
            
            // Move in
            while (Vector3.Distance(transform.localPosition, pressedPos) > 0.001f)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, pressedPos, Time.deltaTime * _pressSpeed);
                yield return null;
            }
            
            // Move out
            while (Vector3.Distance(transform.localPosition, _originalLocalPos) > 0.001f)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, _originalLocalPos, Time.deltaTime * _pressSpeed);
                yield return null;
            }
            
            transform.localPosition = _originalLocalPos;
            _isPressing = false;
        }
        
        public void SetTargetPuzzle(CodePanelPuzzle puzzle) => _targetPuzzle = puzzle;
        public void SetAction(KeypadButtonAction action, string value = "")
        {
            _action = action;
            _value = value;
        }

        public override void OnFocusEnter()
        {
            var keypad = GetComponentInParent<InteractableKeypad>();
            if (keypad != null && keypad.IsFocused)
            {
                // Do not show the yellow outline if we are already zoomed in (focus mode)
                return;
            }
            base.OnFocusEnter();
        }
    }
}
