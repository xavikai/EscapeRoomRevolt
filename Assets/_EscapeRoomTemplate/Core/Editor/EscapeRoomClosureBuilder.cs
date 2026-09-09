using System;
using System.Collections.Generic;
using System.Linq;
using EscapeRoomRevolt.Systems.Hint;
using EscapeRoomRevolt.Systems.Interaction;
using EscapeRoomRevolt.Systems.Puzzle;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace EscapeRoomRevolt.EditorTools
{
    /// <summary>
    /// Idempotent finalisation pass for the two Escape Room sample scenes. It closes content gaps
    /// found by the audit without rebuilding either scene or changing serialized gameplay IDs.
    /// </summary>
    public static class EscapeRoomClosureBuilder
    {
        private const string ShowcasePath = "Assets/_EscapeRoomTemplate/Scenes/ShowcaseMuseum.unity";
        private const string OfficePath = "Assets/_EscapeRoomTemplate/Scenes/LockedOffice.unity";
        private const string PuzzleAssetFolder = "Assets/_EscapeRoomTemplate/ScriptableObjects/Puzzles";

        [MenuItem("Escape Room Framework/Demo/Apply Escape Room Closure Fixes", priority = 46)]
        public static void Apply()
        {
            Scene previousScene = SceneManager.GetActiveScene();
            string previousPath = previousScene.path;
            EditorSceneManager.SaveOpenScenes();

            ApplyShowcaseMuseum();
            ApplyLockedOffice();

            AssetDatabase.SaveAssets();
            if (!string.IsNullOrEmpty(previousPath)) EditorSceneManager.OpenScene(previousPath, OpenSceneMode.Single);
            else EditorSceneManager.OpenScene(ShowcasePath, OpenSceneMode.Single);

            Debug.Log("[Escape Room Framework] Closure fixes applied: definitions, Pipe payoff, semantic names and Sliding prompts.");
        }

        private static void ApplyShowcaseMuseum()
        {
            Scene scene = EditorSceneManager.OpenScene(ShowcasePath, OpenSceneMode.Single);

            AssignDefinition(FindNamed<ThrowPuzzle>("ThrowPuzzleController"), EnsureDefinition(
                "Def_demo_throw_puzzle", "demo_throw_targets", "Galeria de precisió",
                PuzzleCategory.Observation, "Encerta les tres dianes amb objectes llançables.",
                "Els objectes físics només compten si colpegen una diana.",
                "Cada diana només s'ha d'activar una vegada.",
                "Encerta target1, target2 i target3 en qualsevol ordre."));

            AssignDefinition(FindNamed<SequencePuzzle>("MelodyPuzzleController"), EnsureDefinition(
                "Def_demo_melody_puzzle", "demo_melody_sequence", "Seqüència melòdica",
                PuzzleCategory.Sequence, "Escolta la pista i repeteix les quatre notes en el mateix ordre.",
                "La melodia conté quatre notes.",
                "Compara cada botó amb la pista abans d'introduir la seqüència.",
                "La seqüència de demostració és C, A, D, B."));

            // These are implementation stages of the parent puzzle, but explicit definitions keep
            // commercial validation and SaveIds independent from GameObject names.
            AssignDefinition(FindNamed<SequencePuzzle>("Stage01_Sequence"), EnsureDefinition(
                "Def_demo_multistage_sequence", "demo_multistage_sequence_stage", "Fase interna: seqüència",
                PuzzleCategory.Sequence, "Completa la seqüència de colors de la primera fase.",
                "Aquesta és la primera fase de la cadena.", "Prem els colors en ordre.",
                "L'ordre és vermell, verd i blau."));
            AssignDefinition(FindNamed<StatePuzzle>("Stage02_Levers"), EnsureDefinition(
                "Def_demo_multistage_levers", "demo_multistage_levers_stage", "Fase interna: palanques",
                PuzzleCategory.State, "Configura les tres palanques de la segona fase.",
                "Aquesta fase s'activa després de la seqüència.", "Cada palanca té tres posicions.",
                "Les posicions són 2, 0 i 1."));

            EnsurePipePayoff();
            UnifySlidingPrompts();
            RenameShowcaseObjects();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void ApplyLockedOffice()
        {
            Scene scene = EditorSceneManager.OpenScene(OfficePath, OpenSceneMode.Single);

            RenameNumbered("NewDoor_Logic", "OfficeDoor");
            RenameNumbered("NewDrawer_Logic", "OfficeDrawer");
            RenameNumbered("NewCabinet_Logic", "OfficeCabinet");
            RenameNumbered("NewSafe_Logic", "OfficeSafe");
            RenameNumbered("NewKeypad_Logic", "OfficeKeypad");
            RenameNumbered("NewItemReceiver_Logic", "OfficeItemReceiver");
            RenameNumbered("NewPickableItem_Logic", "OfficeClueItem");
            RenameNumbered("NewNote_Logic", "OfficeNote");
            RenameNumbered("NewNarrativeTrigger", "OfficeNarrativeTrigger", suffix: string.Empty);
            RenameVisualChildren();
            RenameRemainingNewObjects("Office");
            RenameCubes("OfficeArchitecture");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsurePipePayoff()
        {
            PipePuzzle puzzle = FindNamed<PipePuzzle>("PipePuzzleController");
            if (puzzle == null) return;

            Transform room = puzzle.transform.parent != null ? puzzle.transform.parent : puzzle.transform;
            Transform existingDoor = room.Find("PipeExitDoor_Logic");
            Door door;
            if (existingDoor != null)
            {
                door = existingDoor.GetComponent<Door>();
            }
            else
            {
                GameObject doorObject = PuzzleCreator.CreatePiece("PipeExitDoor", room,
                    new Vector3(1.35f, 2.5f, .18f), new Color(.12f, .58f, .42f));
                doorObject.transform.localPosition = new Vector3(0f, 1.25f, 2.18f);
                door = doorObject.AddComponent<Door>();
                var doorSo = new SerializedObject(door);
                doorSo.FindProperty("_saveId").stringValue = "demo_pipe_exit_door";
                doorSo.FindProperty("_isLocked").boolValue = true;
                doorSo.FindProperty("_movementType").enumValueIndex = (int)DoorMovementType.Slide;
                doorSo.FindProperty("_slideOffset").vector3Value = new Vector3(0f, 2.7f, 0f);
                doorSo.FindProperty("_openDuration").floatValue = 1.2f;
                doorSo.ApplyModifiedProperties();
            }

            Transform beaconTransform = room.Find("PipeSolvedBeacon");
            GameObject beacon;
            if (beaconTransform != null)
            {
                beacon = beaconTransform.gameObject;
            }
            else
            {
                beacon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                beacon.name = "PipeSolvedBeacon";
                beacon.transform.SetParent(room, false);
                beacon.transform.localPosition = new Vector3(0f, 2.55f, 2.05f);
                beacon.transform.localScale = Vector3.one * .35f;
                UnityEngine.Object.DestroyImmediate(beacon.GetComponent<Collider>());
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader != null) beacon.GetComponent<Renderer>().sharedMaterial = new Material(shader) { color = new Color(.1f, 1f, .3f) };
                beacon.SetActive(false);
            }

            AddListenerOnce(puzzle.OnSolvedEvent, door, "Unlock", () => UnityEventTools.AddVoidPersistentListener(puzzle.OnSolvedEvent, door.Unlock));
            AddListenerOnce(puzzle.OnSolvedEvent, door, "ForceOpen", () => UnityEventTools.AddVoidPersistentListener(puzzle.OnSolvedEvent, door.ForceOpen));
            AddListenerOnce(puzzle.OnSolvedEvent, beacon, "SetActive", () => UnityEventTools.AddBoolPersistentListener(puzzle.OnSolvedEvent, beacon.SetActive, true));
            EditorUtility.SetDirty(puzzle);
        }

        private static void UnifySlidingPrompts()
        {
            foreach (SlidingTileButton tile in UnityEngine.Object.FindObjectsByType<SlidingTileButton>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                InteractableTrigger trigger = tile.GetComponent<InteractableTrigger>();
                if (trigger == null) continue;
                var so = new SerializedObject(trigger);
                so.FindProperty("_prompt").stringValue = "Moure peça";
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(trigger);
            }
        }

        private static void RenameShowcaseObjects()
        {
            RenameUnique("Room3_Note", "Room02_NoteAndCode");
            RenameUnique("Room6_Receiver", "Room03_Receivers");
            RenameUnique("Room9_Sequence", "Room04_Sequence");
            RenameUnique("Room10_State", "Room05_State");
            RenameUnique("Room13_Physics", "Room06_Physics");
            RenameUnique("Room13_HintTest", "Room04_SequenceHints");
            RenameUnique("Room14_PlacementPuzzleTest", "Room07_PlacementPuzzle");
            RenameUnique("Room15_SlidingPuzzleTest", "Room08_SlidingPuzzle");
            RenameUnique("Room9_Melody", "Room09_MelodyPuzzle");
            RenameUnique("Room17_PipePuzzleTest", "Room10_PipePuzzle");
            RenameUnique("PhysicsPuzzle_Logic", "Room06_ThrowPuzzle_Logic");

            RenameNumbered("NewDoor_Logic", "MuseumDoor");
            RenameNumbered("NewLever_Logic", "StateLever");
            RenameNumbered("NewSwitch_Logic", "StateSwitch");
            RenameVisualChildren();
            RenameRemainingNewObjects("Museum");
            RenameCubes("MuseumArchitecture");
        }

        private static T FindNamed<T>(string objectName) where T : Component
        {
            return UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(component => component.name == objectName);
        }

        private static void RenameUnique(string oldName, string newName)
        {
            Transform target = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(transform => transform.name == oldName);
            if (target != null) target.name = newName;
        }

        private static void RenameNumbered(string oldName, string prefix, string suffix = "_Logic")
        {
            List<Transform> targets = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(transform => transform.name == oldName)
                .OrderBy(transform => transform.position.z)
                .ThenBy(transform => transform.position.x)
                .ThenBy(transform => transform.position.y)
                .ToList();
            for (int i = 0; i < targets.Count; i++) targets[i].name = prefix + "_" + (i + 1).ToString("00") + suffix;
        }

        private static void RenameVisualChildren()
        {
            foreach (Transform child in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (child.parent == null || !child.name.StartsWith("New", StringComparison.Ordinal) || !child.name.EndsWith("_Visuals", StringComparison.Ordinal)) continue;
                string parentName = child.parent.name.EndsWith("_Logic", StringComparison.Ordinal)
                    ? child.parent.name.Substring(0, child.parent.name.Length - "_Logic".Length)
                    : child.parent.name;
                child.name = parentName + "_Visuals";
            }
        }

        private static void RenameRemainingNewObjects(string scenePrefix)
        {
            List<Transform> targets = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(transform => transform.name.StartsWith("New", StringComparison.Ordinal))
                .OrderBy(transform => BuildPath(transform))
                .ToList();
            for (int i = 0; i < targets.Count; i++)
            {
                string baseName = targets[i].name.Substring(3).TrimStart('_');
                targets[i].name = scenePrefix + "_" + baseName + "_" + (i + 1).ToString("00");
            }
        }

        private static void RenameCubes(string prefix)
        {
            List<Transform> cubes = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(transform => transform.name == "Cube")
                .OrderBy(transform => transform.position.z)
                .ThenBy(transform => transform.position.x)
                .ToList();
            int wall = 0, platform = 0;
            foreach (Transform cube in cubes)
            {
                if (cube.lossyScale.y > 1.1f) cube.name = prefix + "_Wall_" + (++wall).ToString("00");
                else cube.name = prefix + "_Platform_" + (++platform).ToString("00");
            }
        }

        private static string BuildPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }

        private static PuzzleDefinition EnsureDefinition(string assetName, string persistentId, string displayName,
            PuzzleCategory category, string objective, params string[] hints)
        {
            EnsureFolder("Assets/_EscapeRoomTemplate/ScriptableObjects");
            EnsureFolder(PuzzleAssetFolder);

            string hintPath = PuzzleAssetFolder + "/Hint_" + assetName.Substring(4) + ".asset";
            HintData hintData = AssetDatabase.LoadAssetAtPath<HintData>(hintPath);
            if (hintData == null)
            {
                hintData = ScriptableObject.CreateInstance<HintData>();
                AssetDatabase.CreateAsset(hintData, hintPath);
            }
            hintData.delayBeforeFirstHint = 45f;
            hintData.delayBetweenHints = 35f;
            hintData.hints = hints.Select(text => new HintEntry { hintText = text }).ToList();
            EditorUtility.SetDirty(hintData);

            string definitionPath = PuzzleAssetFolder + "/" + assetName + ".asset";
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
            if (puzzle == null || definition == null) return;
            var so = new SerializedObject(puzzle);
            so.FindProperty("_definition").objectReferenceValue = definition;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(puzzle);
        }

        private static void AddListenerOnce(UnityEventBase unityEvent, UnityEngine.Object target, string methodName, Action add)
        {
            for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++)
                if (unityEvent.GetPersistentTarget(i) == target && unityEvent.GetPersistentMethodName(i) == methodName) return;
            add();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int separator = path.LastIndexOf('/');
            string parent = path.Substring(0, separator);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(separator + 1));
        }
    }
}
