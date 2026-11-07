using UnityEngine;

namespace EscapeRoomRevolt.Player.VR
{
    public enum VRLocomotionMode { TeleportOnly, ContinuousOnly, Both }
    public enum VRTurnMode { Snap, Continuous }
    public enum VRTraversalMode { Animated, Instant }

    /// <summary>Designer-facing comfort profile shared by every VR rig.</summary>
    [CreateAssetMenu(menuName = "Escape Room Framework/VR/Comfort Settings", fileName = "VRComfortSettings")]
    public sealed class VRComfortSettings : ScriptableObject
    {
        [Header("Locomotion")]
        public VRLocomotionMode locomotionMode = VRLocomotionMode.Both;
        [Min(.1f)] public float continuousMoveSpeed = 1.5f;

        [Header("Turning")]
        public VRTurnMode turnMode = VRTurnMode.Snap;
        [Range(15f, 90f)] public float snapTurnAmount = 45f;
        [Range(30f, 180f)] public float continuousTurnSpeed = 75f;

        [Header("Traversal (Quest comfort)")]
        [Tooltip("Animated follows the authored motion. Instant moves to the exit anchor in one frame.")]
        public VRTraversalMode traversalMode = VRTraversalMode.Animated;
        [Range(.5f, 2f)] public float traversalDurationMultiplier = 1f;

        [Header("Evasion (Quest comfort)")]
        [Tooltip("Lean and look-back always use physical head tracking in VR. Enable this only if your game has tested artificial sliding on hardware.")]
        public bool allowArtificialSlide;
        [Range(.5f, 1.25f)] public float artificialSlideSpeedMultiplier = .75f;
    }
}
