using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Player.PC;
using EscapeRoomRevolt.UI.PC;
using EscapeRoomRevolt.Systems.Interaction;
using EscapeRoomRevolt.Systems.Inventory;
using EscapeRoomRevolt.Systems.Puzzle;

namespace EscapeRoomRevolt.EditorTools
{
    public class TemplateSceneBuilder
    {
        [MenuItem("EscapeRoom/3. Demo/Build Minimal Template Scene", priority = 32)]
        public static void CreateMinimalScene()
        {
            if (!EditorUtility.DisplayDialog("Crear Minimal Scene", "Això crearà i sobreescriurà l'escena 'MinimalRoom'. Vols continuar?", "Sí", "Cancel·la"))
                return;

            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            SetupCorePrefabs();
            
            // Basic Floor
            GameObject env = new GameObject("Environment");
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(env.transform);
            floor.transform.localScale = new Vector3(10, 0.5f, 10);
            floor.transform.position = new Vector3(0, -0.25f, 0);
            AssignURPMaterial(floor, Color.gray);

            SaveScene(newScene, "Assets/_EscapeRoomTemplate/Scenes/MinimalRoom.unity");
        }

        [MenuItem("EscapeRoom/3. Demo/Build Showcase Museum Scene", priority = 33)]
        public static void CreateMuseumScene()
        {
            if (!EditorUtility.DisplayDialog("Crear Museum Scene", "Això crearà i sobreescriurà l'escena 'ShowcaseMuseum'. Vols continuar?", "Sí", "Cancel·la"))
                return;

            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            SetupCorePrefabs();

            GameObject env = new GameObject("Environment");

            // 1. Central Corridor
            GameObject corridor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            corridor.name = "Central_Corridor";
            corridor.transform.SetParent(env.transform);
            corridor.transform.localScale = new Vector3(4, 0.5f, 40);
            corridor.transform.position = new Vector3(0, -0.25f, 18);
            AssignURPMaterial(corridor, new Color(0.2f, 0.2f, 0.2f));

            // Room 1: Basic Interaction (Drawers)
            CreatePlatform(env, "Room1_Interaction", new Vector3(-6, 0, 5), new Color(0.3f, 0.4f, 0.3f));
            CreateDrawerPuzzle(new Vector3(-6, 0, 5));

            // Room 2: Keys & Locks
            CreatePlatform(env, "Room2_Locks", new Vector3(6, 0, 10), new Color(0.4f, 0.3f, 0.3f));
            CreateKeyAndLockPuzzle(new Vector3(6, 0, 10));

            // Room 3: Lore & Notes
            CreatePlatform(env, "Room3_Lore", new Vector3(-6, 0, 15), new Color(0.3f, 0.3f, 0.4f));
            CreateLorePuzzle(new Vector3(-6, 0, 15));

            // Room 4: 3D Examination
            CreatePlatform(env, "Room4_Examination", new Vector3(6, 0, 20), new Color(0.4f, 0.4f, 0.3f));
            // Placeholder for Examine Chamber (Requires Prefab setup later)

            // Room 5: Code Panel
            CreatePlatform(env, "Room5_Keypad", new Vector3(-6, 0, 25), new Color(0.4f, 0.3f, 0.4f));
            CreateKeypadPuzzle(new Vector3(-6, 0, 25));

            // Room 6: Narrative Audio
            CreatePlatform(env, "Room6_Narrative", new Vector3(6, 0, 30), new Color(0.3f, 0.4f, 0.4f));
            CreateNarrativePuzzle(new Vector3(6, 0, 30));

            // Room 7: Item Receiver
            CreatePlatform(env, "Room7_Receiver", new Vector3(-6, 0, 35), new Color(0.4f, 0.4f, 0.4f));
            // Placeholder for Item Receiver (Requires Prefab setup later)

            SaveScene(newScene, "Assets/_EscapeRoomTemplate/Scenes/ShowcaseMuseum.unity");
        }

        private static void SetupCorePrefabs()
        {
            // Remove default camera
            GameObject defaultCam = GameObject.Find("Main Camera");
            if (defaultCam != null && defaultCam.transform.parent == null)
                GameObject.DestroyImmediate(defaultCam);

            InstantiatePrefab("Assets/_EscapeRoomTemplate/Prefabs/GameManager.prefab", Vector3.zero);
            InstantiatePrefab("Assets/_EscapeRoomTemplate/Prefabs/UI_Canvas.prefab", Vector3.zero);
            
            GameObject player = InstantiatePrefab("Assets/_EscapeRoomTemplate/Prefabs/Player_PC.prefab", new Vector3(0, 0.5f, 0));
            if (player != null)
            {
                var interactionManager = player.GetComponentInChildren<InteractionManager>();
                if (interactionManager != null)
                {
                    SerializedObject soIM = new SerializedObject(interactionManager);
                    soIM.FindProperty("_interactableLayer").intValue = ~0; // Everything
                    soIM.ApplyModifiedProperties();
                }
            }
        }

        private static GameObject InstantiatePrefab(string path, Vector3 position)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.position = position;
                return instance;
            }
            Debug.LogWarning($"[TemplateSceneBuilder] Could not find prefab at: {path}");
            return null;
        }

        private static void CreatePlatform(GameObject env, string name, Vector3 position, Color color)
        {
            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = name;
            platform.transform.SetParent(env.transform);
            platform.transform.localScale = new Vector3(8, 0.5f, 8);
            platform.transform.position = new Vector3(position.x, -0.25f, position.z);
            AssignURPMaterial(platform, color);
        }

        private static void AssignURPMaterial(GameObject obj, Color color)
        {
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader != null)
            {
                Material mat = new Material(urpShader);
                mat.color = color;
                obj.GetComponent<Renderer>().material = mat;
            }
        }

        private static GameObject CreateInteractableObject(string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject logicObj = new GameObject(name + "_Logic");
            logicObj.transform.position = position;
            logicObj.transform.localScale = Vector3.one;
            
            GameObject visualObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visualObj.name = name + "_Visuals";
            visualObj.transform.SetParent(logicObj.transform);
            visualObj.transform.localPosition = Vector3.zero;
            visualObj.transform.localRotation = Quaternion.identity;
            visualObj.transform.localScale = scale;

            AssignURPMaterial(visualObj, color);
            return logicObj;
        }

        private static void SaveScene(Scene scene, string path)
        {
            if (!AssetDatabase.IsValidFolder("Assets/_EscapeRoomTemplate/Scenes"))
                AssetDatabase.CreateFolder("Assets/_EscapeRoomTemplate", "Scenes");

            EditorSceneManager.SaveScene(scene, path);
            Debug.Log($"[EscapeRoom] Escena guardada a: {path}");
        }

        // --- PUZZLE GENERATORS ---

        private static void CreateDrawerPuzzle(Vector3 center)
        {
            GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "Table";
            table.transform.position = center + new Vector3(0, 0.5f, 0);
            table.transform.localScale = new Vector3(2, 1, 1);
            AssignURPMaterial(table, new Color(0.6f, 0.3f, 0.1f));

            GameObject drawer = CreateInteractableObject("Drawer", center + new Vector3(0, 0.8f, -0.5f), new Vector3(0.8f, 0.3f, 0.8f), new Color(0.5f, 0.2f, 0.1f));
            var doorScript = drawer.AddComponent<Door>();
            SerializedObject soDoor = new SerializedObject(doorScript);
            soDoor.FindProperty("_movementType").enumValueIndex = (int)DoorMovementType.Slide;
            soDoor.FindProperty("_slideOffset").vector3Value = new Vector3(0, 0, -0.5f);
            soDoor.FindProperty("_interactionPrompt").stringValue = "[E] Open Drawer";
            soDoor.ApplyModifiedProperties();
        }

        private static void CreateKeyAndLockPuzzle(Vector3 center)
        {
            // Wall for Door
            GameObject wallL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallL.transform.position = center + new Vector3(-1.5f, 1.5f, 2);
            wallL.transform.localScale = new Vector3(1, 3, 0.5f);
            
            GameObject wallR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallR.transform.position = center + new Vector3(1.5f, 1.5f, 2);
            wallR.transform.localScale = new Vector3(1, 3, 0.5f);

            GameObject wallT = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallT.transform.position = center + new Vector3(0, 2.75f, 2);
            wallT.transform.localScale = new Vector3(2, 0.5f, 0.5f);

            // Locked Door
            GameObject door = CreateInteractableObject("LockedDoor", center + new Vector3(0, 1.25f, 2), new Vector3(1.5f, 2.5f, 0.2f), new Color(0.8f, 0.2f, 0.2f));
            GameObject hinge = new GameObject("CustomPivot");
            hinge.transform.SetParent(door.transform);
            hinge.transform.localPosition = new Vector3(-0.75f, 0, 0);
            
            var doorScript = door.AddComponent<Door>();
            SerializedObject soDoor = new SerializedObject(doorScript);
            soDoor.FindProperty("_isLocked").boolValue = true;
            soDoor.FindProperty("_requiredItemId").stringValue = "key_museum_01";
            soDoor.FindProperty("_customPivot").objectReferenceValue = hinge.transform;
            soDoor.FindProperty("_interactionPrompt").stringValue = "[E] Open Door";
            soDoor.ApplyModifiedProperties();

            // Key
            InventoryItemData keyData = AssetDatabase.LoadAssetAtPath<InventoryItemData>("Assets/_EscapeRoomTemplate/ScriptableObjects/Items/Key_Office.asset");
            if (keyData != null)
            {
                GameObject keyObj = CreateInteractableObject("Key_Museum", center + new Vector3(0, 0.2f, 0), new Vector3(0.2f, 0.1f, 0.2f), Color.yellow);
                var pickable = keyObj.AddComponent<PickableItem>();
                SerializedObject soKey = new SerializedObject(pickable);
                soKey.FindProperty("_itemData").objectReferenceValue = keyData;
                soKey.FindProperty("_destroyOnPickup").boolValue = true;
                soKey.FindProperty("_interactionPrompt").stringValue = "[E] Pick up Key";
                soKey.ApplyModifiedProperties();
                
                // Override required item id to match the office key so it works out of the box
                soDoor.FindProperty("_requiredItemId").stringValue = keyData.ItemId;
                soDoor.ApplyModifiedProperties();
            }
        }

        private static void CreateLorePuzzle(Vector3 center)
        {
            GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.transform.position = center + new Vector3(0, 0.5f, 0);
            table.transform.localScale = new Vector3(2, 1, 1);

            GameObject noteObj = CreateInteractableObject("ClueNote", center + new Vector3(0, 1.05f, 0), new Vector3(0.4f, 0.05f, 0.4f), Color.white);
            var note = noteObj.AddComponent<InteractableNote>();
            SerializedObject soNote = new SerializedObject(note);
            soNote.FindProperty("NoteContent").stringValue = "Welcome to the Escape Room Museum!\n\nEach room here tests a different mechanic of the framework.\n\nEnjoy your stay.";
            soNote.FindProperty("_interactionPrompt").stringValue = "[E] Read Note";
            soNote.ApplyModifiedProperties();
        }

        private static void CreateKeypadPuzzle(Vector3 center)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = center + new Vector3(0, 1.5f, 2);
            wall.transform.localScale = new Vector3(3, 3, 0.5f);

            EscapeRoomRevolt.EditorTools.InteractableCreator.CreateKeypadPanel();
            GameObject keypadObj = UnityEditor.Selection.activeGameObject;
            if (keypadObj != null)
            {
                keypadObj.name = "Keypad_Safe";
                keypadObj.transform.position = center + new Vector3(0, 1.5f, 1.74f);
                
                var codePuzzle = keypadObj.GetComponent<CodePanelPuzzle>();
                if (codePuzzle != null)
                {
                    SerializedObject soCode = new SerializedObject(codePuzzle);
                    soCode.FindProperty("_correctCode").stringValue = "1234";
                    soCode.FindProperty("_maxCodeLength").intValue = 4;
                    soCode.FindProperty("_interactionPrompt").stringValue = "[E] Use Keypad";
                    soCode.ApplyModifiedProperties();
                }
            }
        }

        private static void CreateNarrativePuzzle(Vector3 center)
        {
            GameObject triggerArea = new GameObject("AudioTriggerArea");
            triggerArea.transform.position = center;
            var collider = triggerArea.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(4, 2, 4);

            var triggerScript = triggerArea.AddComponent<NarrativeTrigger>();
            // Just basic setup.
        }
    }
}
