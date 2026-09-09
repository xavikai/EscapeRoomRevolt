using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Player;
using EscapeRoomRevolt.Systems.Interaction;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>
    /// Interaction adapter for a decimal wheel. The physical Up/Down buttons on the panel and the
    /// generated VR controls both call TryStep, so changing a digit follows one path on every platform.
    /// </summary>
    [RequireComponent(typeof(SteppedPositioner))]
    public sealed class NumberWheelInteractable : InteractableBase
    {
        [SerializeField] private PuzzleFocusPoint _focusPoint;
        private SteppedPositioner _positioner;

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

        public bool CanUseStepButtons => base.CanInteract &&
            (IsVirtualReality || (_focusPoint != null && _focusPoint.IsFocused));

        public int CurrentDigit
        {
            get
            {
                NumberWheelView view = GetComponent<NumberWheelView>();
                return view != null ? view.CurrentDigit : Positioner.CurrentIndex;
            }
        }

        public override string InteractionPrompt => IsVirtualReality
            ? "Girar rodet"
            : "Utilitza els botons ▲ / ▼";

        protected override void Awake()
        {
            base.Awake();
            _positioner = GetComponent<SteppedPositioner>();
        }

        /// <summary>Steps the wheel from a panel button on PC or a direct/generated VR button.</summary>
        public bool TryStep(int direction)
        {
            if (direction == 0) return false;
            if (IsVirtualReality)
            {
                if (!base.CanInteract) return false;
            }
            else if (!CanInteract) return false;
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
            base.OnFocusExit();
        }
    }
}
