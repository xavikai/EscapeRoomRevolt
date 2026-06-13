using UnityEngine;
using EscapeRoomRevolt.Core;

namespace EscapeRoomRevolt.Systems.Interaction
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class PhysicsGrabbable : InteractableBase
    {
        private Rigidbody _rb;

        protected override void Awake()
        {
            base.Awake();
            _rb = GetComponent<Rigidbody>();
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
    }
}
