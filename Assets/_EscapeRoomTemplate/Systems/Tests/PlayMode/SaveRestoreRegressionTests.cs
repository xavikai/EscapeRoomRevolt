using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Systems.Interaction;
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
    }
}
