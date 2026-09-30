using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Core.Flow;
using EscapeRoomRevolt.Core.Save;
using EscapeRoomRevolt.Systems.Flow;
using EscapeRoomRevolt.Systems.Interaction;
using EscapeRoomRevolt.Systems.Puzzle;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace EscapeRoomRevolt.Systems.Tests
{
    public sealed class AdvancedEscapeRoomMechanicsTests
    {
        [UnityTest]
        public IEnumerator MultiStage_KeepsChildrenVisibleLocksOrderAndSolvesWhenAllComplete()
        {
            var owner = new GameObject("MultiStageTest");
            owner.SetActive(false);
            var puzzle = owner.AddComponent<MultiStagePuzzle>();
            var firstRoot = new GameObject("FirstPuzzle");
            firstRoot.transform.SetParent(owner.transform);
            var first = firstRoot.AddComponent<SequencePuzzle>();
            SetPrivate(first, "_correctSequence", new List<string> { "first" });
            var firstControl = firstRoot.AddComponent<InteractableTrigger>();

            var secondRoot = new GameObject("SecondPuzzle");
            secondRoot.transform.SetParent(owner.transform);
            var second = secondRoot.AddComponent<SequencePuzzle>();
            SetPrivate(second, "_correctSequence", new List<string> { "second" });
            var secondControl = secondRoot.AddComponent<InteractableTrigger>();

            SetPrivate(puzzle, "_puzzles", new List<ChainedPuzzle>
            {
                new ChainedPuzzle { id = "first", puzzle = first, interactionRoot = firstRoot },
                new ChainedPuzzle { id = "second", puzzle = second, interactionRoot = secondRoot }
            });
            SetPrivate(puzzle, "_requireOrder", true);
            SetPrivate(puzzle, "_lockFuturePuzzles", true);
            owner.SetActive(true);
            yield return null;

            Assert.That(firstRoot.activeSelf, Is.True);
            Assert.That(secondRoot.activeSelf, Is.True);
            Assert.That(firstControl.CanInteract, Is.True);
            Assert.That(secondControl.CanInteract, Is.False);

            first.InputStep("first");
            Assert.That(puzzle.IsSolved, Is.False);
            Assert.That(secondRoot.activeSelf, Is.True);
            Assert.That(secondControl.CanInteract, Is.True);

            second.InputStep("second");
            Assert.That(puzzle.IsSolved, Is.True);
            Object.Destroy(owner);
        }

        [UnityTest]
        public IEnumerator NumberWheel_StepsBothDirectionsAndWraps()
        {
            var owner = new GameObject("NumberWheelTest");
            owner.SetActive(false);
            var positioner = owner.AddComponent<SteppedPositioner>();
            var positions = new List<SteppedPosition>();
            for (int i = 0; i < 10; i++) positions.Add(new SteppedPosition { rotation = new Vector3(i * 36f, 0f, 0f) });
            SetPrivate(positioner, "_positions", positions);
            var view = owner.AddComponent<NumberWheelView>();
            owner.SetActive(true);
            yield return null;

            positioner.Previous();
            Assert.That(view.CurrentDigit, Is.EqualTo(9));
            positioner.Advance();
            Assert.That(view.CurrentDigit, Is.EqualTo(0));
            positioner.Step(2);
            Assert.That(view.CurrentDigit, Is.EqualTo(2));
            Object.Destroy(owner);
        }

        [UnityTest]
        public IEnumerator NumberWheelInteraction_RequiresFocusAndSupportsBothDirections()
        {
            var focusOwner = new GameObject("NumberWheelFocusTest");
            var focus = focusOwner.AddComponent<PuzzleFocusPoint>();
            var cameraOwner = new GameObject("NumberWheelFocusCameraTest");
            var focusCamera = cameraOwner.AddComponent<Camera>();
            cameraOwner.SetActive(false);
            SetPrivate(focus, "_focusCamera", focusCamera);

            var wheelOwner = new GameObject("FocusedNumberWheelTest");
            wheelOwner.SetActive(false);
            var positioner = wheelOwner.AddComponent<SteppedPositioner>();
            var wheel = wheelOwner.AddComponent<NumberWheelInteractable>();
            SetPrivate(positioner, "_positions", new List<SteppedPosition>
            {
                new SteppedPosition(), new SteppedPosition(), new SteppedPosition()
            });
            SetPrivate(wheel, "_focusPoint", focus);
            wheelOwner.SetActive(true);
            yield return null;

            Assert.That(wheel.CanInteract, Is.False);
            Assert.That(wheel.TryStep(1), Is.False);
            Assert.That(positioner.CurrentIndex, Is.Zero);

            focus.Enter();
            Assert.That(wheel.CanInteract, Is.True);
            Assert.That(wheel.TryStep(1), Is.True);
            Assert.That(positioner.CurrentIndex, Is.EqualTo(1));
            Assert.That(wheel.TryStep(-1), Is.True);
            Assert.That(positioner.CurrentIndex, Is.Zero);

            focus.Exit();
            Assert.That(wheel.CanInteract, Is.False);
            Object.Destroy(wheelOwner);
            Object.Destroy(focusOwner);
            Object.Destroy(cameraOwner);
            yield return null;
        }

        [Test]
        public void NumberWheelsAuthoring_NormalizesCountAndDigits()
        {
            var owner = new GameObject("NumberWheelsAuthoringTest");
            var authoring = owner.AddComponent<NumberWheelsPuzzleAuthoring>();

            authoring.SetCombination(new[] { -4, 2, 17, 5, 6, 7, 8, 9, 1 });

            Assert.That(authoring.WheelCount, Is.EqualTo(NumberWheelsPuzzleAuthoring.MaximumWheels));
            Assert.That(authoring.Combination, Is.EqualTo(new[] { 0, 2, 9, 5, 6, 7, 8, 9 }));
            Object.DestroyImmediate(owner);
        }

        [UnityTest]
        public IEnumerator MovingHazard_TravelsAlongAnArbitraryThreeDimensionalDirection()
        {
            var start = new GameObject("HazardStart");
            var end = new GameObject("HazardEnd");
            var body = new GameObject("HazardBody");
            start.transform.position = new Vector3(1f, 5f, -2f);
            end.transform.position = new Vector3(-3f, 1f, 4f);
            body.SetActive(false);
            var hazard = body.AddComponent<MovingHazard>();
            SetPrivate(hazard, "_startPoint", start.transform);
            SetPrivate(hazard, "_endPoint", end.transform);
            SetPrivate(hazard, "_travelDuration", 10f);
            SetPrivate(hazard, "_failAtDestination", false);
            body.SetActive(true);
            yield return null;

            hazard.StartHazard();
            hazard.AdvanceTime(5f);
            Assert.That(hazard.Progress, Is.EqualTo(.5f).Within(.001f));
            Assert.That(Vector3.Distance(body.transform.position, new Vector3(-1f, 3f, 1f)), Is.LessThan(.001f));
            Assert.That(Vector3.Distance(hazard.Direction,
                (end.transform.position - start.transform.position).normalized), Is.LessThan(.001f));

            Object.Destroy(body);
            Object.Destroy(start);
            Object.Destroy(end);
        }

        [UnityTest]
        public IEnumerator EventTriggerZone_FiresOnceForThePlayerAndCanBeReset()
        {
            var zoneObject = new GameObject("EventTriggerZoneTest");
            zoneObject.AddComponent<BoxCollider>().isTrigger = true;
            var zone = zoneObject.AddComponent<EventTriggerZone>();
            int activationCount = 0;
            zone.OnEntered.AddListener(() => activationCount++);

            var playerObject = new GameObject("EventTriggerZonePlayerTest");
            playerObject.tag = "Player";
            var playerCollider = playerObject.AddComponent<BoxCollider>();
            yield return null;

            zoneObject.SendMessage("OnTriggerEnter", playerCollider);
            zoneObject.SendMessage("OnTriggerEnter", playerCollider);
            Assert.That(activationCount, Is.EqualTo(1));
            Assert.That(zone.HasTriggered, Is.True);

            zone.ResetZone();
            zoneObject.SendMessage("OnTriggerEnter", playerCollider);
            Assert.That(activationCount, Is.EqualTo(2));

            Object.Destroy(zoneObject);
            Object.Destroy(playerObject);
        }

        [UnityTest, Order(200)]
        public IEnumerator GameOverTimer_PublishesHudStateAndFailsIndependentlyAtZero()
        {
            var owner = new GameObject("GameOverTimerTest");
            owner.SetActive(false);
            var timer = owner.AddComponent<GameOverTimer>();
            SetPrivate(timer, "_duration", 2f);
            SetPrivate(timer, "_showInHud", true);
            SetPrivate(timer, "_hudLabel", "TEST TIMER");

            OnGameOverTimerChanged latest = default;
            int eventCount = 0;
            System.Action<OnGameOverTimerChanged> handler = evt =>
            {
                latest = evt;
                eventCount++;
            };
            EventBus.Subscribe(handler);

            owner.SetActive(true);
            yield return null;
            timer.StartTimer();
            timer.AdvanceTime(1f);

            Assert.That(timer.IsRunning, Is.True);
            Assert.That(timer.TimeRemaining, Is.EqualTo(1f).Within(.001f));
            Assert.That(latest.isVisible, Is.True);
            Assert.That(latest.label, Is.EqualTo("TEST TIMER"));

            timer.AdvanceTime(1f);
            Assert.That(timer.HasExpired, Is.True);
            Assert.That(GameFlowManager.State, Is.EqualTo(GameFlowState.Failed));
            Assert.That(latest.hasExpired, Is.True);
            Assert.That(latest.secondsRemaining, Is.Zero);
            Assert.That(eventCount, Is.GreaterThanOrEqualTo(4));

            EventBus.Unsubscribe(handler);
            Object.Destroy(owner);
            if (GameFlowManager.Instance != null) Object.Destroy(GameFlowManager.Instance.gameObject);
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator ThrowPuzzle_RequiresEveryAuthoredTarget()
        {
            var owner = new GameObject("ThrowPuzzleTest");
            owner.SetActive(false);
            var puzzle = owner.AddComponent<ThrowPuzzle>();
            SetPrivate(puzzle, "_targetIds", new List<string> { "left", "right" });
            owner.SetActive(true);
            yield return null;

            puzzle.RegisterHit("left");
            puzzle.RegisterHit("unknown");
            Assert.That(puzzle.HitCount, Is.EqualTo(1));
            Assert.That(puzzle.IsSolved, Is.False);
            puzzle.RegisterHit("right");
            Assert.That(puzzle.IsSolved, Is.True);
            Object.Destroy(owner);
        }

        [UnityTest]
        public IEnumerator SequencePuzzle_RejectsWrongInputThenAcceptsCorrectOrder()
        {
            var owner = new GameObject("SequencePuzzleTest");
            owner.SetActive(false);
            var puzzle = owner.AddComponent<SequencePuzzle>();
            SetPrivate(puzzle, "_correctSequence", new List<string> { "red", "green", "blue" });
            owner.SetActive(true);
            yield return null;

            puzzle.InputStep("green");
            Assert.That(puzzle.IsSolved, Is.False);
            puzzle.InputStep("red");
            puzzle.InputStep("green");
            puzzle.InputStep("blue");
            Assert.That(puzzle.IsSolved, Is.True);
            Object.Destroy(owner);
        }

        [UnityTest]
        public IEnumerator CodePanelPuzzle_SolvesOnlyWithTheCompleteCode()
        {
            var owner = new GameObject("CodePanelPuzzleTest");
            owner.SetActive(false);
            var puzzle = owner.AddComponent<CodePanelPuzzle>();
            SetPrivate(puzzle, "_correctCode", "3142");
            SetPrivate(puzzle, "_maxCodeLength", 4);
            owner.SetActive(true);
            yield return null;

            puzzle.InputDigit("3");
            puzzle.InputDigit("1");
            puzzle.InputDigit("4");
            Assert.That(puzzle.IsSolved, Is.False);
            puzzle.InputDigit("2");
            Assert.That(puzzle.IsSolved, Is.True);
            Object.Destroy(owner);
        }

        [UnityTest]
        public IEnumerator PipePuzzle_SolvesWhenAdjacentOpeningsConnect()
        {
            var owner = new GameObject("PipePuzzleTest");
            owner.SetActive(false);
            var puzzle = owner.AddComponent<PipePuzzle>();
            SetPrivate(puzzle, "_tiles", new List<PipeTileDefinition>
            {
                new PipeTileDefinition { tileId = "source", row = 0, column = 0, openSides = PipeSide.East },
                new PipeTileDefinition { tileId = "sink", row = 0, column = 1, openSides = PipeSide.North, startingRotationSteps = 2 }
            });
            SetPrivate(puzzle, "_sourceTileId", "source");
            SetPrivate(puzzle, "_sinkTileId", "sink");
            owner.SetActive(true);
            yield return null;

            Assert.That(puzzle.IsPathConnected(), Is.False);
            Assert.That(puzzle.RotateTile("sink"), Is.True);
            Assert.That(puzzle.IsSolved, Is.True);
            Object.Destroy(owner);
        }

        [UnityTest]
        public IEnumerator SlidingPuzzle_LegalMoveCanRestoreSolvedBoard()
        {
            var owner = new GameObject("SlidingPuzzleTest");
            owner.SetActive(false);
            var puzzle = owner.AddComponent<SlidingPuzzle>();
            SetPrivate(puzzle, "_columns", 2);
            SetPrivate(puzzle, "_rows", 2);
            SetPrivate(puzzle, "_holeCell", new Vector2Int(1, 1));
            SetPrivate(puzzle, "_shuffleMoveCount", 1);
            owner.SetActive(true);
            yield return null;

            string tileNextToHole = puzzle.GetTileAt(3);
            Assert.That(string.IsNullOrEmpty(tileNextToHole), Is.False);
            Assert.That(puzzle.TryMoveTile(tileNextToHole), Is.True);
            Assert.That(puzzle.IsSolved, Is.True);
            Object.Destroy(owner);
        }

        [UnityTest]
        public IEnumerator SaveManager_CaptureAndRestoreSnapshotRoundTripsState()
        {
            var managerObject = new GameObject("SaveManagerTest");
            var manager = managerObject.AddComponent<SaveManager>();
            var stateObject = new GameObject("SaveableStateTest");
            var state = stateObject.AddComponent<TestSaveableState>();
            yield return null;

            manager.Register(state);
            state.Value = 7;
            SaveGameData snapshot = manager.CaptureSnapshot();
            state.Value = 99;
            manager.RestoreSnapshot(snapshot, "playmode_memory_test");

            Assert.That(state.Value, Is.EqualTo(7));
            Assert.That(snapshot.keys, Does.Contain(state.SaveId));
            Object.Destroy(stateObject);
            Object.Destroy(managerObject);
            yield return null;
        }

        [UnityTest, Order(100)]
        public IEnumerator MainMenuScene_BuildsEveryRequiredPrimaryOption()
        {
            Scene previous = SceneManager.GetActiveScene();
            AsyncOperation load = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);
            Assert.That(load, Is.Not.Null, "MainMenu must be enabled in Build Settings.");
            yield return load;

            Scene menuScene = SceneManager.GetSceneByName("MainMenu");
            Assert.That(menuScene.IsValid() && menuScene.isLoaded, Is.True);
            SceneManager.SetActiveScene(menuScene);
            yield return null;

            MonoBehaviour controller = null;
            foreach (GameObject root in menuScene.GetRootGameObjects())
            {
                foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (behaviour != null && behaviour.GetType().FullName == "EscapeRoomRevolt.UI.Toolkit.UIToolkitMenuController")
                    {
                        controller = behaviour;
                        break;
                    }
                }
                if (controller != null) break;
            }
            Assert.That(controller, Is.Not.Null);
            MethodInfo showMain = controller.GetType().GetMethod("ShowMain", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(showMain, Is.Not.Null);
            showMain.Invoke(controller, null);
            yield return null;

            UIDocument document = controller.GetComponent<UIDocument>();
            Assert.That(document, Is.Not.Null);
            List<Button> buttons = document.rootVisualElement.Query<Button>().ToList();
            AssertButton(buttons, "Continuar");
            AssertButton(buttons, "Nueva partida");
            AssertButton(buttons, "Cargar partida");
            AssertButton(buttons, "Ajustes");
            AssertButton(buttons, "Créditos");
            AssertButton(buttons, "Salir");
            PropertyInfo blocking = controller.GetType().GetProperty("IsBlockingGameplay", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(blocking, Is.Not.Null);
            Assert.That((bool)blocking.GetValue(controller), Is.True);

            if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            AsyncOperation unload = SceneManager.UnloadSceneAsync(menuScene);
            if (unload != null) yield return unload;
        }

        private static void SetPrivate<T>(object target, string fieldName, T value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Missing field " + fieldName);
            field.SetValue(target, value);
        }

        private static void AssertButton(List<Button> buttons, string expectedText)
        {
            Assert.That(buttons.Exists(button => button.text == expectedText), Is.True,
                "Missing main-menu option: " + expectedText);
        }

        private sealed class TestSaveableState : MonoBehaviour, ISaveable
        {
            [System.Serializable]
            private sealed class Data { public int value; }

            public int Value { get; set; }
            public string SaveId => "playmode_test_state";
            public string SaveData() => JsonUtility.ToJson(new Data { value = Value });
            public void LoadData(string json) => Value = JsonUtility.FromJson<Data>(json).value;
        }
    }
}
