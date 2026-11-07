using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    [CreateAssetMenu(fileName = "HorrorEnemyProfile", menuName = "Escape Room Framework/Survival/Horror Enemy Profile")]
    public sealed class HorrorEnemyProfile : ScriptableObject
    {
        [Header("Movement")]
        [Min(0f)] public float patrolSpeed = 1.8f;
        [Min(0f)] public float investigateSpeed = 2.7f;
        [Min(0f)] public float chaseSpeed = 4.8f;
        [Header("Perception")]
        [Min(0f)] public float sightRange = 14f;
        [Range(1f, 179f)] public float sightAngle = 85f;
        [Min(0f)] public float hearingMultiplier = 1f;
        [Min(.02f)] public float perceptionInterval = .12f;
        [Min(.01f)] public float detectionSeconds = .35f;
        [Min(0f)] public float awarenessDecayPerSecond = .7f;
        [Min(0f)] public float instantDetectionRange = 2.2f;
        public bool useVisibilityModifiers = true;
        [Min(0f)] public float chaseMemory = 4f;
        [Min(0f)] public float searchDuration = 7f;
        [Header("Investigation")]
        public bool inspectHidingSpots = true;
        [Min(0f)] public float hidingInspectionDelay = 1.1f;
        [Header("Doors")]
        public bool operateDoors = true;
        public bool forceLockedDoors = false;
        [Tooltip("When enabled, an enemy in Chase uses the loud, fast slam operation instead of normal opening.")]
        public bool slamDoorsDuringChase = true;
        [Min(.2f)] public float doorInteractionDistance = 1.35f;
        [Min(.1f)] public float doorInteractionCooldown = .8f;
        [Header("Attack")]
        [Min(0f)] public float attackRange = 1.45f;
        [Min(0f)] public float attackDamage = 45f;
        [Min(.1f)] public float attackCooldown = 1.4f;
    }
}
