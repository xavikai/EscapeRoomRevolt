using UnityEngine;

namespace EscapeRoomRevolt.Player
{
    public abstract class PlayerPlatformAdapterBase : MonoBehaviour, IPlayerPlatformAdapter
    {
        public abstract PlayerPlatform Platform { get; }
        public abstract Transform Head { get; }
        public abstract Transform GetHand(PlayerHand hand);
        public abstract bool SupportsHaptics { get; }
        public abstract void SendHaptic(PlayerHand hand, float amplitude, float duration);
        public abstract void SetMovementBlocked(bool blocked);
        public abstract void SetLookBlocked(bool blocked);
        public abstract void TeleportTo(Vector3 position, Quaternion rotation);

        protected virtual void OnEnable() => PlayerPlatformRegistry.Register(this);
        protected virtual void OnDisable() => PlayerPlatformRegistry.Unregister(this);
    }
}
