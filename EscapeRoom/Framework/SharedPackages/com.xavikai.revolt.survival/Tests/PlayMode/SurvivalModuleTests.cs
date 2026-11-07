using System.Collections.Generic;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Core.Settings;
using EscapeRoomRevolt.Systems.Inventory;
using EscapeRoomRevolt.Systems.Survival;
using NUnit.Framework;
using UnityEngine;

namespace EscapeRoomRevolt.Survival.Tests
{
    public sealed class SurvivalModuleTests
    {
        private GameGenre _genre;
        private OptionalGameFeature _features;
        private readonly List<GameObject> _owned = new();
        private readonly SurvivalModule _module = new();

        [SetUp]
        public void SetUp()
        {
            _genre = GameFeatures.Settings.Genre;
            _features = GameFeatures.Settings.CustomFeatures;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject owner in _owned)
                if (owner != null) Object.DestroyImmediate(owner);
            _owned.Clear();
            GameFeatures.Settings.SetCustomFeatures(_features);
            GameFeatures.Settings.SetProfile(_genre);
        }

        private GameObject Create(string name)
        {
            var owner = new GameObject(name);
            _owned.Add(owner);
            return owner;
        }

        [Test]
        public void EscapeRoom_InstallsNoSurvivalServicesOrPickupComponents()
        {
            GameFeatures.Settings.SetProfile(GameGenre.EscapeRoom);
            var player = Create("EscapeRoomPlayer");
            var pickup = Create("EscapeRoomPickup");
            _module.EnsureServices();
            _module.InitializeScene(player);
            _module.PreparePickup(pickup);
            Assert.That(player.GetComponent<PlayerVitals>(), Is.Null);
            Assert.That(player.GetComponent<TraversalController>(), Is.Null);
            Assert.That(pickup.GetComponent<CheckpointEntity>(), Is.Null);
            Assert.That(SanityController.Instance, Is.Null);
            Assert.That(CheckpointManager.Instance, Is.Null);
            Assert.That(SurvivalDifficultyService.Instance, Is.Null);
        }

        [Test]
        public void CheckpointPickup_IsRetainedAndRestorableThroughSharedContract()
        {
            GameFeatures.Settings.SetCustomFeatures(OptionalGameFeature.Checkpoints);
            var manager = Create("CheckpointManager").AddComponent<CheckpointManager>();
            var pickup = Create("Pickup");
            pickup.SetActive(false);
            pickup.AddComponent<BoxCollider>();
            pickup.AddComponent<PickableItem>();
            _module.PreparePickup(pickup);
            pickup.SetActive(true);
            var entity = pickup.GetComponent<CheckpointEntity>();
            Assert.That(entity, Is.Not.Null);
            CheckpointEntityState state = entity.Capture();
            Assert.That(((IPickupRemovalHandler)entity).TryRemoveFromWorld(), Is.True);
            Assert.That(pickup.activeSelf, Is.False);
            entity.Restore(state);
            Assert.That(pickup.activeSelf, Is.True);
            Assert.That(manager, Is.Not.Null);
        }

        [Test]
        public void CheckpointEntity_WithoutManager_DoesNotOverrideNormalPickupRemoval()
        {
            GameFeatures.Settings.SetCustomFeatures(OptionalGameFeature.Checkpoints);
            var pickup = Create("Pickup");
            var entity = pickup.AddComponent<CheckpointEntity>();
            Assert.That(((IPickupRemovalHandler)entity).TryRemoveFromWorld(), Is.False);
            Assert.That(pickup.activeSelf, Is.True);
        }

        [Test]
        public void PuzzleStress_ReachesSanityOnlyWhenFeatureIsEnabled()
        {
            GameFeatures.Settings.SetCustomFeatures(OptionalGameFeature.Sanity);
            var sanity = Create("Sanity").AddComponent<SanityController>();
            GameModules.ApplyPuzzleStress(12f);
            Assert.That(sanity.Value, Is.EqualTo(88f));
            GameFeatures.Settings.SetProfile(GameGenre.EscapeRoom);
            GameModules.ApplyPuzzleStress(12f);
            Assert.That(sanity.Value, Is.EqualTo(88f));
        }
    }
}
