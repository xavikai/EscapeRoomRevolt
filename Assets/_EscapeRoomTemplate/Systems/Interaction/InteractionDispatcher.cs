using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Player;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Interaction
{
    /// <summary>Platform-neutral interaction execution shared by desktop rays and XR interactors.</summary>
    public static class InteractionDispatcher
    {
        public static bool TryPerform(IInteractable target, PlayerHand hapticHand = PlayerHand.Right)
        {
            if (!target.IsAlive() || !target.CanInteract) return false;
            MonoBehaviour behaviour = target as MonoBehaviour;
            string targetId = behaviour != null ? behaviour.name : "Unknown";
            GameObject targetObject = behaviour != null ? behaviour.gameObject : null;

            target.Interact();
            EventBus.Publish(new OnInteractionPerformed { interactableId = targetId, target = targetObject });
            IPlayerPlatformAdapter platform = PlayerPlatformRegistry.Current;
            if (platform != null && platform.SupportsHaptics) platform.SendHaptic(hapticHand, .25f, .06f);
            return true;
        }
    }
}
