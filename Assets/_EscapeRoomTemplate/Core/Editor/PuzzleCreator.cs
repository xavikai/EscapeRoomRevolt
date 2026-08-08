using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using EscapeRoomRevolt.Systems.Interaction;
using EscapeRoomRevolt.Systems.Puzzle;
using EscapeRoomRevolt.Player.VR;

namespace EscapeRoomRevolt.EditorTools
{
    /// <summary>
    /// Builds puzzles that are playable the moment they are created: controller, the objects the
    /// player actually touches, the view that renders them, and the wiring between all three.
    /// The older creators only produced a configured controller and left a note telling the designer
    /// to add the interactables by hand, which is exactly the manual scripting this menu exists to avoid.
    /// </summary>
    public static class PuzzleCreator
    {
        private const string SolvedClipPath = "Assets/_EscapeRoomTemplate/Audio/UI/PuzzleSolved.wav";

        // ── Sliding ──────────────────────────────────────────────────────────

        [MenuItem("Escape Room Framework/Create/Puzzles/Sliding Puzzle", priority = 204)]
        public static void CreateSlidingPuzzle()
        {
            GameObject root = CreateRoot("NewSlidingPuzzle");
            var puzzle = root.AddComponent<SlidingPuzzle>();

            const int columns = 3, rows = 2;
            var so = new SerializedObject(puzzle);
            so.FindProperty("_columns").intValue = columns;
            so.FindProperty("_rows").intValue = rows;
            SerializedProperty order = so.FindProperty("_targetOrder");
            int cells = columns * rows;
            order.arraySize = cells;
            for (int i = 0; i < cells - 1; i++) order.GetArrayElementAtIndex(i).stringValue = (i + 1).ToString();
            order.GetArrayElementAtIndex(cells - 1).stringValue = string.Empty;
            so.ApplyModifiedProperties();

            GameObject board = new GameObject("Board");
            board.transform.SetParent(root.transform, false);
            board.transform.localPosition = new Vector3(0f, 1.35f, 0f);

            var view = board.AddComponent<SlidingBoardView>();
            var viewSo = new SerializedObject(view);
            viewSo.FindProperty("_puzzle").objectReferenceValue = puzzle;
            viewSo.FindProperty("_origin").objectReferenceValue = board.transform;
            viewSo.FindProperty("_spacing").floatValue = .46f;

            Color[] palette = { Color.red, new Color(1f, .5f, 0f), Color.yellow, Color.green, Color.blue };
            SerializedProperty tiles = viewSo.FindProperty("_tiles");
            tiles.arraySize = cells - 1;
            for (int i = 0; i < cells - 1; i++)
            {
                string id = (i + 1).ToString();
                GameObject tile = CreateBlock("Tile" + id + "_Logic", root.transform,
                    new Vector3(.34f, .34f, .12f), palette[i % palette.Length]);

                var button = tile.AddComponent<SlidingTileButton>();
                var buttonSo = new SerializedObject(button);
                buttonSo.FindProperty("_puzzle").objectReferenceValue = puzzle;
                buttonSo.FindProperty("_tileId").stringValue = id;
                buttonSo.ApplyModifiedProperties();

                MakeClickable(tile, "Mover pieza", button.Move);

                SerializedProperty binding = tiles.GetArrayElementAtIndex(i);
                binding.FindPropertyRelative("tileId").stringValue = id;
                binding.FindPropertyRelative("tile").objectReferenceValue = tile.transform;
            }

            GameObject hole = CreateBlock("EmptySlot_Marker", root.transform,
                new Vector3(.36f, .36f, .06f), new Color(.07f, .07f, .08f));
            Object.DestroyImmediate(hole.GetComponent<Collider>());
            viewSo.FindProperty("_emptyMarker").objectReferenceValue = hole.transform;
            viewSo.ApplyModifiedProperties();

            view.SnapAll();
            Finalize(root, $"Sliding Puzzle {columns}x{rows} created and playable: {cells - 1} clickable tiles laid out on the board. "
                + "Change Columns/Rows on the puzzle, then use the board's context menu 'Rebuild tiles for grid size'.");
        }

        // ── Placement ────────────────────────────────────────────────────────

        [MenuItem("Escape Room Framework/Create/Puzzles/Placement Puzzle", priority = 202)]
        public static void CreatePlacementPuzzle()
        {
            GameObject root = CreateRoot("NewPlacementPuzzle");
            var puzzle = root.AddComponent<PlacementPuzzle>();

            string[] pieceIds = { "piece_a", "piece_b" };
            string[] socketIds = { "socket_a", "socket_b", "socket_c" };

            var so = new SerializedObject(puzzle);
            SerializedProperty rules = so.FindProperty("_rules");
            rules.arraySize = pieceIds.Length;
            for (int i = 0; i < pieceIds.Length; i++)
            {
                rules.GetArrayElementAtIndex(i).FindPropertyRelative("pieceId").stringValue = pieceIds[i];
                rules.GetArrayElementAtIndex(i).FindPropertyRelative("correctSocketId").stringValue = socketIds[i];
            }
            so.ApplyModifiedProperties();

            // Carriable pieces.
            Color[] pieceColors = { new Color(.85f, .25f, .25f), new Color(.25f, .45f, .85f) };
            for (int i = 0; i < pieceIds.Length; i++)
            {
                GameObject piece = CreateBlock("Piece" + (char)('A' + i) + "_Logic", root.transform,
                    Vector3.one * .3f, pieceColors[i]);
                piece.transform.localPosition = new Vector3(-.8f + i * 1.6f, 1f, 0f);

                var body = piece.AddComponent<Rigidbody>();
                body.mass = .5f;

                var grabbable = piece.AddComponent<PhysicsGrabbable>();
                var grabSo = new SerializedObject(grabbable);
                grabSo.FindProperty("_interactionPrompt").stringValue = "Coger pieza";
                // Carried, never hurled: a thrown piece can land out of reach and softlock the puzzle.
                grabSo.FindProperty("_canBeThrown").boolValue = false;
                grabSo.ApplyModifiedProperties();

                var tag = piece.AddComponent<GrabbablePiece>();
                var tagSo = new SerializedObject(tag);
                tagSo.FindProperty("_pieceId").stringValue = pieceIds[i];
                tagSo.ApplyModifiedProperties();

                MakeVRCarryable(piece, grabbable);
            }

            // Sockets: one more than there are pieces, so there is a real choice to get wrong.
            for (int i = 0; i < socketIds.Length; i++)
            {
                GameObject socket = CreateBlock("Socket" + (char)('A' + i) + "_Receiver", root.transform,
                    Vector3.one * .4f, new Color(.5f, .5f, .5f));
                socket.transform.localPosition = new Vector3(-1f + i, 1f, 2f);

                var box = socket.GetComponent<BoxCollider>();
                box.isTrigger = true;
                box.size = new Vector3(2f, 2f, 2f); // generous capture zone relative to the visual

                var receiver = socket.AddComponent<PieceSocketReceiver>();
                var recvSo = new SerializedObject(receiver);
                recvSo.FindProperty("_puzzle").objectReferenceValue = puzzle;
                recvSo.FindProperty("_socketId").stringValue = socketIds[i];
                recvSo.ApplyModifiedProperties();
            }

            Finalize(root, "Placement Puzzle created and playable: 2 carriable pieces and 3 sockets (one is a decoy). "
                + "Enable Randomize Mapping on the puzzle to reshuffle which socket is correct each playthrough.");
        }

        // ── Throw ────────────────────────────────────────────────────────────

        [MenuItem("Escape Room Framework/Create/Puzzles/Throw Puzzle", priority = 206)]
        public static void CreateThrowPuzzle()
        {
            GameObject root = CreateRoot("NewThrowPuzzle");
            var puzzle = root.AddComponent<ThrowPuzzle>();

            string[] ids = { "target_1", "target_2", "target_3" };
            var so = new SerializedObject(puzzle);
            SerializedProperty list = so.FindProperty("_targetIds");
            list.arraySize = ids.Length;
            for (int i = 0; i < ids.Length; i++) list.GetArrayElementAtIndex(i).stringValue = ids[i];
            so.ApplyModifiedProperties();

            float[] heights = { 1f, 1.7f, 1f };
            for (int i = 0; i < ids.Length; i++)
            {
                GameObject target = CreateBlock("Target" + (i + 1), root.transform,
                    new Vector3(.6f, .6f, .15f), Color.red);
                target.transform.localPosition = new Vector3(-1.5f + i * 1.5f, heights[i], 3.5f);

                var script = target.AddComponent<ThrowTarget>();
                var targetSo = new SerializedObject(script);
                targetSo.FindProperty("_puzzle").objectReferenceValue = puzzle;
                targetSo.FindProperty("_targetId").stringValue = ids[i];
                targetSo.ApplyModifiedProperties();
            }

            Finalize(root, "Throw Puzzle created and playable: 3 targets that must each be hit by a thrown PhysicsGrabbable. "
                + "Place some grabbable objects nearby for the player to throw.");
        }

        // ── Sequence ─────────────────────────────────────────────────────────

        [MenuItem("Escape Room Framework/Create/Puzzles/Sequence Puzzle", priority = 207)]
        public static void CreateSequencePuzzle()
        {
            GameObject root = CreateRoot("NewSequencePuzzle");
            var puzzle = root.AddComponent<SequencePuzzle>();

            string[] steps = { "red", "green", "blue" };
            Color[] colors = { Color.red, Color.green, Color.blue };

            var so = new SerializedObject(puzzle);
            SerializedProperty sequence = so.FindProperty("_correctSequence");
            sequence.arraySize = steps.Length;
            for (int i = 0; i < steps.Length; i++) sequence.GetArrayElementAtIndex(i).stringValue = steps[i];
            so.ApplyModifiedProperties();

            for (int i = 0; i < steps.Length; i++)
            {
                GameObject button = CreateBlock(steps[i] + "Button_Logic", root.transform,
                    new Vector3(.3f, .3f, .3f), colors[i]);
                button.transform.localPosition = new Vector3(-.6f + i * .6f, 1f, 0f);

                var relay = button.AddComponent<SequenceStepButton>();
                var relaySo = new SerializedObject(relay);
                relaySo.FindProperty("_puzzle").objectReferenceValue = puzzle;
                relaySo.FindProperty("_stepId").stringValue = steps[i];
                relaySo.ApplyModifiedProperties();

                MakeClickable(button, "Pulsar", relay.Press);
            }

            Finalize(root, "Sequence Puzzle created and playable: 3 buttons that must be pressed red -> green -> blue. "
                + "Enable Randomize Order to reshuffle the required order each playthrough.");
        }

        // ── State ────────────────────────────────────────────────────────────

        [MenuItem("Escape Room Framework/Create/Puzzles/State Puzzle", priority = 208)]
        public static void CreateStatePuzzle()
        {
            GameObject root = CreateRoot("NewStatePuzzle");
            var puzzle = root.AddComponent<StatePuzzle>();

            int[] required = { 0, 1, 2 };
            var so = new SerializedObject(puzzle);
            SerializedProperty conditions = so.FindProperty("_conditions");
            conditions.arraySize = required.Length;

            for (int i = 0; i < required.Length; i++)
            {
                GameObject lever = CreateBlock("Lever" + (i + 1) + "_Logic", root.transform,
                    new Vector3(.12f, .5f, .12f), new Color(.6f, .4f, .2f));
                lever.transform.localPosition = new Vector3(-.8f + i * .8f, 1f, 0f);

                var positioner = lever.AddComponent<SteppedPositioner>();
                var posSo = new SerializedObject(positioner);
                SerializedProperty positions = posSo.FindProperty("_positions");
                positions.arraySize = 3;
                Vector3[] angles = { new Vector3(-40f, 0f, 0f), Vector3.zero, new Vector3(40f, 0f, 0f) };
                for (int p = 0; p < 3; p++)
                {
                    positions.GetArrayElementAtIndex(p).FindPropertyRelative("prompt").stringValue = "Mover palanca";
                    positions.GetArrayElementAtIndex(p).FindPropertyRelative("rotation").vector3Value = angles[p];
                }
                posSo.FindProperty("_visualTransform").objectReferenceValue = lever.transform;
                posSo.ApplyModifiedProperties();

                lever.AddComponent<InteractableCycler>();
                AddVRBridge(lever, lever.GetComponent<InteractableCycler>());

                conditions.GetArrayElementAtIndex(i).FindPropertyRelative("Positioner").objectReferenceValue = positioner;
                conditions.GetArrayElementAtIndex(i).FindPropertyRelative("RequiredIndex").intValue = required[i];
            }
            so.ApplyModifiedProperties();

            Finalize(root, "State Puzzle created and playable: 3 levers with 3 positions each; solved when they read 0/1/2 left to right. "
                + "Add or remove entries in each lever's Positions list to change how many states it cycles through.");
        }

        // ── Linking a puzzle to what it activates ────────────────────────────

        [MenuItem("Escape Room Framework/Create/Puzzles/Link Selected Objects To Puzzle", priority = 219)]
        public static void LinkSelectionToPuzzle()
        {
            GameObject[] selection = Selection.gameObjects;
            PuzzleController puzzle = null;
            var targets = new System.Collections.Generic.List<GameObject>();

            foreach (GameObject selected in selection)
            {
                var candidate = selected.GetComponent<PuzzleController>();
                if (candidate != null && puzzle == null) puzzle = candidate;
                else targets.Add(selected);
            }

            if (puzzle == null || targets.Count == 0)
            {
                EditorUtility.DisplayDialog("Link To Puzzle",
                    "Select the puzzle and, holding Ctrl/Cmd, the object(s) it should activate.\n\n" +
                    "Doors get Unlock + ForceOpen; anything else is switched on with SetActive(true).",
                    "OK");
                return;
            }

            var report = new System.Text.StringBuilder();
            foreach (GameObject target in targets)
            {
                var door = target.GetComponent<Door>();
                var portal = target.GetComponent<RoomPortal>();
                var audio = target.GetComponent<AudioSource>();

                if (door != null)
                {
                    if (!AlreadyWired(puzzle, target, "Unlock")) UnityEventTools.AddVoidPersistentListener(puzzle.OnSolvedEvent, door.Unlock);
                    if (!AlreadyWired(puzzle, target, "ForceOpen")) UnityEventTools.AddVoidPersistentListener(puzzle.OnSolvedEvent, door.ForceOpen);
                    report.Append($"\n  · {target.name}: Unlock + ForceOpen");
                }
                else if (portal != null)
                {
                    if (!AlreadyWired(puzzle, target, "Unlock")) UnityEventTools.AddVoidPersistentListener(puzzle.OnSolvedEvent, portal.Unlock);
                    report.Append($"\n  · {target.name}: Unlock");
                }
                else if (audio != null)
                {
                    if (!AlreadyWired(puzzle, target, "Play")) UnityEventTools.AddVoidPersistentListener(puzzle.OnSolvedEvent, audio.Play);
                    report.Append($"\n  · {target.name}: Play");
                }
                else
                {
                    // Generic fallback: a light, a particle system, a whole prop — start it disabled
                    // in the scene and let solving switch it on.
                    if (!AlreadyWired(puzzle, target, "SetActive"))
                        UnityEventTools.AddBoolPersistentListener(puzzle.OnSolvedEvent, target.SetActive, true);
                    report.Append($"\n  · {target.name}: SetActive(true) — remember to disable it in the scene");
                }
            }

            EditorUtility.SetDirty(puzzle);
            Debug.Log($"[Escape Room Framework] '{puzzle.name}' On Solved now activates:{report}", puzzle);
        }

        /// <summary>Prevents a second click from stacking a duplicate call onto On Solved.</summary>
        private static bool AlreadyWired(PuzzleController puzzle, GameObject target, string method)
        {
            var so = new SerializedObject(puzzle);
            SerializedProperty calls = so.FindProperty("_onSolved").FindPropertyRelative("m_PersistentCalls.m_Calls");
            for (int i = 0; i < calls.arraySize; i++)
            {
                SerializedProperty call = calls.GetArrayElementAtIndex(i);
                Object wired = call.FindPropertyRelative("m_Target").objectReferenceValue;
                GameObject wiredObject = wired is Component c ? c.gameObject : wired as GameObject;
                if (wiredObject == target && call.FindPropertyRelative("m_MethodName").stringValue == method) return true;
            }
            return false;
        }

        // ── Feedback for any puzzle ──────────────────────────────────────────

        [MenuItem("Escape Room Framework/Create/Puzzles/Add Feedback To Selected Puzzle", priority = 220)]
        public static void AddFeedbackToSelectedPuzzle()
        {
            var puzzle = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponent<PuzzleController>()
                : null;
            if (puzzle == null)
            {
                EditorUtility.DisplayDialog("Add Feedback",
                    "Select a GameObject with a puzzle component (any PuzzleController) first.", "OK");
                return;
            }

            Transform owner = puzzle.transform;

            // The consequence (door, light, whatever) belongs to the level, not to this tool. If one
            // is already wired to OnSolved, frame that; otherwise frame the puzzle and say so.
            Transform payoff = FindWiredPayoff(puzzle) ?? owner;
            bool foundPayoff = payoff != owner;
            Vector3 focus = payoff.position;

            GameObject camObject = new GameObject("PuzzleFeedbackCamera");
            Undo.RegisterCreatedObjectUndo(camObject, "Add puzzle feedback");
            camObject.transform.SetParent(owner, true);
            camObject.transform.position = focus + owner.right * 2.9f + Vector3.up * .7f - owner.forward * 2.6f;
            camObject.transform.LookAt(focus);
            Camera camera = camObject.AddComponent<Camera>();
            camObject.SetActive(false);

            GameObject soundObject = new GameObject("SolvedSound");
            Undo.RegisterCreatedObjectUndo(soundObject, "Add puzzle feedback");
            soundObject.transform.SetParent(owner, true);
            soundObject.transform.position = focus;
            var source = soundObject.AddComponent<AudioSource>();
            source.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(SolvedClipPath);
            source.playOnAwake = false;
            source.spatialBlend = .85f;
            source.minDistance = 3f;
            source.maxDistance = 25f;

            // Camera assignment goes through a SerializedObject; the persistent listener is added
            // afterwards, because applying a stale SerializedObject would wipe it.
            var puzzleSo = new SerializedObject(puzzle);
            puzzleSo.FindProperty("_feedbackCamera").objectReferenceValue = camera;
            puzzleSo.ApplyModifiedProperties();

            if (source.clip != null) UnityEventTools.AddVoidPersistentListener(puzzle.OnSolvedEvent, source.Play);
            EditorUtility.SetDirty(puzzle);

            Selection.activeGameObject = camObject;
            Debug.Log($"[Escape Room Framework] Feedback added to '{puzzle.name}': "
                + (foundPayoff
                    ? $"camera framing '{payoff.name}' (already wired to On Solved)"
                    : "camera framing the puzzle itself — move it to point at whatever solving it activates")
                + (source.clip != null ? " and solved sound." : ". No PuzzleSolved.wav found, so no sound was wired."));
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        /// <summary>
        /// The object the puzzle already acts on when solved, so the feedback camera frames the
        /// actual payoff instead of a generic point. Prefers a door, since that is the usual
        /// consequence, and otherwise takes the first target wired to On Solved.
        /// </summary>
        private static Transform FindWiredPayoff(PuzzleController puzzle)
        {
            var so = new SerializedObject(puzzle);
            SerializedProperty calls = so.FindProperty("_onSolved").FindPropertyRelative("m_PersistentCalls.m_Calls");
            if (calls == null) return null;

            Transform firstTarget = null;
            for (int i = 0; i < calls.arraySize; i++)
            {
                Object target = calls.GetArrayElementAtIndex(i).FindPropertyRelative("m_Target").objectReferenceValue;
                Transform candidate = target is Component component ? component.transform
                    : target is GameObject go ? go.transform
                    : null;
                if (candidate == null) continue;

                if (candidate.GetComponent<Door>() != null) return candidate;
                if (firstTarget == null) firstTarget = candidate;
            }
            return firstTarget;
        }

        private static GameObject CreateRoot(string name)
        {
            var root = new GameObject(name);
            SceneView view = SceneView.lastActiveSceneView;
            if (view != null) root.transform.position = view.pivot;
            return root;
        }

        private static GameObject CreateBlock(string name, Transform parent, Vector3 scale, Color color)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent, false);
            block.transform.localScale = scale;
            Paint(block, color);
            return block;
        }

        private static void Paint(GameObject target, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) return;
            var material = new Material(shader) { color = color };
            target.GetComponent<Renderer>().material = material;
        }

        /// <summary>Wires a click (PC and VR alike) to a void method, which is the part designers otherwise have to do by hand for every single piece.</summary>
        private static void MakeClickable(GameObject target, string prompt, UnityEngine.Events.UnityAction action)
        {
            var trigger = target.AddComponent<InteractableTrigger>();
            var so = new SerializedObject(trigger);
            so.FindProperty("_prompt").stringValue = prompt;
            so.ApplyModifiedProperties();

            UnityEventTools.AddVoidPersistentListener(trigger.OnInteractEvent, action);
            AddVRBridge(target, trigger);
        }

        /// <summary>
        /// Sets a piece up so it can actually be picked up and carried in VR. XRSimpleInteractable —
        /// what the clickable helpers use — can only be *selected*: it never attaches the object to
        /// the hand, so a piece set up that way is impossible to carry to its socket in a headset.
        /// </summary>
        private static void MakeVRCarryable(GameObject target, MonoBehaviour source)
        {
            var grab = target.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            var grabSo = new SerializedObject(grab);
            grabSo.FindProperty("m_MovementType").enumValueIndex = 2;
            grabSo.FindProperty("m_ThrowOnDetach").boolValue = true;
            grabSo.ApplyModifiedProperties();

            Collider collider = target.GetComponent<Collider>();
            if (collider != null) grab.colliders.Add(collider);

            var bridge = target.AddComponent<VRInteractionBridge>();
            var bridgeSo = new SerializedObject(bridge);
            bridgeSo.FindProperty("_interactableSource").objectReferenceValue = source;
            // Grabbing is the interaction in VR; firing Interact() as well would double up.
            bridgeSo.FindProperty("_interactOnSelect").boolValue = false;
            bridgeSo.ApplyModifiedProperties();
        }

        private static void AddVRBridge(GameObject target, MonoBehaviour source)
        {
            var interactable = target.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            if (interactable == null)
            {
                interactable = target.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
                Collider collider = target.GetComponent<Collider>();
                if (collider != null) interactable.colliders.Add(collider);
            }

            if (target.GetComponent<VRInteractionBridge>() != null) return;
            var bridge = target.AddComponent<VRInteractionBridge>();
            var so = new SerializedObject(bridge);
            so.FindProperty("_interactableSource").objectReferenceValue = source;
            so.ApplyModifiedProperties();
        }

        private static void Finalize(GameObject root, string message)
        {
            Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);
            Selection.activeGameObject = root;
            Debug.Log("[Escape Room Framework] " + message);
        }
    }
}
