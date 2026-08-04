using System;
using EscapeRoomRevolt.Core.Settings;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    public enum GameplayNoiseType { Footstep, Sprint, Door, DoorCareful, DoorSlam, Impact, PlayerAction }

    public readonly struct GameplayNoiseStimulus
    {
        public readonly Vector3 Position;
        public readonly float Radius;
        public readonly GameplayNoiseType Type;
        public readonly GameObject Source;
        public readonly float Time;

        public GameplayNoiseStimulus(Vector3 position, float radius, GameplayNoiseType type, GameObject source)
        {
            Position = position;
            Radius = radius;
            Type = type;
            Source = source;
            Time = UnityEngine.Time.time;
        }
    }

    /// <summary>Decoupled gameplay-noise bus. It is unrelated to mixer volume or audible clips.</summary>
    public static class GameplayNoise
    {
        public static event Action<GameplayNoiseStimulus> Emitted;

        public static void Emit(Vector3 position, float radius, GameplayNoiseType type, GameObject source = null)
        {
            if (radius <= 0f || !GameFeatures.IsEnabled(OptionalGameFeature.EnemyAI)) return;
            Emitted?.Invoke(new GameplayNoiseStimulus(position, radius, type, source));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Emitted = null;
    }

}
