using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Core.Flow;
using EscapeRoomRevolt.Core.Save;
using EscapeRoomRevolt.Systems.Flow;
using EscapeRoomRevolt.Systems.Interaction;
using EscapeRoomRevolt.Systems.Inventory;
using EscapeRoomRevolt.Systems.Puzzle;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EscapeRoomRevolt.Systems.Tests
{
    /// <summary>
    /// Regressions found in the September 2026 code review: state that was lost or desynchronised
    /// when a game was saved and loaded, and visuals that drifted away from puzzle logic.
    /// </summary>
    public class SaveRestoreRegressionTests
    {
        private readonly List<GameObject> _objects = new List<GameObject>();

        private GameObject Create(string name)
        {
            var root = new GameObject(name);
            _objects.Add(root);
            return root;
        }

        private static void Set(object target, string name, object value)
        {
            System.Type type = target.GetType();
            FieldInfo field = null;
            while (type != null && field == null)
            {
                field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
                type = type.BaseType;
            }
            field.SetValue(target, value);
        }

        [SetUp]
        public void SetUp() { EventBus.Clear(); GameplayBlockState.Reset(); }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (GameObject root in _objects) if (root != null) Object.Destroy(root);
            _objects.Clear();
            EventBus.Clear();
            GameplayBlockState.Reset();
            yield return null;
        }

        private SteppedPositioner CreateWheel(string name, int positions)
        {
            GameObject owner = Create(name);
            owner.SetActive(false);
            var positioner = owner.AddComponent<SteppedPositioner>();
            var list = new List<SteppedPosition>();
            for (int i = 0; i < positions; i++) list.Add(new SteppedPosition { rotation = new Vector3(i * 36f, 0f, 0f) });
            Set(positioner, "_positions", list);
            return positioner;
        }

        [UnityTest]
        public IEnumerator StatePuzzle_SavesAndRestoresPartialWheelPositions()
        {
            SteppedPositioner first = CreateWheel("WheelA", 10);
            SteppedPositioner second = CreateWheel("WheelB", 10);
            GameObject root = Create("WheelsPuzzle");
            root.SetActive(false);
            var puzzle = root.AddComponent<StatePuzzle>();
            Set(puzzle, "_conditions", new List<StateCondition>
            {
                new StateCondition { Positioner = first, RequiredIndex = 3 },
                new StateCondition { Positioner = second, RequiredIndex = 7 }
            });
            first.gameObject.SetActive(true);
            second.gameObject.SetActive(true);
            root.SetActive(true);
            yield return null;

            first.Step(3);
            string snapshot = puzzle.SaveData();
            Assert.That(puzzle.IsSolved, Is.False);

            // Simulate a fresh scene: the saved positions are applied before Start would run.
            SteppedPositioner restoredFirst = CreateWheel("RestoredWheelA", 10);
            SteppedPositioner restoredSecond = CreateWheel("RestoredWheelB", 10);
            GameObject restoredRoot = Create("RestoredPuzzle");
            restoredRoot.SetActive(false);
            var restored = restoredRoot.AddComponent<StatePuzzle>();
            Set(restored, "_conditions", new List<StateCondition>
            {
                new StateCondition { Positioner = restoredFirst, RequiredIndex = 3 },
                new StateCondition { Positioner = restoredSecond, RequiredIndex = 7 }
            });
            restored.LoadData(snapshot);
            restoredFirst.gameObject.SetActive(true);
            restoredSecond.gameObject.SetActive(true);
            restoredRoot.SetActive(true);
            yield return null; // Start runs here and must keep the restored index.

            Assert.That(restoredFirst.CurrentIndex, Is.EqualTo(3));
            Assert.That(restoredSecond.CurrentIndex, Is.Zero);
            restoredSecond.Step(7);
            Assert.That(restored.IsSolved, Is.True);
        }

        [UnityTest]
        public IEnumerator StatePuzzle_LegacySolvedSaveShowsTheAnswer()
        {
            SteppedPositioner wheel = CreateWheel("LegacyWheel", 10);
            GameObject root = Create("LegacyPuzzle");
            var puzzle = root.AddComponent<StatePuzzle>();
            Set(puzzle, "_conditions", new List<StateCondition> { new StateCondition { Positioner = wheel, RequiredIndex = 4 } });
            puzzle.LoadData("{\"stateIndex\":2}");
            wheel.gameObject.SetActive(true);
            yield return null;
            Assert.That(puzzle.IsSolved, Is.True);
            Assert.That(wheel.CurrentIndex, Is.EqualTo(4));
        }

        [UnityTest]
        public IEnumerator PipeTileButton_DoesNotTurnVisualOnceSolved()
        {
            GameObject root = Create("Pipes");
            root.SetActive(false);
            var puzzle = root.AddComponent<PipePuzzle>();
            Set(puzzle, "_tiles", new List<PipeTileDefinition>
            {
                new PipeTileDefinition { tileId = "source", row = 0, column = 0, openSides = PipeSide.East },
                new PipeTileDefinition { tileId = "sink", row = 0, column = 1, openSides = PipeSide.North, startingRotationSteps = 2 }
            });
            Set(puzzle, "_sourceTileId", "source");
            Set(puzzle, "_sinkTileId", "sink");
            SteppedPositioner visual = CreateWheel("SinkVisual", 4);
            var button = root.AddComponent<PipeTileButton>();
            Set(button, "_puzzle", puzzle);
            Set(button, "_tileId", "sink");
            Set(button, "_visualPositioner", visual);
            visual.gameObject.SetActive(true);
            root.SetActive(true);
            yield return null;

            Assert.That(visual.CurrentIndex, Is.EqualTo(2));
            button.Rotate(); // 2 → 3: West opening meets the source's East opening.
            Assert.That(puzzle.IsSolved, Is.True);
            Assert.That(visual.CurrentIndex, Is.EqualTo(3));
            button.Rotate();
            Assert.That(visual.CurrentIndex, Is.EqualTo(3), "A solved pipe must not rotate its art any more.");
        }

        [UnityTest]
        public IEnumerator SingleUseTrigger_LoadKeepsObjectAndStaysSpent()
        {
            GameObject root = Create("SingleUseButton");
            root.AddComponent<BoxCollider>();
            var trigger = root.AddComponent<InteractableTrigger>();
            Set(trigger, "_singleUse", true);
            int fired = 0;
            trigger.OnInteractEvent = new UnityEngine.Events.UnityEvent();
            trigger.OnInteractEvent.AddListener(() => fired++);
            yield return null;

            trigger.Interact();
            string snapshot = trigger.SaveData();

            GameObject restoredRoot = Create("RestoredSingleUseButton");
            restoredRoot.AddComponent<BoxCollider>();
            var restored = restoredRoot.AddComponent<InteractableTrigger>();
            Set(restored, "_singleUse", true);
            restored.LoadData(snapshot);
            yield return null;

            Assert.That(fired, Is.EqualTo(1));
            Assert.That(restoredRoot != null, Is.True);
            Assert.That(restored.CanInteract, Is.False);
        }

        [Test]
        public void PlacementPuzzle_DoesNotRestoreLoosePartialPlacements()
        {
            GameObject root = Create("Placement");
            var puzzle = root.AddComponent<PlacementPuzzle>();
            Set(puzzle, "_rules", new List<PlacementRule>
            {
                new PlacementRule { pieceId = "a", correctSocketId = "1" },
                new PlacementRule { pieceId = "b", correctSocketId = "2" }
            });
            puzzle.Connect("a", "1");
            string partial = puzzle.SaveData();
            puzzle.ResetPuzzle();
            puzzle.LoadData(partial);
            Assert.That(puzzle.PlacedCount, Is.Zero);
        }

        private const string ClosedDoorSave = "{\"version\":2,\"isLocked\":true,\"isOpen\":false,\"openAmount\":0,\"openDirection\":1}";
        private const string AjarDoorSave = "{\"version\":2,\"isLocked\":false,\"isOpen\":false,\"openAmount\":0.5,\"openDirection\":1}";

        // Scene loads restore saves after Awake but before Start. A door used to capture its hinge in
        // Start, so a loaded door, drawer or lid jumped to the centre of the scene.
        [UnityTest]
        public IEnumerator Door_RestoredBeforeStart_KeepsItsPlace()
        {
            GameObject root = Create("Drawer");
            root.transform.SetPositionAndRotation(new Vector3(4f, 1f, -3f), Quaternion.Euler(0f, 30f, 0f));
            var door = root.AddComponent<Door>();   // Awake runs now, Start on the next frame

            door.LoadData(ClosedDoorSave);
            Assert.That(Vector3.Distance(root.transform.position, new Vector3(4f, 1f, -3f)), Is.LessThan(.001f));
            yield return null;

            Assert.That(Vector3.Distance(root.transform.position, new Vector3(4f, 1f, -3f)), Is.LessThan(.001f));
            Assert.That(Quaternion.Angle(root.transform.rotation, Quaternion.Euler(0f, 30f, 0f)), Is.LessThan(.01f));
            Assert.That(door.IsLocked, Is.True);
        }

        [UnityTest]
        public IEnumerator Door_RestoredWhileInactive_KeepsPlaceAndAjarAmount()
        {
            GameObject root = Create("Lid");
            root.SetActive(false);                  // Awake has not run when the save arrives
            root.transform.position = new Vector3(-2f, .5f, 6f);
            var door = root.AddComponent<Door>();
            Set(door, "_movementType", DoorMovementType.Slide);
            Set(door, "_slideOffset", new Vector3(1f, 0f, 0f));

            door.LoadData(AjarDoorSave);
            root.SetActive(true);
            yield return null;

            Assert.That(Vector3.Distance(root.transform.position, new Vector3(-1.5f, .5f, 6f)), Is.LessThan(.001f));
            Assert.That(door.OpenAmount, Is.EqualTo(.5f).Within(.001f));
        }

        // An auto-start timer the player had stopped used to start again after loading.
        [UnityTest]
        public IEnumerator GameOverTimer_RestoredStopped_DoesNotAutoStartAgain()
        {
            GameObject root = Create("Timer");
            root.SetActive(false);
            var timer = root.AddComponent<GameOverTimer>();
            Set(timer, "_autoStart", true);
            Set(timer, "_duration", 60f);
            timer.LoadData("{\"elapsed\":10,\"isRunning\":false,\"hasExpired\":false}");
            root.SetActive(true);
            yield return null;

            Assert.That(timer.IsRunning, Is.False);
            Assert.That(timer.TimeRemaining, Is.EqualTo(50f).Within(.01f));
        }

        // A one-shot zone fired again after loading (restarting timers, reopening doors...).
        [Test]
        public void EventTriggerZone_RestoresHasTriggered()
        {
            GameObject root = Create("Zone");
            root.AddComponent<BoxCollider>();
            var zone = root.AddComponent<EventTriggerZone>();
            zone.LoadData("{\"hasTriggered\":true}");

            Assert.That(zone.HasTriggered, Is.True);
            StringAssert.Contains("\"hasTriggered\":true", zone.SaveData());
        }

        // A note set to disappear after reading came back after loading.
        [Test]
        public void InteractableNote_ReadAndDisappearing_StaysHiddenAfterLoad()
        {
            GameObject root = Create("Note");
            var note = root.AddComponent<InteractableNote>();
            note.DisappearAfterRead = true;
            note.LoadData("{\"hasBeenRead\":true}");

            Assert.That(root.activeSelf, Is.False);
            Assert.That(note.HasBeenRead, Is.True);
        }

        // New game / Retry kept the hotspots of the previous run revealed and stopped saving them.
        [Test]
        public void ExamineHotspotRegistry_NewSession_ClearsAndKeepsSaving()
        {
            SaveManager save = SaveManager.Instance != null ? SaveManager.Instance : Create("SaveManager").AddComponent<SaveManager>();
            ExamineHotspotRegistry registry = ExamineHotspotRegistry.Instance != null
                ? ExamineHotspotRegistry.Instance : Create("Hotspots").AddComponent<ExamineHotspotRegistry>();
            save.Register(registry);
            registry.MarkRevealed("drawer_key");

            save.ResetSession();

            Assert.That(registry.IsRevealed("drawer_key"), Is.False);
            Assert.That(save.CaptureSnapshot().keys, Does.Contain(registry.SaveId));
        }

        // F5 in the main menu or on the results screen wrote a save of that screen.
        [Test]
        public void SaveManager_RefusesToSaveOutsideAGame()
        {
            SaveManager save = SaveManager.Instance != null ? SaveManager.Instance : Create("SaveManager").AddComponent<SaveManager>();
            GameFlowManager flow = GameFlowManager.Instance != null ? GameFlowManager.Instance : Create("GameFlow").AddComponent<GameFlowManager>();
            GameFlowState previousState = GameFlowManager.State;
            typeof(GameFlowManager).GetMethod("SetState", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(flow, new object[] { GameFlowState.MainMenu });
            string failure = null;
            save.OperationFailed += (slot, message) => failure = message;

            save.SaveGame("test_outside_game");
            bool canSave = SaveManager.CanSaveNow();
            typeof(GameFlowManager).GetMethod("SetState", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(flow, new object[] { previousState });

            Assert.That(failure, Is.Not.Null);
            Assert.That(canSave, Is.False);
        }

        // Items missing from the item catalogue vanished from the inventory on load.
        [Test]
        public void InventoryManager_RestoresSceneItemsMissingFromTheCatalogue()
        {
            var item = ScriptableObject.CreateInstance<InventoryItemData>();
            Set(item, "_itemId", "test_uncatalogued_key");
            GameObject pickupRoot = Create("Pickup");
            pickupRoot.SetActive(false);
            Set(pickupRoot.AddComponent<PickableItem>(), "_itemData", item);
            var inventory = Create("Inventory").AddComponent<InventoryManager>();
            Assert.That(inventory.AddItem(item), Is.True);
            string json = inventory.SaveData();
            inventory.Clear();

            inventory.LoadData(json);

            Assert.That(inventory.HasItem("test_uncatalogued_key"), Is.True);
            Object.Destroy(item);
        }
    }
}
