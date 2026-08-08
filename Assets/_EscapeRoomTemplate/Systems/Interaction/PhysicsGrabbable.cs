using UnityEngine;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Systems.Survival;

namespace EscapeRoomRevolt.Systems.Interaction
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(GameplayImpactNoiseEmitter))]
    public class PhysicsGrabbable : InteractableBase
    {
        [Tooltip("Whether the player can hurl this. Turn it off for anything a puzzle needs back: a "
               + "thrown piece can end up somewhere unreachable and leave the puzzle unsolvable.")]
        [SerializeField] private bool _canBeThrown = true;

        public bool CanBeThrown => _canBeThrown;

        private Rigidbody _rb;
        private GameplayImpactNoiseEmitter _impactNoise;

        protected override void Awake()
        {
            base.Awake();
            _rb = GetComponent<Rigidbody>();
            _impactNoise = GetComponent<GameplayImpactNoiseEmitter>();
        }

        protected override void OnInteract()
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

        /// <summary>
        /// Turns grabbing on or off at runtime. Used when a prop stops being a loose object and
        /// becomes part of the scenery — a fuse seated in its holder once the circuit is live.
        /// </summary>
        public void SetGrabbable(bool value) => SetInteractable(value);

        public void OnDropped()
        {
            // Optional: Actions when the item is released
        }

        public void OnThrown() => _impactNoise?.NotifyThrown();
    }
}
