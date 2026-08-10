using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using EscapeRoomRevolt.Systems.Interaction;
using EscapeRoomRevolt.Systems.Puzzle;
using EscapeRoomRevolt.Systems.Survival;
using EscapeRoomRevolt.Systems.Flow;
using EscapeRoomRevolt.Player.VR;
using TMPro;

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

            const int columns = 3, rows = 3;
            var so = new SerializedObject(puzzle);
            so.FindProperty("_columns").intValue = columns;
            so.FindProperty("_rows").intValue = rows;
            so.FindProperty("_holeCell").vector2IntValue = new Vector2Int(columns - 1, rows - 1);
            // Applying runs the puzzle's OnValidate, which is what authors the solved arrangement.
            so.ApplyModifiedProperties();

            GameObject board = new GameObject("Board");
            board.transform.SetParent(root.transform, false);
            board.transform.localPosition = new Vector3(0f, 1.35f, 0f);

            var view = board.AddComponent<SlidingBoardView>();
            var viewSo = new SerializedObject(view);
            viewSo.FindProperty("_puzzle").objectReferenceValue = puzzle;
            viewSo.FindProperty("_origin").objectReferenceValue = board.transform;
            viewSo.ApplyModifiedProperties();

            int tiles = SlidingBoardBuilder.Rebuild(view);
            Finalize(root, $"Sliding Puzzle {columns}x{rows} created and playable: {tiles} clickable tiles. "
                + "Assign Source Image on the board to cut a picture into fragments — that is what tells the player which arrangement is correct. "
                + "To resize, change Columns/Rows on the puzzle and press 'Rebuild board'.");
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
                GameObject piece = CreatePiece("Piece" + (char)('A' + i), root.transform,
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
                GameObject socket = CreatePiece("Socket" + (char)('A' + i), root.transform,
                    Vector3.one * .4f, new Color(.5f, .5f, .5f), PieceCollider.Trigger);
                socket.transform.localPosition = new Vector3(-1f + i, 1f, 2f);

                // Twice the visual, so a piece dropped near the socket still counts. Expressed in
                // world units now that the root is unscaled, where it used to ride on a 0.4 scale.
                socket.GetComponent<BoxCollider>().size = Vector3.one * .8f;

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
                GameObject target = CreatePiece("Target" + (i + 1), root.transform,
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
                GameObject button = CreatePiece(steps[i] + "Button", root.transform,
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
                GameObject lever = CreatePiece("Lever" + (i + 1), root.transform,
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
                // The art swings, the logic node stays put — otherwise rotating the lever rotates its
                // collider and every script hanging off it.
                posSo.FindProperty("_visualTransform").objectReferenceValue = FindVisuals(lever.transform);
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

        // ── Number wheels ───────────────────────────────────────────────────────────────

        [MenuItem("Escape Room Framework/Create/Puzzles/Number Wheels Puzzle", priority = 209)]
        public static void CreateNumberWheelsPuzzle()
        {
            NumberWheelsPuzzleWizard.Open();
        }

        internal static void CreateConfiguredNumberWheelsPuzzle(int[] combination)
        {
            int[] normalized = NormalizeNumberWheelsCombination(combination);
            GameObject root = CreateNumberWheelsPuzzleKit("NumberWheelsPuzzle", normalized);
            Finalize(root, $"Dynamic Number Wheels Puzzle created and playable: {normalized.Length} decimal wheels; "
                + $"the solution is {string.Concat(normalized)}. Resize or rebuild it from NumberWheelsPuzzleAuthoring.");
        }

        public static GameObject CreateNumberWheelsPuzzleKit(string name, int[] combination)
        {
            GameObject root = new GameObject(name);
            root.AddComponent<StatePuzzle>();
            root.AddComponent<PuzzleFocusPoint>();
            root.AddComponent<NumberWheelsPuzzleAuthoring>();
            RebuildNumberWheelsPuzzleKit(root, combination);
            return root;
        }

        public static void RebuildNumberWheelsPuzzleKit(GameObject root, int[] combination)
        {
            if (root == null) return;
            int[] normalized = NormalizeNumberWheelsCombination(combination);

            StatePuzzle puzzle = root.GetComponent<StatePuzzle>();
            if (puzzle == null) puzzle = root.AddComponent<StatePuzzle>();
            PuzzleFocusPoint focus = root.GetComponent<PuzzleFocusPoint>();
            if (focus == null) focus = root.AddComponent<PuzzleFocusPoint>();
            NumberWheelsPuzzleAuthoring authoring = root.GetComponent<NumberWheelsPuzzleAuthoring>();
            if (authoring == null) authoring = root.AddComponent<NumberWheelsPuzzleAuthoring>();
            authoring.SetCombination(normalized);

            var replacementModels = new Dictionary<string, GameObject>();
            foreach (ReplaceableModelSlot slot in root.GetComponentsInChildren<ReplaceableModelSlot>(true))
                if (slot.ModelPrefab != null) replacementModels[slot.gameObject.name] = slot.ModelPrefab;

            Transform previousGenerated = root.transform.Find("NumberWheels_Generated");
            if (previousGenerated != null) Object.DestroyImmediate(previousGenerated.gameObject);
            RemoveLegacyNumberWheelsChildren(root.transform);

            var generatedObject = new GameObject("NumberWheels_Generated");
            generatedObject.transform.SetParent(root.transform, false);
            Transform generated = generatedObject.transform;

            float panelWidth = CalculateNumberWheelsPanelWidth(normalized.Length);
            const float wheelSpacing = .48f;

            // The frame is replaceable independently of the controls. A suitcase, safe or padlock
            // model can therefore replace the demo panel without moving the interaction roots.
            GameObject housing = CreatePiece("CombinationLockHousing", generated,
                new Vector3(panelWidth, 1.35f, .12f), new Color(.075f, .085f, .12f), PieceCollider.None);
            housing.transform.localPosition = new Vector3(0f, 1.08f, .14f);
            RestoreReplacementModel(housing, replacementModels);

            TextMeshPro title = CreatePanelLabel(generated, "CombinationTitle", new Vector3(0f, 1.64f, -.005f),
                $"CODI DE {normalized.Length} XIFRES", 1.25f, new Color(.78f, .85f, .95f));
            title.rectTransform.sizeDelta = new Vector2(panelWidth * .92f, .4f);

            var puzzleSo = new SerializedObject(puzzle);
            SerializedProperty conditions = puzzleSo.FindProperty("_conditions");
            conditions.arraySize = normalized.Length;

            var wheelInputs = new List<NumberWheelInteractable>();
            for (int i = 0; i < normalized.Length; i++)
            {
                GameObject wheel = CreateNumberWheel("Wheel" + (i + 1), generated);
                wheel.transform.localPosition = new Vector3((i - (normalized.Length - 1) * .5f) * wheelSpacing, 1.12f, 0f);
                RestoreReplacementModel(wheel, replacementModels);
                SteppedPositioner positioner = wheel.GetComponent<SteppedPositioner>();
                wheelInputs.Add(wheel.GetComponent<NumberWheelInteractable>());
                conditions.GetArrayElementAtIndex(i).FindPropertyRelative("Positioner").objectReferenceValue = positioner;
                conditions.GetArrayElementAtIndex(i).FindPropertyRelative("RequiredIndex").intValue = normalized[i];
            }

            puzzleSo.ApplyModifiedProperties();

            // PC uses a centred in-world close-up. It renders the actual replaceable models rather
            // than a duplicate flat UI. PuzzleFocusPoint deliberately leaves VR in direct world use.
            GameObject cameraObject = new GameObject("NumberWheelsFocusCamera");
            cameraObject.transform.SetParent(generated, false);
            cameraObject.transform.localPosition = new Vector3(0f, 1.23f, -2.2f);
            cameraObject.transform.localRotation = Quaternion.identity;
            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = CalculateNumberWheelsFocusFov(normalized.Length);
            camera.nearClipPlane = .05f;
            cameraObject.SetActive(false);

            var focusSo = new SerializedObject(focus);
            focusSo.FindProperty("_focusCamera").objectReferenceValue = camera;
            focusSo.FindProperty("_puzzle").objectReferenceValue = puzzle;
            focusSo.ApplyModifiedProperties();

            foreach (NumberWheelInteractable wheelInput in wheelInputs)
            {
                var wheelSo = new SerializedObject(wheelInput);
                wheelSo.FindProperty("_focusPoint").objectReferenceValue = focus;
                wheelSo.ApplyModifiedProperties();
            }

            float openButtonWidth = Mathf.Clamp(panelWidth * .58f, 1.1f, 2.3f);
            GameObject openButton = CreatePiece("OpenCombinationPanel", generated,
                new Vector3(openButtonWidth, .18f, .1f), new Color(.16f, .34f, .52f));
            openButton.transform.localPosition = new Vector3(0f, .27f, -.08f);
            RestoreReplacementModel(openButton, replacementModels);
            MakeClickable(openButton, "Examinar combinació", focus.Enter);
            TextMeshPro openLabel = CreatePanelLabel(openButton.transform, "OpenLabel", new Vector3(0f, 0f, -.065f),
                "EXAMINAR CANDAU", .7f, Color.white);
            openLabel.rectTransform.sizeDelta = new Vector2(openButtonWidth * .92f, .35f);

            TextMeshPro help = CreatePanelLabel(generated, "FocusHelp", new Vector3(0f, .78f, -.01f),
                "W PUJA · S BAIXA · CLIC DRET SURT", .55f, new Color(.6f, .68f, .8f));
            help.rectTransform.sizeDelta = new Vector2(panelWidth * .9f, .4f);

            EditorUtility.SetDirty(authoring);
            EditorUtility.SetDirty(puzzle);
            EditorUtility.SetDirty(focus);
        }

        public static float CalculateNumberWheelsPanelWidth(int wheelCount)
        {
            int count = Mathf.Clamp(wheelCount, NumberWheelsPuzzleAuthoring.MinimumWheels,
                NumberWheelsPuzzleAuthoring.MaximumWheels);
            return Mathf.Max(1.38f, (count - 1) * .48f + .9f);
        }

        public static float CalculateNumberWheelsFocusFov(int wheelCount)
        {
            float paddedWidth = CalculateNumberWheelsPanelWidth(wheelCount) + .3f;
            const float cameraDistance = 2.2f;
            const float targetAspect = 16f / 9f;
            float verticalFov = 2f * Mathf.Atan(paddedWidth / (2f * cameraDistance * targetAspect)) * Mathf.Rad2Deg;
            return Mathf.Clamp(verticalFov, 42f, 68f);
        }

        private static int[] NormalizeNumberWheelsCombination(int[] combination)
        {
            int requested = combination != null ? combination.Length : NumberWheelsPuzzleAuthoring.MinimumWheels;
            int count = Mathf.Clamp(requested, NumberWheelsPuzzleAuthoring.MinimumWheels,
                NumberWheelsPuzzleAuthoring.MaximumWheels);
            var normalized = new int[count];
            for (int index = 0; index < count; index++)
                normalized[index] = Mathf.Clamp(combination != null && index < combination.Length ? combination[index] : 0, 0, 9);
            return normalized;
        }

        private static void RestoreReplacementModel(GameObject target, Dictionary<string, GameObject> replacements)
        {
            GameObject modelPrefab;
            if (!replacements.TryGetValue(target.name, out modelPrefab) || modelPrefab == null) return;
            ReplaceableModelSlot slot = target.GetComponent<ReplaceableModelSlot>();
            if (slot != null) slot.SetModel(modelPrefab);
        }

        private static void RemoveLegacyNumberWheelsChildren(Transform root)
        {
            for (int index = root.childCount - 1; index >= 0; index--)
            {
                Transform child = root.GetChild(index);
                string name = child.name;
                bool generated = name == "CombinationLockHousing_Logic" || name == "CombinationTitle" ||
                    name == "NumberWheelsFocusCamera" || name == "OpenCombinationPanel_Logic" ||
                    name == "FocusHelp" || (name.StartsWith("Wheel") && name.EndsWith("_Logic"));
                if (generated) Object.DestroyImmediate(child.gameObject);
            }
        }

        private static GameObject CreateNumberWheel(string name, Transform parent)
        {
            GameObject logic = new GameObject(name + "_Logic");
            logic.transform.SetParent(parent, false);

            GameObject visuals = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visuals.name = name + "_Visuals";
            visuals.transform.SetParent(logic.transform, false);
            visuals.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            visuals.transform.localScale = new Vector3(.22f, .11f, .22f);
            Object.DestroyImmediate(visuals.GetComponent<Collider>());
            Paint(visuals, new Color(.16f, .18f, .22f));

            var collider = logic.AddComponent<BoxCollider>();
            collider.size = new Vector3(.25f, .46f, .46f);

            var slot = logic.AddComponent<ReplaceableModelSlot>();
            var slotSo = new SerializedObject(slot);
            slotSo.FindProperty("_placeholderVisual").objectReferenceValue = visuals;
            slotSo.ApplyModifiedProperties();

            var positioner = logic.AddComponent<SteppedPositioner>();
            var posSo = new SerializedObject(positioner);
            SerializedProperty positions = posSo.FindProperty("_positions");
            positions.arraySize = 10;
            for (int digit = 0; digit < 10; digit++)
            {
                SerializedProperty position = positions.GetArrayElementAtIndex(digit);
                position.FindPropertyRelative("prompt").stringValue = "Avançar rodet";
                position.FindPropertyRelative("rotation").vector3Value = new Vector3(digit * 36f, 0f, 90f);
            }
            posSo.FindProperty("_visualTransform").objectReferenceValue = visuals.transform;
            posSo.ApplyModifiedProperties();

            var wheelInput = logic.AddComponent<NumberWheelInteractable>();
            AddVRBridge(logic, wheelInput);

            GameObject labelObject = new GameObject("DigitLabel", typeof(RectTransform));
            labelObject.transform.SetParent(logic.transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 0f, -.245f);
            labelObject.transform.localRotation = Quaternion.identity;
            var label = labelObject.AddComponent<TextMeshPro>();
            label.text = "0";
            label.fontSize = 3.2f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(.95f, .85f, .3f);
            label.rectTransform.sizeDelta = new Vector2(.34f, .46f);

            var view = logic.AddComponent<NumberWheelView>();
            var viewSo = new SerializedObject(view);
            viewSo.FindProperty("_digitLabel").objectReferenceValue = label;
            viewSo.ApplyModifiedProperties();

            return logic;
        }

        private static TextMeshPro CreatePanelLabel(Transform parent, string name, Vector3 localPosition,
            string text, float fontSize, Color color)
        {
            GameObject labelObject = new GameObject(name, typeof(RectTransform));
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = localPosition;
            labelObject.transform.localRotation = Quaternion.identity;
            var label = labelObject.AddComponent<TextMeshPro>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.color = color;
            label.rectTransform.sizeDelta = new Vector2(2.1f, .4f);
            return label;
        }

        // ── Multi-stage chain ───────────────────────────────────────────────────────────

        public static void CreateMultiStageChainPuzzle()
        {
            GameObject root = CreateMultiStageChainPuzzleKit("MultiStageChainPuzzle");
            Finalize(root, "Multi-Stage Chain Puzzle created and playable: solve the colour sequence, then set the three levers. "
                + "Only the child puzzle belonging to the active stage may advance the chain.");
        }

        internal static GameObject CreateMultiStageChainPuzzleKit(string name)
        {
            GameObject root = new GameObject(name);
            var parent = root.AddComponent<MultiStagePuzzle>();

            GameObject sequenceGroup = new GameObject("Stage01_Sequence");
            sequenceGroup.transform.SetParent(root.transform, false);
            var sequence = sequenceGroup.AddComponent<SequencePuzzle>();
            string[] steps = { "red", "green", "blue" };
            Color[] colors = { Color.red, Color.green, Color.blue };
            var sequenceSo = new SerializedObject(sequence);
            SerializedProperty correct = sequenceSo.FindProperty("_correctSequence");
            correct.arraySize = steps.Length;
            for (int i = 0; i < steps.Length; i++) correct.GetArrayElementAtIndex(i).stringValue = steps[i];
            sequenceSo.ApplyModifiedProperties();

            for (int i = 0; i < steps.Length; i++)
            {
                GameObject button = CreatePiece("Stage1_" + steps[i] + "Button", sequenceGroup.transform,
                    new Vector3(.38f, .18f, .38f), colors[i]);
                button.transform.localPosition = new Vector3(-.7f + i * .7f, 1.05f, 0f);
                var relay = button.AddComponent<SequenceStepButton>();
                var relaySo = new SerializedObject(relay);
                relaySo.FindProperty("_puzzle").objectReferenceValue = sequence;
                relaySo.FindProperty("_stepId").stringValue = steps[i];
                relaySo.ApplyModifiedProperties();
                MakeClickable(button, "Pulsar", relay.Press);
            }

            GameObject stateGroup = new GameObject("Stage02_Levers");
            stateGroup.transform.SetParent(root.transform, false);
            stateGroup.transform.localPosition = new Vector3(0f, 0f, .9f);
            var state = stateGroup.AddComponent<StatePuzzle>();
            int[] required = { 2, 0, 1 };
            var stateSo = new SerializedObject(state);
            SerializedProperty conditions = stateSo.FindProperty("_conditions");
            conditions.arraySize = required.Length;
            for (int i = 0; i < required.Length; i++)
            {
                GameObject lever = CreatePiece("Stage2_Lever" + (i + 1), stateGroup.transform,
                    new Vector3(.12f, .48f, .12f), new Color(.72f, .5f, .18f));
                lever.transform.localPosition = new Vector3(-.7f + i * .7f, 1.05f, 0f);
                var positioner = lever.AddComponent<SteppedPositioner>();
                var posSo = new SerializedObject(positioner);
                SerializedProperty positions = posSo.FindProperty("_positions");
                positions.arraySize = 3;
                for (int p = 0; p < 3; p++)
                {
                    SerializedProperty position = positions.GetArrayElementAtIndex(p);
                    position.FindPropertyRelative("prompt").stringValue = "Mover palanca";
                    position.FindPropertyRelative("rotation").vector3Value = new Vector3(-40f + 40f * p, 0f, 0f);
                }
                posSo.FindProperty("_visualTransform").objectReferenceValue = FindVisuals(lever.transform);
                posSo.ApplyModifiedProperties();
                var cycler = lever.AddComponent<InteractableCycler>();
                AddVRBridge(lever, cycler);
                conditions.GetArrayElementAtIndex(i).FindPropertyRelative("Positioner").objectReferenceValue = positioner;
                conditions.GetArrayElementAtIndex(i).FindPropertyRelative("RequiredIndex").intValue = required[i];
            }
            stateSo.ApplyModifiedProperties();

            var parentSo = new SerializedObject(parent);
            SerializedProperty stages = parentSo.FindProperty("_stages");
            stages.arraySize = 3;
            ConfigureStage(stages.GetArrayElementAtIndex(0), "sequence", false);
            ConfigureStage(stages.GetArrayElementAtIndex(1), "levers", false);
            ConfigureStage(stages.GetArrayElementAtIndex(2), "solved", true);
            parentSo.ApplyModifiedProperties();

            UnityEventTools.AddStringPersistentListener(sequence.OnSolvedEvent, parent.CompleteStage, "sequence");
            UnityEventTools.AddBoolPersistentListener(parent.GetStage(0).onExit, sequenceGroup.SetActive, false);
            UnityEventTools.AddBoolPersistentListener(parent.GetStage(1).onEnter, stateGroup.SetActive, true);
            UnityEventTools.AddBoolPersistentListener(parent.GetStage(1).onExit, stateGroup.SetActive, false);
            UnityEventTools.AddStringPersistentListener(state.OnSolvedEvent, parent.CompleteStage, "levers");
            stateGroup.SetActive(false);
            EditorUtility.SetDirty(parent);
            return root;
        }

        private static void ConfigureStage(SerializedProperty stage, string id, bool solved)
        {
            stage.FindPropertyRelative("id").stringValue = id;
            stage.FindPropertyRelative("isSolvedStage").boolValue = solved;
        }

        // ── Independent fail-state mechanics ────────────────────────────────────────────

        [MenuItem("Escape Room Framework/Create/Flow/Game Over Timer (HUD)", priority = 230)]
        public static void CreateGameOverTimer() => GameOverTimerWizard.Open();

        [MenuItem("Escape Room Framework/Create/Flow/Moving Hazard (Any Direction)", priority = 231)]
        public static void CreateMovingHazard() => MovingHazardWizard.Open();

        internal static void CreateConfiguredGameOverTimer(float duration, bool autoStart, bool showInHud,
            string hudLabel)
        {
            GameObject root = CreateRoot("GameOverTimer");
            var timer = root.AddComponent<GameOverTimer>();
            var so = new SerializedObject(timer);
            so.FindProperty("_duration").floatValue = Mathf.Max(.01f, duration);
            so.FindProperty("_autoStart").boolValue = autoStart;
            so.FindProperty("_showInHud").boolValue = showInHud;
            so.FindProperty("_hudLabel").stringValue = string.IsNullOrWhiteSpace(hudLabel) ? "TEMPS RESTANT" : hudLabel;
            so.ApplyModifiedProperties();
            Finalize(root, $"Independent Game Over Timer created: {duration:0.##} seconds, HUD {(showInHud ? "enabled" : "disabled")}. "
                + "Wire StartTimer to a trigger, puzzle, button or Timeline event.");
        }

        internal static void CreateConfiguredMovingHazard(Vector3 direction, float distance, float duration,
            Vector3 size, bool failAtDestination, bool failOnPlayerContact)
        {
            Vector3 normalizedDirection = direction.sqrMagnitude > .0001f ? direction.normalized : Vector3.down;
            GameObject root = CreateRoot("MovingHazard");

            GameObject start = new GameObject("StartPoint");
            start.transform.SetParent(root.transform, false);
            start.transform.localPosition = -normalizedDirection * Mathf.Max(.1f, distance) * .5f;
            GameObject end = new GameObject("EndPoint");
            end.transform.SetParent(root.transform, false);
            end.transform.localPosition = normalizedDirection * Mathf.Max(.1f, distance) * .5f;

            GameObject moving = CreatePiece("MovingHazardBody", root.transform, size,
                new Color(.5f, .08f, .08f), PieceCollider.Trigger);
            moving.transform.position = start.transform.position;
            var body = moving.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            var hazard = moving.AddComponent<MovingHazard>();
            var so = new SerializedObject(hazard);
            so.FindProperty("_startPoint").objectReferenceValue = start.transform;
            so.FindProperty("_endPoint").objectReferenceValue = end.transform;
            so.FindProperty("_travelDuration").floatValue = Mathf.Max(.01f, duration);
            so.FindProperty("_failAtDestination").boolValue = failAtDestination;
            so.FindProperty("_failOnPlayerContact").boolValue = failOnPlayerContact;
            so.ApplyModifiedProperties();

            Finalize(root, $"Independent Moving Hazard created: direction {normalizedDirection}, distance {distance:0.##} m, duration {duration:0.##} s. "
                + "Move StartPoint and EndPoint freely to author walls, ceilings, floors, platforms or water volumes.");
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

        /// <summary>What kind of interaction volume a piece needs on its logic root, if any.</summary>
        internal enum PieceCollider { None, Solid, Trigger }

        /// <summary>
        /// Builds the framework's standard piece: a '&lt;name&gt;_Logic' root carrying gameplay — the
        /// scripts, the interaction collider and a ReplaceableModelSlot — and a '&lt;name&gt;_Visuals'
        /// child carrying nothing but the placeholder mesh.
        ///
        /// The split is the whole point: every one of these cubes is a stand-in for a model that does
        /// not exist yet, and a designer must be able to drop that model in without inheriting a
        /// hierarchy of scripts, or hand-rewiring references, or discovering that deleting the
        /// placeholder also deleted the collider. Which is why the collider lives on the root and not
        /// on the art: the interaction volume is gameplay, and swapping the art must not take it away.
        ///
        /// The naming is a contract, not decoration — SteppedPositioner and InteractableToggle resolve
        /// their visual by looking for '&lt;name&gt;_Visuals'. Rename one, rename both.
        /// </summary>
        internal static GameObject CreatePiece(string baseName, Transform parent, Vector3 size, Color color,
            PieceCollider colliderKind = PieceCollider.Solid)
        {
            GameObject logic = new GameObject(baseName + "_Logic");
            logic.transform.SetParent(parent, false);

            GameObject visuals = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visuals.name = baseName + "_Visuals";
            visuals.transform.SetParent(logic.transform, false);
            visuals.transform.localScale = size;
            Object.DestroyImmediate(visuals.GetComponent<Collider>());
            Paint(visuals, color);

            if (colliderKind != PieceCollider.None)
            {
                var box = logic.AddComponent<BoxCollider>();
                box.size = size; // root stays at scale 1, so the collider matches the placeholder
                box.isTrigger = colliderKind == PieceCollider.Trigger;
            }

            var slot = logic.AddComponent<ReplaceableModelSlot>();
            var slotSo = new SerializedObject(slot);
            slotSo.FindProperty("_placeholderVisual").objectReferenceValue = visuals;
            slotSo.ApplyModifiedProperties();

            return logic;
        }

        /// <summary>The placeholder mesh of a piece built by CreatePiece, or the object itself for hand-built pieces that never got the split.</summary>
        internal static Transform FindVisuals(Transform logic)
        {
            Transform named = logic.Find(logic.name.Replace("_Logic", "") + "_Visuals");
            return named != null ? named : logic;
        }

        private static void Paint(GameObject target, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) return;
            var material = new Material(shader) { color = color };
            target.GetComponent<Renderer>().material = material;
        }

        /// <summary>Wires a click (PC and VR alike) to a void method, which is the part designers otherwise have to do by hand for every single piece.</summary>
        internal static void MakeClickable(GameObject target, string prompt, UnityEngine.Events.UnityAction action)
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

            // Searches children too: pieces built before the Logic/Visuals split keep their collider
            // on the art, and an XR interactable with no collider is silently ungrabbable in VR.
            Collider collider = target.GetComponentInChildren<Collider>();
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
                Collider collider = target.GetComponentInChildren<Collider>();
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
