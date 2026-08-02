using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    [CreateAssetMenu(fileName = "SurvivalDifficulty", menuName = "Escape Room Framework/Survival/Difficulty Profile")]
    public sealed class SurvivalDifficultyProfile : ScriptableObject
    {
        [SerializeField] private string _difficultyId = "standard";
        [SerializeField] private string _displayName = "Standard";
        [Header("Player")]
        [SerializeField, Min(.1f)] private float _incomingDamageMultiplier = 1f;
        [SerializeField, Min(.1f)] private float _staminaDrainMultiplier = 1f;
        [SerializeField, Min(.1f)] private float _staminaRecoveryMultiplier = 1f;
        [SerializeField, Range(.1f, 1f)] private float _checkpointHealthRestore = 1f;
        [Header("Enemy")]
        [SerializeField, Min(.1f)] private float _enemySpeedMultiplier = 1f;
        [SerializeField, Min(.1f)] private float _enemySightMultiplier = 1f;
        [SerializeField, Min(.1f)] private float _enemyHearingMultiplier = 1f;
        [SerializeField, Min(.1f)] private float _enemyDamageMultiplier = 1f;
        [SerializeField, Min(.1f)] private float _enemyAttackCooldownMultiplier = 1f;
        [SerializeField, Min(.1f)] private float _hidingInspectionDelayMultiplier = 1f;
        [Header("Resources and saves")]
        [SerializeField, Min(.1f)] private float _resourceConsumptionMultiplier = 1f;
        [SerializeField] private bool _allowCheckpoints = true;
        [SerializeField] private bool _allowManualSaving = true;

        public string DifficultyId => string.IsNullOrWhiteSpace(_difficultyId) ? name : _difficultyId;
        public string DisplayName => string.IsNullOrWhiteSpace(_displayName) ? name : _displayName;
        public float IncomingDamageMultiplier => _incomingDamageMultiplier;
        public float StaminaDrainMultiplier => _staminaDrainMultiplier;
        public float StaminaRecoveryMultiplier => _staminaRecoveryMultiplier;
        public float CheckpointHealthRestore => _checkpointHealthRestore;
        public float EnemySpeedMultiplier => _enemySpeedMultiplier;
        public float EnemySightMultiplier => _enemySightMultiplier;
        public float EnemyHearingMultiplier => _enemyHearingMultiplier;
        public float EnemyDamageMultiplier => _enemyDamageMultiplier;
        public float EnemyAttackCooldownMultiplier => _enemyAttackCooldownMultiplier;
        public float HidingInspectionDelayMultiplier => _hidingInspectionDelayMultiplier;
        public float ResourceConsumptionMultiplier => _resourceConsumptionMultiplier;
        public bool AllowCheckpoints => _allowCheckpoints;
        public bool AllowManualSaving => _allowManualSaving;

#if UNITY_EDITOR
        public void Configure(string id, string displayName, float damage, float staminaDrain,
            float staminaRecovery, float checkpointHealth, float enemySpeed, float enemySight,
            float enemyHearing, float enemyDamage, float attackCooldown, float hidingInspection,
            float resourceConsumption, bool checkpoints, bool manualSaving)
        {
            _difficultyId = id;
            _displayName = displayName;
            _incomingDamageMultiplier = damage;
            _staminaDrainMultiplier = staminaDrain;
            _staminaRecoveryMultiplier = staminaRecovery;
            _checkpointHealthRestore = checkpointHealth;
            _enemySpeedMultiplier = enemySpeed;
            _enemySightMultiplier = enemySight;
            _enemyHearingMultiplier = enemyHearing;
            _enemyDamageMultiplier = enemyDamage;
            _enemyAttackCooldownMultiplier = attackCooldown;
            _hidingInspectionDelayMultiplier = hidingInspection;
            _resourceConsumptionMultiplier = resourceConsumption;
            _allowCheckpoints = checkpoints;
            _allowManualSaving = manualSaving;
        }
#endif
    }
}
