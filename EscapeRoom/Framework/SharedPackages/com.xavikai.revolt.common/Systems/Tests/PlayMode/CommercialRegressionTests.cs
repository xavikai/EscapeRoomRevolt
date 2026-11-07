using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Core.Flow;
using EscapeRoomRevolt.Core.Save;
using EscapeRoomRevolt.Player.VR;
using EscapeRoomRevolt.Systems.Interaction;
using EscapeRoomRevolt.Systems.Puzzle;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EscapeRoomRevolt.Systems.Tests
{
    public class CommercialRegressionTests
    {
        private readonly List<GameObject> _objects = new List<GameObject>();
        private GameObject Create(string name)
        {
            var root = new GameObject(name);
            _objects.Add(root);
            return root;
        }

        [SetUp]
        public void SetUp() { EventBus.Clear(); GameplayBlockState.Reset(); Time.timeScale = 1f; }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var root in _objects) if (root != null) Object.Destroy(root);
            _objects.Clear();
            Time.timeScale = 1f;
            EventBus.Clear();
            GameplayBlockState.Reset();
            yield return null;
        }

        private static void Set(object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

        [Test]
        public void DisabledInteractable_RejectsDispatch()
        {
            var trigger = Create("DisabledTrigger").AddComponent<InteractableTrigger>();
            trigger.enabled = false;
            Assert.That(InteractionDispatcher.TryPerform(trigger), Is.False);
            trigger.enabled = true;
            Assert.That(InteractionDispatcher.TryPerform(trigger), Is.True);
        }

        [Test]
        public void VrBridge_DoesNotDispatchThroughModalOrPause()
        {
            var root = Create("VRTrigger");
            root.AddComponent<InteractableTrigger>();
            var bridge = root.AddComponent<VRInteractionBridge>();
            int calls = 0;
            EventBus.Subscribe<OnInteractionPerformed>(_ => calls++);
            EventBus.Publish(new OnMenuUIBlockingChanged { isBlocking = true });
            bridge.Select();
            Assert.That(calls, Is.Zero);
            EventBus.Publish(new OnMenuUIBlockingChanged { isBlocking = false });
            Time.timeScale = 0f;
            bridge.Select();
            Assert.That(calls, Is.Zero);
            Time.timeScale = 1f;
            bridge.Select();
            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void VrHardware_HeldObjectBlocksSocketAndSecondHand()
        {
            var rig = Create("VRRig");
            var left = Create("LeftHand").transform;
            var right = Create("RightHand").transform;
            left.SetParent(rig.transform);
            right.SetParent(rig.transform);
            rig.AddComponent<VRPlayerPlatformAdapter>().Configure(rig.transform, left, right);
            var hardware = rig.AddComponent<VRHardwareInteractor>();
            var prop = Create("VRProp");
            prop.AddComponent<BoxCollider>();
            var item = prop.AddComponent<PhysicsGrabbable>();
            var type = typeof(VRHardwareInteractor);
            object leftState = type.GetField("_left", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(hardware);
            object rightState = type.GetField("_right", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(hardware);
            var begin = type.GetMethod("BeginGrab", BindingFlags.NonPublic | BindingFlags.Instance);
            begin.Invoke(hardware, new object[] { leftState, item });
            Assert.That(hardware.IsHolding(item), Is.True, "Grab failed: interact=" + item.CanInteract +
                ", body=" + (item.GetComponent<Rigidbody>() != null) + ", blocked=" + GameplayBlockState.IsBlocking +
                ", leftOrigin=" + leftState.GetType().GetField("origin").GetValue(leftState));
            var heldBySocket = typeof(PhysicsSocket).GetMethod("IsCurrentlyHeld", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(heldBySocket.Invoke(null, new object[] { item }), Is.True);
            begin.Invoke(hardware, new object[] { rightState, item });
            Assert.That(item.transform.parent, Is.EqualTo(left));
            type.GetMethod("EndGrab", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(hardware, new[] { leftState });
            Assert.That(hardware.IsHolding(item), Is.False);
            Assert.That(heldBySocket.Invoke(null, new object[] { item }), Is.False);
            Assert.That(item.GetComponent<Rigidbody>().isKinematic, Is.False);
        }

        [Test]
        public void Sequence_RestoresPartialAttemptWithoutKeepingLaterInput()
        {
            var puzzle = Create("Sequence").AddComponent<SequencePuzzle>();
            Set(puzzle, "_correctSequence", new List<string> { "A", "B", "C" });
            puzzle.InputStep("A");
            string snapshot = puzzle.SaveData();
            puzzle.InputStep("B");
            puzzle.LoadData(snapshot);
            puzzle.InputStep("B");
            Assert.That(puzzle.IsSolved, Is.False);
            puzzle.InputStep("C");
            Assert.That(puzzle.IsSolved, Is.True);
        }

        [Test]
        public void EmptySequence_DoesNotThrowOrSolve()
        {
            var puzzle = Create("EmptySequence").AddComponent<SequencePuzzle>();
            Assert.DoesNotThrow(() => puzzle.InputStep("A"));
            Assert.That(puzzle.IsSolved, Is.False);
        }

        [Test]
        public void StatePuzzle_MissingRequiredPositionerCannotSolve()
        {
            var root = Create("StatePuzzle");
            root.SetActive(false);
            var puzzle = root.AddComponent<StatePuzzle>();
            Set(puzzle, "_conditions", new List<StateCondition> { new StateCondition() });
            root.SetActive(true);
            typeof(StatePuzzle).GetMethod("CheckStates", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(puzzle, null);
            Assert.That(puzzle.IsSolved, Is.False);
        }

        [UnityTest]
        public IEnumerator Socket_LoadRecreatesOneVisualAndResetRemovesIt()
        {
            var root = Create("SocketPuzzle");
            var puzzle = root.AddComponent<SocketPuzzle>();
            var anchor = Create("Placement").transform;
            anchor.SetParent(root.transform);
            var visual = Create("VisualPrefab");
            Set(puzzle, "_itemPlacementPoint", anchor);
            Set(puzzle, "_placedItemPrefab", visual);
            puzzle.LoadData("{\"stateIndex\":2}");
            puzzle.LoadData("{\"stateIndex\":2}");
            Assert.That(anchor.childCount, Is.EqualTo(1));
            puzzle.ResetPuzzle();
            yield return null;
            Assert.That(anchor.childCount, Is.Zero);
        }

        [Test]
        public void FailedLoad_RestoresFlowState()
        {
            Create("SaveManager").AddComponent<SaveManager>();
            var flow = Create("GameFlow").AddComponent<GameFlowManager>();
            GameFlowState previous = GameFlowManager.State;
            LogAssert.Expect(LogType.Warning, new Regex("No se ha encontrado la partida"));
            flow.LoadSlot("audit-missing-" + System.Guid.NewGuid());
            Assert.That(GameFlowManager.State, Is.EqualTo(previous));
        }

        [Test]
        public void InvalidRoom_DoesNotEnterLoadingOrLoseSession()
        {
            var save = Create("SaveManager").AddComponent<SaveManager>();
            save.MarkAsDestroyed("collected-key");
            var flow = Create("GameFlow").AddComponent<GameFlowManager>();
            var previous = GameFlowManager.State;
            LogAssert.Expect(LogType.Warning, new Regex("is not available"));
            flow.TransitionToRoom("AuditMissingScene", "", RoomLoadMode.Single);
            Assert.That(GameFlowManager.State, Is.EqualTo(previous));
            Assert.That(save.IsDestroyed("collected-key"), Is.True);
        }
    }
}
