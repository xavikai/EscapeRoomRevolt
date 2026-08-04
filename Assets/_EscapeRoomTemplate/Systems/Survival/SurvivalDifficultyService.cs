using System;
using EscapeRoomRevolt.Core.Save;
using EscapeRoomRevolt.Core.Settings;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    [Serializable]
    public sealed class SurvivalDifficultySaveState { public string difficultyId; }

    [DefaultExecutionOrder(-48)]
    public sealed class SurvivalDifficultyService : MonoBehaviour, ISaveable
    {
        private const string ResourcePath = "SurvivalDifficultySettings";
        private const string PreferenceKey = "EscapeRoomRevolt.SurvivalDifficulty";
        public static SurvivalDifficultyService Instance { get; private set; }

        [SerializeField] private SurvivalDifficultySettings _settings;
        private SurvivalDifficultyProfile _activeProfile;

        public SurvivalDifficultyProfile ActiveProfile => _activeProfile;
        public SurvivalDifficultyProfile[] AvailableProfiles => _settings != null
            ? _settings.Profiles
            : Array.Empty<SurvivalDifficultyProfile>();
        public string ActiveId => _activeProfile != null ? _activeProfile.DifficultyId : "standard";
        public event Action<SurvivalDifficultyProfile> DifficultyChanged;

        private void Awake()
        {
            if (!GameFeatures.IsEnabled(OptionalGameFeature.PlayerVitals)) { enabled = false; return; }
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SaveManager.ManualSaveGate = () => AllowsManualSaving;
            if (_settings == null) _settings = Resources.Load<SurvivalDifficultySettings>(ResourcePath);
            string preferredId = PlayerPrefs.GetString(PreferenceKey, string.Empty);
            _activeProfile = _settings != null ? _settings.Find(preferredId) ?? _settings.DefaultProfile : null;
            if (_activeProfile == null)
                Debug.LogWarning("[Difficulty] No SurvivalDifficultySettings/default profile found. Neutral multipliers will be used.");
            SaveManager.Instance?.Register(this);
        }

        private void OnDestroy()
        {
            SaveManager.Instance?.Unregister(this);
            if (Instance == this) Instance = null;
        }

        public bool SetDifficulty(string difficultyId)
        {
            SurvivalDifficultyProfile profile = _settings != null ? _settings.Find(difficultyId) : null;
            if (profile == null) return false;
            if (_activeProfile == profile) return true;
            _activeProfile = profile;
            PlayerPrefs.SetString(PreferenceKey, profile.DifficultyId);
            DifficultyChanged?.Invoke(profile);
            return true;
        }

        public string SaveId => "SurvivalDifficulty";
        public string SaveData() => JsonUtility.ToJson(new SurvivalDifficultySaveState { difficultyId = ActiveId });

        public void LoadData(string json)
        {
            SurvivalDifficultySaveState state = JsonUtility.FromJson<SurvivalDifficultySaveState>(json);
            if (state != null) SetDifficulty(state.difficultyId);
        }

        private static SurvivalDifficultyProfile Profile => Instance != null ? Instance.ActiveProfile : null;
        public static float IncomingDamage => Profile != null ? Profile.IncomingDamageMultiplier : 1f;
        public static float StaminaDrain => Profile != null ? Profile.StaminaDrainMultiplier : 1f;
        public static float StaminaRecovery => Profile != null ? Profile.StaminaRecoveryMultiplier : 1f;
        public static float CheckpointHealth => Profile != null ? Profile.CheckpointHealthRestore : 1f;
        public static float EnemySpeed => Profile != null ? Profile.EnemySpeedMultiplier : 1f;
        public static float EnemySight => Profile != null ? Profile.EnemySightMultiplier : 1f;
        public static float EnemyHearing => Profile != null ? Profile.EnemyHearingMultiplier : 1f;
        public static float EnemyDamage => Profile != null ? Profile.EnemyDamageMultiplier : 1f;
        public static float EnemyAttackCooldown => Profile != null ? Profile.EnemyAttackCooldownMultiplier : 1f;
        public static float HidingInspectionDelay => Profile != null ? Profile.HidingInspectionDelayMultiplier : 1f;
        public static float FlashlightConsumption => Profile != null ? Profile.FlashlightConsumptionMultiplier : 1f;
        public static float CamcorderConsumption => Profile != null ? Profile.CamcorderConsumptionMultiplier : 1f;
        public static bool AllowsCheckpoints => Profile == null || Profile.AllowCheckpoints;
        public static bool AllowsManualSaving => Profile == null || Profile.AllowManualSaving;
    }
}
