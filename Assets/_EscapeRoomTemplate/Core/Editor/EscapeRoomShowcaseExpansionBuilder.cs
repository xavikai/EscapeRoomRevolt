using System.Collections.Generic;
using EscapeRoomRevolt.Systems.Flow;
using EscapeRoomRevolt.Systems.Hint;
using EscapeRoomRevolt.Systems.Interaction;
using EscapeRoomRevolt.Systems.Puzzle;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeRoomRevolt.EditorTools
{
    /// <summary>Idempotently adds the three advanced escape-room examples to ShowcaseMuseum.</summary>
    public static class EscapeRoomShowcaseExpansionBuilder
    {
        private const string ScenePath = "Assets/_EscapeRoomTemplate/Scenes/ShowcaseMuseum.unity";
        private const string AssetFolder = "Assets/_EscapeRoomTemplate/ScriptableObjects/Puzzles";
        private const string ExpansionRootName = "EscapeRoomExpansion_Rooms11_13";

        [MenuItem("Escape Room Framework/Demo/Add or Update Expansion Rooms", priority = 45)]
        public static void Build()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath)
            {
                EditorUtility.DisplayDialog("Escape Room Expansion",
                    "Open ShowcaseMuseum before adding the expansion rooms.", "OK");
                return;
            }

            ExtendShowcaseShell();

            GameObject previous = GameObject.Find(ExpansionRootName);
            if (previous != null) Object.DestroyImmediate(previous);

            var expansion = new GameObject(ExpansionRootName);
            BuildMultiStageRoom(expansion.transform);
            BuildHazardRoom(expansion.transform);
            BuildNumberWheelsRoom(expansion.transform);

            Selection.activeGameObject = expansion;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Escape Room Framework] Showcase expansion built: Room11 Multi-Stage, Room12 Timed Hazard, Room13 Number Wheels.", expansion);
        }

        private static void ExtendShowcaseShell()
        {
            Transform environment = GameObject.Find("Environment")?.transform;
            Transform corridor = environment != null ? environment.Find("Central_Corridor") : null;
            if (corridor != null)
            {
                corridor.position = new Vector3(0f, -.25f, 34f);
                corridor.localScale = new Vector3(4f, .5f, 72f);
            }

            Transform shell = GameObject.Find("Commercial_Atmosphere/Replaceable_PresentationShell")?.transform;
            if (shell == null) return;

            ExtendAlongZ(shell.Find("Wall_Left"), 34f, 72f);
            ExtendAlongZ(shell.Find("Wall_Right"), 34f, 72f);

            Transform finalWall = shell.Find("EndCap_Final");
            if (finalWall != null)
                finalWall.position = new Vector3(finalWall.position.x, finalWall.position.y, 70f);
        }

        private static void ExtendAlongZ(Transform target, float centerZ, float length)
        {
            if (target == null) return;
            target.position = new Vector3(target.position.x, target.position.y, centerZ);
            target.localScale = new Vector3(target.localScale.x, target.localScale.y, length);
        }

        private static void BuildMultiStageRoom(Transform parent)
        {
            Transform room = CreateRoomShell(parent, "Room11_MultiStageChain", new Vector3(-6f, 0f, 55f),
                new Color(.18f, .32f, .48f), "11 · PUZZLES ENCADENATS", "Resol-los tots · ordre opcional");

            GameObject kit = PuzzleCreator.CreateMultiStageChainPuzzleKit("MultiStageChain_Logic");
            kit.transform.SetParent(room, false);
            kit.transform.localPosition = new Vector3(0f, 0f, -.25f);
            var puzzle = kit.GetComponent<MultiStagePuzzle>();

            PuzzleDefinition definition = EnsureDefinition(
                "Def_demo_multistage_puzzle", "demo_multistage_chain", "Cadena de seguretat",
                PuzzleCategory.Sequence, "Resol la seqüència de colors i després configura les tres palanques.",
                new[]
                {
                    "Els colors s'han de prémer en un ordre concret.",
                    "L'ordre de la primera fase és vermell, verd i blau.",
                    "A la segona fase, deixa les palanques en les posicions 2, 0 i 1."
                });
            AssignDefinition(puzzle, definition);

            Door door = CreatePayoffDoor(room, "MultiStageExitDoor", new Vector3(0f, 1.25f, 2.18f), new Color(.12f, .5f, .68f));
            UnityEventTools.AddVoidPersistentListener(puzzle.OnSolvedEvent, door.Unlock);
            UnityEventTools.AddVoidPersistentListener(puzzle.OnSolvedEvent, door.ForceOpen);
            AddSolvedBeacon(room, puzzle, new Vector3(0f, 2.55f, 2.05f));
            EditorUtility.SetDirty(puzzle);
        }

        private static void BuildHazardRoom(Transform parent)
        {
            Transform room = CreateRoomShell(parent, "Room12_IndependentHazards", new Vector3(6f, 0f, 60f),
                new Color(.46f, .13f, .12f), "12 · INDEPENDENT HAZARDS", "Sostre mòbil · temporitzador HUD separat");

            GameObject start = new GameObject("MovingHazardStartPoint");
            start.transform.SetParent(room, false);
            start.transform.localPosition = new Vector3(0f, 2.35f, 0f);
            GameObject end = new GameObject("MovingHazardEndPoint");
            end.transform.SetParent(room, false);
            end.transform.localPosition = new Vector3(0f, .35f, 0f);

            GameObject ceiling = PuzzleCreator.CreatePiece("DescendingCeiling", room, new Vector3(4.35f, .2f, 4.35f),
                new Color(.6f, .055f, .04f), PuzzleCreator.PieceCollider.Trigger);
            var body = ceiling.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            var movingHazard = ceiling.AddComponent<MovingHazard>();
            var movingSo = new SerializedObject(movingHazard);
            movingSo.FindProperty("_startPoint").objectReferenceValue = start.transform;
            movingSo.FindProperty("_endPoint").objectReferenceValue = end.transform;
            movingSo.FindProperty("_travelDuration").floatValue = 20f;
            movingSo.FindProperty("_failAtDestination").boolValue = true;
            movingSo.FindProperty("_failOnPlayerContact").boolValue = true;
            movingSo.ApplyModifiedProperties();
            ceiling.transform.position = start.transform.position;

            GameObject timerObject = new GameObject("GameOverTimer_Logic");
            timerObject.transform.SetParent(room, false);
            var timer = timerObject.AddComponent<GameOverTimer>();
            var timerSo = new SerializedObject(timer);
            timerSo.FindProperty("_duration").floatValue = 25f;
            timerSo.FindProperty("_showInHud").boolValue = true;
            timerSo.FindProperty("_hudLabel").stringValue = "TEMPS DE LA SALA";
            timerSo.ApplyModifiedProperties();

            GameObject movingButton = PuzzleCreator.CreatePiece("StartMovingHazardButton", room,
                new Vector3(.55f, .18f, .55f), new Color(.85f, .55f, .08f));
            movingButton.transform.localPosition = new Vector3(-.85f, .22f, -1.55f);
            PuzzleCreator.MakeClickable(movingButton, "Iniciar sostre mòbil", movingHazard.StartHazard);

            GameObject timerButton = PuzzleCreator.CreatePiece("StartGameOverTimerButton", room,
                new Vector3(.55f, .18f, .55f), new Color(.72f, .25f, .12f));
            timerButton.transform.localPosition = new Vector3(.85f, .22f, -1.55f);
            PuzzleCreator.MakeClickable(timerButton, "Iniciar temporitzador", timer.StartTimer);

            GameObject timerZoneObject = new GameObject("StartGameOverTimerZone");
            timerZoneObject.transform.SetParent(room, false);
            timerZoneObject.transform.localPosition = new Vector3(0f, 1f, -2.65f);
            var timerZoneCollider = timerZoneObject.AddComponent<BoxCollider>();
            timerZoneCollider.isTrigger = true;
            timerZoneCollider.size = new Vector3(4.5f, 2f, .5f);
            var timerZone = timerZoneObject.AddComponent<EventTriggerZone>();
            UnityEventTools.AddVoidPersistentListener(timerZone.OnEntered, timer.StartTimer);

            CreateWorldText(room, "HazardInstructions", new Vector3(0f, .35f, 2.24f),
                "ESQUERRA: interruptor del sostre\nENTRADA O DRETA: temporitzador HUD", .56f, Color.white);
            EditorUtility.SetDirty(movingHazard);
            EditorUtility.SetDirty(timer);
        }

        private static void BuildNumberWheelsRoom(Transform parent)
        {
            Transform room = CreateRoomShell(parent, "Room13_NumberWheels", new Vector3(-6f, 0f, 65f),
                new Color(.32f, .24f, .42f), "13 · NUMBER WHEELS", "Quatre xifres · combinació 3142");

            GameObject kit = PuzzleCreator.CreateNumberWheelsPuzzleKit("NumberWheels_Logic", new[] { 3, 1, 4, 2 });
            kit.transform.SetParent(room, false);
            // A compact wall-mounted lock beside the exit. The focus camera is a child of the kit,
            // so the close-up remains comfortably large while the object reads at suitcase scale in-world.
            kit.transform.localPosition = new Vector3(-1.35f, .72f, 2.1f);
            kit.transform.localScale = Vector3.one * .48f;
            var puzzle = kit.GetComponent<StatePuzzle>();

            PuzzleDefinition definition = EnsureDefinition(
                "Def_demo_number_wheels_puzzle", "demo_number_wheels", "Candau de rodets",
                PuzzleCategory.Code, "Gira els quatre rodets fins a introduir la combinació correcta.",
                new[]
                {
                    "Cada rodet avança una xifra quan hi interactues.",
                    "Busca una combinació de quatre xifres.",
                    "La combinació de demostració és 3142."
                });
            AssignDefinition(puzzle, definition);

            Door door = CreatePayoffDoor(room, "NumberWheelsExitDoor", new Vector3(0f, 1.25f, 2.18f), new Color(.45f, .22f, .62f));
            UnityEventTools.AddVoidPersistentListener(puzzle.OnSolvedEvent, door.Unlock);
            UnityEventTools.AddVoidPersistentListener(puzzle.OnSolvedEvent, door.ForceOpen);
            AddSolvedBeacon(room, puzzle, new Vector3(0f, 2.55f, 2.05f));
            EditorUtility.SetDirty(puzzle);
        }

        private static Transform CreateRoomShell(Transform parent, string name, Vector3 position, Color color,
            string title, string subtitle)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.position = position;

            // Match the original museum platforms exactly: the 8x8 floor meets the central
            // corridor at x +/-2 and keeps every new room on the same walkable y=0 plane.
            CreateBlock(root.transform, "Floor", new Vector3(0f, -.25f, 0f), new Vector3(8f, .5f, 8f), color * .55f);
            CreateBlock(root.transform, "BackWall", new Vector3(0f, 1.35f, 2.42f), new Vector3(5f, 2.7f, .16f), color * .72f);
            CreateBlock(root.transform, "LeftRail", new Vector3(-2.42f, .55f, 0f), new Vector3(.16f, 1.1f, 5f), color * .62f);
            CreateBlock(root.transform, "RightRail", new Vector3(2.42f, .55f, 0f), new Vector3(.16f, 1.1f, 5f), color * .62f);

            CreateWorldText(root.transform, "RoomTitle", new Vector3(0f, 2.3f, 2.31f), title, .65f, Color.white);
            CreateWorldText(root.transform, "RoomSubtitle", new Vector3(0f, 1.9f, 2.3f), subtitle, .38f, new Color(.8f, .88f, 1f));

            string roomNumber = title.Split('·')[0].Trim();
            CreateFloorText(root.transform, "FloorRoomNumber", new Vector3(0f, .01f, 0f),
                roomNumber, 200, .1f);
            CreateFloorText(root.transform, "FloorRoomDescription", new Vector3(0f, .01f, -2.5f),
                subtitle, 100, .05f);

            GameObject lightObject = new GameObject("RoomLight");
            lightObject.transform.SetParent(root.transform, false);
            lightObject.transform.localPosition = new Vector3(0f, 2.8f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = Color.Lerp(Color.white, color, .35f);
            light.intensity = 4.5f;
            light.range = 7f;
            return root.transform;
        }

        private static TextMesh CreateFloorText(Transform parent, string name, Vector3 localPosition,
            string text, int fontSize, float characterSize)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            textObject.transform.localPosition = localPosition;
            textObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            var label = textObject.AddComponent<TextMesh>();
            label.text = text;
            label.fontSize = fontSize;
            label.characterSize = characterSize;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = new Color(1f, 1f, 1f, .5f);
            return label;
        }

        private static Door CreatePayoffDoor(Transform parent, string name, Vector3 localPosition, Color color)
        {
            GameObject doorObject = PuzzleCreator.CreatePiece(name, parent, new Vector3(1.35f, 2.5f, .18f), color);
            doorObject.transform.localPosition = localPosition;
            var door = doorObject.AddComponent<Door>();
            var so = new SerializedObject(door);
            so.FindProperty("_isLocked").boolValue = true;
            so.FindProperty("_movementType").enumValueIndex = (int)DoorMovementType.Slide;
            so.FindProperty("_slideOffset").vector3Value = new Vector3(0f, 2.7f, 0f);
            so.FindProperty("_openDuration").floatValue = 1.2f;
            so.ApplyModifiedProperties();
            return door;
        }

        private static void AddSolvedBeacon(Transform room, PuzzleController puzzle, Vector3 position)
        {
            GameObject beacon = CreateBlock(room, "SolvedBeacon", position, new Vector3(.35f, .35f, .35f), new Color(.1f, 1f, .3f));
            beacon.SetActive(false);
            UnityEventTools.AddBoolPersistentListener(puzzle.OnSolvedEvent, beacon.SetActive, true);
        }

        private static GameObject CreateBlock(Transform parent, string name, Vector3 localPosition, Vector3 scale, Color color)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent, false);
            block.transform.localPosition = localPosition;
            block.transform.localScale = scale;
            Paint(block, color);
            return block;
        }

        private static TextMeshPro CreateWorldText(Transform parent, string name, Vector3 localPosition,
            string text, float fontSize, Color color)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            textObject.transform.localPosition = localPosition;
            textObject.transform.localRotation = Quaternion.identity;
            var label = textObject.AddComponent<TextMeshPro>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.color = color;
            label.rectTransform.sizeDelta = new Vector2(4.5f, 1f);
            return label;
        }

        private static void Paint(GameObject target, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) return;
            target.GetComponent<Renderer>().sharedMaterial = new Material(shader) { color = color };
        }

        private static PuzzleDefinition EnsureDefinition(string assetName, string persistentId, string displayName,
            PuzzleCategory category, string objective, string[] hints)
        {
            EnsureFolder("Assets/_EscapeRoomTemplate/ScriptableObjects");
            EnsureFolder(AssetFolder);
            string hintPath = AssetFolder + "/Hint_" + assetName.Substring(4) + ".asset";
            HintData hintData = AssetDatabase.LoadAssetAtPath<HintData>(hintPath);
            if (hintData == null)
            {
                hintData = ScriptableObject.CreateInstance<HintData>();
                AssetDatabase.CreateAsset(hintData, hintPath);
            }
            hintData.delayBeforeFirstHint = 45f;
            hintData.delayBetweenHints = 35f;
            hintData.hints = new List<HintEntry>();
            foreach (string text in hints) hintData.hints.Add(new HintEntry { hintText = text });
            EditorUtility.SetDirty(hintData);

            string definitionPath = AssetFolder + "/" + assetName + ".asset";
            PuzzleDefinition definition = AssetDatabase.LoadAssetAtPath<PuzzleDefinition>(definitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<PuzzleDefinition>();
                AssetDatabase.CreateAsset(definition, definitionPath);
            }
            var so = new SerializedObject(definition);
            so.FindProperty("_persistentId").stringValue = persistentId;
            so.FindProperty("_displayName").stringValue = displayName;
            so.FindProperty("_category").enumValueIndex = (int)category;
            so.FindProperty("_objective").stringValue = objective;
            so.FindProperty("_hints").objectReferenceValue = hintData;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void AssignDefinition(PuzzleController puzzle, PuzzleDefinition definition)
        {
            var so = new SerializedObject(puzzle);
            so.FindProperty("_definition").objectReferenceValue = definition;
            so.ApplyModifiedProperties();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int separator = path.LastIndexOf('/');
            string parent = path.Substring(0, separator);
            string child = path.Substring(separator + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, child);
        }
    }
}
