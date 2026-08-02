using System;
using UnityEngine;

namespace EscapeRoomRevolt.Core.Settings
{
    public enum GameGenre
    {
        EscapeRoom,
        SurvivalHorror,
        CustomHybrid
    }

    [Flags]
    public enum OptionalGameFeature
    {
        None = 0,
        Flashlight = 1 << 0,
        Sanity = 1 << 1,
        HorrorEvents = 1 << 2,
        PlayerVitals = 1 << 3,
        EnemyAI = 1 << 4,
        Hiding = 1 << 5,
        NightVision = 1 << 6,
        Checkpoints = 1 << 7,
        Traversal = 1 << 8,
        EvidenceRecording = 1 << 9,
        AdvancedEvasion = 1 << 10,
        AdvancedDoors = 1 << 11
    }

    /// <summary>
    /// Project-wide genre profile. Shared escape-room systems remain available in every profile;
    /// only genre-specific runtime systems are gated here.
    /// </summary>
    [CreateAssetMenu(
        fileName = "GenreFeatureSettings",
        menuName = "Escape Room Framework/Configuration/Genre Feature Settings",
        order = 1)]
    public sealed class GenreFeatureSettings : ScriptableObject
    {
        public const OptionalGameFeature EscapeRoomFeatures = OptionalGameFeature.None;
        public const OptionalGameFeature SurvivalHorrorFeatures =
            OptionalGameFeature.Flashlight | OptionalGameFeature.Sanity | OptionalGameFeature.HorrorEvents |
            OptionalGameFeature.PlayerVitals | OptionalGameFeature.EnemyAI | OptionalGameFeature.Hiding |
            OptionalGameFeature.NightVision | OptionalGameFeature.Checkpoints | OptionalGameFeature.Traversal |
            OptionalGameFeature.EvidenceRecording | OptionalGameFeature.AdvancedEvasion |
            OptionalGameFeature.AdvancedDoors;

        [Header("Project Genre")]
        [SerializeField] private GameGenre _genre = GameGenre.EscapeRoom;

        [Header("Custom Hybrid Features")]
        [Tooltip("Used only when Project Genre is Custom Hybrid.")]
        [SerializeField] private OptionalGameFeature _customFeatures = SurvivalHorrorFeatures;

        public GameGenre Genre => _genre;
        public OptionalGameFeature CustomFeatures => _customFeatures;
        public OptionalGameFeature ActiveFeatures => _genre switch
        {
            GameGenre.EscapeRoom => EscapeRoomFeatures,
            GameGenre.SurvivalHorror => SurvivalHorrorFeatures,
            _ => _customFeatures
        };

        public bool IsEnabled(OptionalGameFeature feature) => (ActiveFeatures & feature) == feature;

        public void SetProfile(GameGenre genre)
        {
            _genre = genre;
        }

        public void SetCustomFeatures(OptionalGameFeature features)
        {
            _customFeatures = features;
            _genre = GameGenre.CustomHybrid;
        }
    }

    /// <summary>Read-only runtime access to the active genre and optional mechanics.</summary>
    public static class GameFeatures
    {
        private const string ResourcePath = "GenreFeatureSettings";
        private static GenreFeatureSettings _settings;
        private static bool _loadAttempted;

        public static GenreFeatureSettings Settings
        {
            get
            {
                if (!_loadAttempted)
                {
                    _loadAttempted = true;
                    _settings = Resources.Load<GenreFeatureSettings>(ResourcePath);
                    if (_settings == null)
                        Debug.LogWarning("[Game Features] GenreFeatureSettings was not found in Resources. Escape Room profile will be used.");
                }
                return _settings;
            }
        }

        public static GameGenre Genre => Settings != null ? Settings.Genre : GameGenre.EscapeRoom;
        public static OptionalGameFeature ActiveFeatures =>
            Settings != null ? Settings.ActiveFeatures : GenreFeatureSettings.EscapeRoomFeatures;

        public static bool IsEnabled(OptionalGameFeature feature) => (ActiveFeatures & feature) == feature;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _settings = null;
            _loadAttempted = false;
        }
    }
}
