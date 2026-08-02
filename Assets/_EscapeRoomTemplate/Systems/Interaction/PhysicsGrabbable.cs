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

        public void OnDropped()
        {
            // Optional: Actions when the item is released
        }

        public void OnThrown() => _impactNoise?.NotifyThrown();
    }
}
