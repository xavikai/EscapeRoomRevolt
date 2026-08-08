using UnityEngine;

namespace EscapeRoomRevolt.Systems.Interaction
{
    /// <summary>
    /// Makes a SteppedPositioner player-operable: each interact advances it one position. The
    /// multi-position counterpart to InteractableToggle, which stays deliberately binary for plain
    /// on-off switches.
    /// </summary>
    [RequireComponent(typeof(SteppedPositioner))]
    public class InteractableCycler : InteractableBase
    {
        private SteppedPositioner _positioner;

        public SteppedPositioner Positioner
        {
            get
            {
                if (_positioner == null) _positioner = GetComponent<SteppedPositioner>();
                return _positioner;
            }
        }

        public int CurrentIndex => Positioner.CurrentIndex;

        public override string InteractionPrompt => Positioner.CurrentPrompt;

        protected override void Awake()
        {
            base.Awake();
            _positioner = GetComponent<SteppedPositioner>();
        }

        protected override void OnInteract() => Positioner.Advance();
    }
}
