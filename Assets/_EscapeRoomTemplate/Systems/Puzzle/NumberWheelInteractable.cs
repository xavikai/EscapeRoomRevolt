using EscapeRoomRevolt.Core.Input;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Player;
using EscapeRoomRevolt.Systems.Interaction;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>
    /// Interaction adapter for a decimal wheel. On PC the player must first enter the puzzle focus,
    /// hover the desired wheel and use the vertical movement axis (W/S by default) to choose the
    /// rotation direction. In VR a direct interaction still advances the wheel so the same authored
    /// puzzle remains usable without forcing a detached camera.
    /// </summary>
    [RequireComponent(typeof(SteppedPositioner))]
    public sealed class NumberWheelInteractable : InteractableBase
    {
        [SerializeField] private PuzzleFocusPoint _focusPoint;
        [SerializeField, Range(.1f, 1f)] private float _axisThreshold = .5f;

        private SteppedPositioner _positioner;
        private bool _verticalWasHeld;

        private SteppedPositioner Positioner
        {
            get
            {
                if (_positioner == null) _positioner = GetComponent<SteppedPositioner>();
                return _positioner;
            }
        }

        private static bool IsVirtualReality =>
            PlayerPlatformRegistry.Current != null &&
            PlayerPlatformRegistry.Current.Platform == PlayerPlatform.VirtualReality;

        public override bool CanInteract => base.CanInteract &&
            (IsVirtualReality || (_focusPoint != null && _focusPoint.IsFocused));

        public override string InteractionPrompt => IsVirtualReality
            ? "Girar rodet"
            : "W / S · pujar / baixar";

        protected override void Awake()
        {
            base.Awake();
            _positioner = GetComponent<SteppedPositioner>();
        }

        private void Update()
        {
            bool isHovered = InteractionManager.Instance != null &&
                object.ReferenceEquals(InteractionManager.Instance.CurrentTarget, this);
            if (!CanInteract || IsVirtualReality || !isHovered)
            {
                _verticalWasHeld = false;
                return;
            }

            InputRouter input = InputRouter.Instance;
            float vertical = input != null ? input.Move.y : 0f;
            if (Mathf.Abs(vertical) < _axisThreshold)
            {
                _verticalWasHeld = false;
                return;
            }

            if (_verticalWasHeld) return;
            _verticalWasHeld = true;
            TryStep(vertical > 0f ? 1 : -1);
        }

        /// <summary>Steps the wheel only while the focused puzzle owns PC input (or from direct VR interaction).</summary>
        public bool TryStep(int direction)
        {
            if (!CanInteract || direction == 0) return false;
            Positioner.Step(direction > 0 ? 1 : -1);
            return true;
        }

        protected override void OnInteract()
        {
            if (IsVirtualReality) TryStep(1);
        }

        public override void OnFocusEnter()
        {
            if (CanInteract) base.OnFocusEnter();
        }

        public override void OnFocusExit()
        {
            _verticalWasHeld = false;
            base.OnFocusExit();
        }
    }
}
