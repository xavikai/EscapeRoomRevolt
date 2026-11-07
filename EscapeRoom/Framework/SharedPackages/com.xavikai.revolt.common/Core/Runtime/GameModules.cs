using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomRevolt.Core
{
    /// <summary>Optional composition hooks. Shared gameplay never references a product assembly.</summary>
    public abstract class GameModule
    {
        public virtual void EnsureServices() { }
        public virtual void InitializeScene(GameObject playerRoot) { }
        public virtual void PreparePickup(GameObject pickup) { }
        public virtual void ApplyPuzzleStress(float amount) { }
    }

    public readonly struct DifficultyOption
    {
        public readonly string Id;
        public readonly string DisplayName;
        public DifficultyOption(string id, string displayName) { Id = id; DisplayName = displayName; }
    }

    /// <summary>Product-owned difficulty exposed without product-specific ScriptableObjects.</summary>
    public interface IGameDifficultyProvider
    {
        IReadOnlyList<DifficultyOption> Options { get; }
        string ActiveId { get; }
        bool SetDifficulty(string id);
        float FlashlightConsumptionMultiplier { get; }
    }

    /// <summary>Lets an optional world-state system retain an item for later restoration.</summary>
    public interface IPickupRemovalHandler
    {
        bool TryRemoveFromWorld();
    }

    public static class GameModules
    {
        private static readonly List<GameModule> Modules = new();
        public static IGameDifficultyProvider Difficulty { get; set; }
        public static float FlashlightConsumptionMultiplier => Difficulty?.FlashlightConsumptionMultiplier ?? 1f;

        // Modules register in BeforeSceneLoad, after every SubsystemRegistration reset.
        public static void Register(GameModule module)
        {
            if (module != null && !Modules.Contains(module)) Modules.Add(module);
        }

        public static void Unregister(GameModule module) => Modules.Remove(module);

        public static void EnsureServices()
        {
            foreach (GameModule module in Modules.ToArray()) module.EnsureServices();
        }

        public static void InitializeScene(GameObject playerRoot)
        {
            foreach (GameModule module in Modules.ToArray()) module.InitializeScene(playerRoot);
        }

        public static void PreparePickup(GameObject pickup)
        {
            foreach (GameModule module in Modules.ToArray()) module.PreparePickup(pickup);
        }

        public static void ApplyPuzzleStress(float amount)
        {
            if (amount <= 0f) return;
            foreach (GameModule module in Modules.ToArray()) module.ApplyPuzzleStress(amount);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Modules.Clear();
            Difficulty = null;
        }
    }
}
