using EscapeRoomRevolt.Core;
using NUnit.Framework;

namespace EscapeRoomRevolt.Core.Tests
{
    public sealed class GameModulesTests
    {
        private sealed class RecordingModule : GameModule
        {
            public int Initializations;
            public float Stress;
            public override void EnsureServices() => Initializations++;
            public override void ApplyPuzzleStress(float amount) => Stress += amount;
        }

        [Test]
        public void RepeatedRegistration_DoesNotDuplicateCallbacks_AndUnregisterStopsThem()
        {
            var module = new RecordingModule();
            try
            {
                GameModules.Register(module);
                GameModules.Register(module);
                GameModules.EnsureServices();
                Assert.That(module.Initializations, Is.EqualTo(1));
                GameModules.Unregister(module);
                GameModules.EnsureServices();
                Assert.That(module.Initializations, Is.EqualTo(1));
            }
            finally { GameModules.Unregister(module); }
        }

        [Test]
        public void PuzzleStress_IgnoresNonPositiveAmounts_AndDispatchesToOptionalModule()
        {
            var module = new RecordingModule();
            try
            {
                GameModules.Register(module);
                GameModules.ApplyPuzzleStress(-2f);
                GameModules.ApplyPuzzleStress(0f);
                GameModules.ApplyPuzzleStress(7f);
                Assert.That(module.Stress, Is.EqualTo(7f));
            }
            finally { GameModules.Unregister(module); }
        }

        [Test]
        public void MissingDifficulty_UsesNeutralFlashlightConsumption()
        {
            IGameDifficultyProvider previous = GameModules.Difficulty;
            try
            {
                GameModules.Difficulty = null;
                Assert.That(GameModules.FlashlightConsumptionMultiplier, Is.EqualTo(1f));
            }
            finally { GameModules.Difficulty = previous; }
        }
    }
}
