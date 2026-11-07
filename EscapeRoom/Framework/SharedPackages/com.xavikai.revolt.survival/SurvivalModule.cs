using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Core.Settings;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    /// <summary>Survival owns its installation; shared Bootstrap and UI only call module hooks.</summary>
    public sealed class SurvivalModule : GameModule
    {
        private static readonly SurvivalModule Module = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register() => GameModules.Register(Module);

        public override void EnsureServices()
        {
            if (GameFeatures.IsEnabled(OptionalGameFeature.PlayerVitals) && SurvivalDifficultyService.Instance == null)
                new GameObject("SurvivalDifficultyService").AddComponent<SurvivalDifficultyService>();
        }

        public override void InitializeScene(GameObject playerRoot)
        {
            EnsureServices();
            Add<PlayerVitals>(playerRoot, OptionalGameFeature.PlayerVitals);
            Add<PlayerDamageFeedbackRelay>(playerRoot, OptionalGameFeature.PlayerVitals);
            Add<TraversalController>(playerRoot, OptionalGameFeature.Traversal);
            Add<EvasionController>(playerRoot, OptionalGameFeature.AdvancedEvasion);
            Ensure<EvidenceJournal>(OptionalGameFeature.EvidenceRecording);
            Add<PlayerVisibility>(playerRoot, OptionalGameFeature.EnemyAI);
            Ensure<ChaseDirector>(OptionalGameFeature.EnemyAI);
            Ensure<CheckpointManager>(OptionalGameFeature.Checkpoints);
            Ensure<SanityController>(OptionalGameFeature.Sanity);
            Add<SanityFeedbackController>(playerRoot, OptionalGameFeature.Sanity);
            Add<HidingViewFeedback>(playerRoot, OptionalGameFeature.Hiding);
        }

        public override void PreparePickup(GameObject pickup)
        {
            if (pickup != null && GameFeatures.IsEnabled(OptionalGameFeature.Checkpoints)
                && pickup.GetComponent<CheckpointEntity>() == null)
                pickup.AddComponent<CheckpointEntity>();
        }

        public override void ApplyPuzzleStress(float amount)
        {
            if (GameFeatures.IsEnabled(OptionalGameFeature.Sanity))
                SanityController.Instance?.ApplyStress(amount);
        }

        private static void Add<T>(GameObject player, OptionalGameFeature feature) where T : Component
        {
            if (player != null && GameFeatures.IsEnabled(feature) && player.GetComponent<T>() == null)
                player.AddComponent<T>();
        }

        private static void Ensure<T>(OptionalGameFeature feature) where T : Component
        {
            if (GameFeatures.IsEnabled(feature) && Object.FindAnyObjectByType<T>() == null)
                new GameObject(typeof(T).Name).AddComponent<T>();
        }
    }
}
