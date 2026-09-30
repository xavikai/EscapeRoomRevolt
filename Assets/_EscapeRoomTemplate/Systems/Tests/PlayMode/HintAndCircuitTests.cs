using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Systems.Hint;
using EscapeRoomRevolt.Systems.Interaction;
using EscapeRoomRevolt.Systems.Puzzle;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EscapeRoomRevolt.Systems.Tests
{
    public sealed class HintAndCircuitTests
    {
        private readonly List<Object> _objects = new List<Object>();
        private GameObject Owner(string name) { var go = new GameObject(name); _objects.Add(go); return go; }
        private static void Set(object owner, string name, object value) => owner.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(owner, value);
        [TearDown] public void Cleanup() { foreach (var obj in _objects) if (obj != null) Object.DestroyImmediate(obj); _objects.Clear(); }

        [UnityTest]
        public IEnumerator HintZone_PhysicsDetectsChildColliderAndShowsDelayedText()
        {
            var manager = Owner("HintManagerTest").AddComponent<HintManager>();
            var data = ScriptableObject.CreateInstance<HintData>(); _objects.Add(data);
            data.delayBeforeFirstHint = .05f;
            data.hints.Add(new HintEntry { hintText = "Child collider hint" });
            var zoneGo = Owner("HintZoneTest"); zoneGo.transform.position = new Vector3(800, 0, 0);
            zoneGo.AddComponent<BoxCollider>().size = Vector3.one * 3;
            var zone = zoneGo.AddComponent<HintZoneTrigger>();
            Set(zone, "_puzzleHintData", data); Set(zone, "_clearOnExit", true);
            var player = Owner("TaggedParent"); player.tag = "Player"; player.transform.position = new Vector3(810, 0, 0);
            var child = new GameObject("UntaggedBody"); child.transform.SetParent(player.transform, false); child.AddComponent<CapsuleCollider>();
            string shown = null;
            System.Action<RequestShowSubtitle> handler = e => shown = e.text;
            EventBus.Subscribe(handler);
            try {
                yield return new WaitForFixedUpdate();
                player.transform.position = zoneGo.transform.position;
                Physics.SyncTransforms();
                yield return new WaitForSeconds(.2f);
                Assert.That(shown, Does.Contain("Child collider hint"));
                player.transform.position += Vector3.right * 10;
                Physics.SyncTransforms();
                yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
                Assert.That(manager.RequestNextHint(), Is.False);
            } finally { EventBus.Unsubscribe(handler); }
        }

        [Test]
        public void LeavingOldZoneDoesNotClearNewZoneHint()
        {
            var manager = Owner("HintManagerTest").AddComponent<HintManager>();
            var oldData = ScriptableObject.CreateInstance<HintData>(); _objects.Add(oldData);
            var newData = ScriptableObject.CreateInstance<HintData>(); _objects.Add(newData);
            newData.hints.Add(new HintEntry { hintText = "New context" });
            manager.SetActivePuzzle(oldData); manager.SetActivePuzzle(newData); manager.ClearActivePuzzle(oldData);
            Assert.That(manager.RequestNextHint(), Is.True);
        }

        [UnityTest]
        public IEnumerator Narrative_PhysicsDetectsChildAndAlwaysCanReplay()
        {
            var zoneGo = Owner("NarrativeTest"); zoneGo.transform.position = new Vector3(900, 0, 0);
            var narrative = zoneGo.AddComponent<NarrativeTrigger>();
            Set(narrative, "_playMode", NarrativePlayMode.Always); Set(narrative, "_cooldownTime", 0f);
            Set(narrative, "_sequences", new List<NarrativeSequence> { new NarrativeSequence { subtitleLines = new List<SubtitleLine> { new SubtitleLine { text = "Narrative replay", duration = .05f } } } });
            var player = Owner("NarrativePlayer"); player.tag = "Player"; player.transform.position = new Vector3(910, 0, 0);
            var child = new GameObject("Body"); child.transform.SetParent(player.transform, false); child.AddComponent<CapsuleCollider>();
            int shown = 0;
            System.Action<RequestShowSubtitle> handler = e => { if (e.text == "Narrative replay") shown++; };
            EventBus.Subscribe(handler);
            try {
                yield return new WaitForFixedUpdate();
                for (int i = 0; i < 2; i++) {
                    player.transform.position = zoneGo.transform.position; Physics.SyncTransforms();
                    yield return new WaitForSeconds(.1f);
                    player.transform.position += Vector3.right * 10; Physics.SyncTransforms();
                    yield return new WaitForSeconds(.1f);
                }
                Assert.That(shown, Is.EqualTo(2));
            } finally { EventBus.Unsubscribe(handler); }
        }

        private LinkedLightsPuzzle Circuit()
        {
            var go = Owner("CircuitTest"); go.SetActive(false);
            var puzzle = go.AddComponent<LinkedLightsPuzzle>();
            var nodes = new LightCircuitNode[5];
            for (int i = 0; i < 5; i++) {
                var links = new List<int>();
                for (int j = Mathf.Max(0, i - 1); j <= Mathf.Min(4, i + 1); j++) links.Add(j);
                nodes[i] = new LightCircuitNode { initiallyOn = i == 1 || i == 3, connections = links.ToArray() };
            }
            Set(puzzle, "_nodes", nodes); go.SetActive(true); return puzzle;
        }

        [Test]
        public void Circuit_SolutionFiresOnceAndResetRestoresInitialState()
        {
            var puzzle = Circuit(); int solved = 0; puzzle.OnSolvedEvent.AddListener(() => solved++);
            puzzle.Press(-1); puzzle.Press(99); Assert.That(puzzle.IsSolved, Is.False);
            puzzle.Press(0); puzzle.Press(2); Assert.That(puzzle.IsSolved, Is.False);
            puzzle.Press(4); Assert.That(puzzle.IsSolved, Is.True);
            puzzle.Press(0); Assert.That(solved, Is.EqualTo(1));
            puzzle.ResetPuzzle(); Assert.That(puzzle.IsSolved, Is.False);
            for (int i = 0; i < 5; i++) Assert.That(puzzle.IsLightOn(i), Is.EqualTo(i == 1 || i == 3));
        }

        [Test]
        public void Circuit_SaveRestoresPartialProgressAndSolvedStateWithoutReplayingEvents()
        {
            var puzzle = Circuit(); puzzle.Press(0); string partial = puzzle.SaveData();
            puzzle.Press(1); puzzle.LoadData(partial);
            puzzle.Press(2); puzzle.Press(4); Assert.That(puzzle.IsSolved, Is.True);
            string solved = puzzle.SaveData(); puzzle.ResetPuzzle();
            int events = 0; puzzle.OnSolvedEvent.AddListener(() => events++);
            puzzle.LoadData(solved); Assert.That(puzzle.IsSolved, Is.True); Assert.That(events, Is.Zero);
            for (int i = 0; i < 5; i++) Assert.That(puzzle.IsLightOn(i), Is.True);
        }
    }
}
