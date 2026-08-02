using System;
using UnityEngine;

namespace EscapeRoomRevolt.Player
{
    public enum PlayerPlatform { Desktop, VirtualReality }
    public enum PlayerHand { Left, Right }

    public interface IPlayerPlatformAdapter
    {
        PlayerPlatform Platform { get; }
        Transform Head { get; }
        Transform GetHand(PlayerHand hand);
        bool SupportsHaptics { get; }
        void SendHaptic(PlayerHand hand, float amplitude, float duration);
    }

    public static class PlayerPlatformRegistry
    {
        public static IPlayerPlatformAdapter Current { get; private set; }
        public static event Action<IPlayerPlatformAdapter> Changed;

        public static void Register(IPlayerPlatformAdapter adapter)
        {
            if (adapter == null || ReferenceEquals(Current, adapter)) return;
            Current = adapter;
            Changed?.Invoke(Current);
        }

        public static void Unregister(IPlayerPlatformAdapter adapter)
        {
            if (!ReferenceEquals(Current, adapter)) return;
            Current = null;
            Changed?.Invoke(null);
        }
    }
}
