using System;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    [CreateAssetMenu(fileName = "SurvivalDifficultySettings", menuName = "Escape Room Framework/Survival/Difficulty Settings")]
    public sealed class SurvivalDifficultySettings : ScriptableObject
    {
        [SerializeField] private SurvivalDifficultyProfile[] _profiles = Array.Empty<SurvivalDifficultyProfile>();
        [SerializeField] private SurvivalDifficultyProfile _defaultProfile;

        public SurvivalDifficultyProfile DefaultProfile => _defaultProfile != null
            ? _defaultProfile
            : _profiles.Length > 0 ? _profiles[0] : null;
        public SurvivalDifficultyProfile[] Profiles => _profiles;

        public SurvivalDifficultyProfile Find(string difficultyId)
        {
            if (string.IsNullOrWhiteSpace(difficultyId)) return DefaultProfile;
            foreach (SurvivalDifficultyProfile profile in _profiles)
                if (profile != null && string.Equals(profile.DifficultyId, difficultyId, StringComparison.OrdinalIgnoreCase))
                    return profile;
            return null;
        }

#if UNITY_EDITOR
        public void Configure(SurvivalDifficultyProfile[] profiles, SurvivalDifficultyProfile defaultProfile)
        {
            _profiles = profiles ?? Array.Empty<SurvivalDifficultyProfile>();
            _defaultProfile = defaultProfile;
        }
#endif
    }
}
