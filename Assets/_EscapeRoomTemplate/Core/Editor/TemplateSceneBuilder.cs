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
            CreateExaminePuzzle(new Vector3(6, 0, 20));

            // Room 5: Code Panel
            CreatePlatform(env, "Room5_Keypad", new Vector3(-6, 0, 25), new Color(0.4f, 0.3f, 0.4f));
            CreateKeypadPuzzle(new Vector3(-6, 0, 25));

            // Room 6: Narrative Audio
            CreatePlatform(env, "Room6_Narrative", new Vector3(6, 0, 30), new Color(0.3f, 0.4f, 0.4f));
            CreateNarrativePuzzle(new Vector3(6, 0, 30));

            // Room 7: Item Receiver
            CreatePlatform(env, "Room7_Receiver", new Vector3(-6, 0, 35), new Color(0.4f, 0.4f, 0.4f));
            CreateItemReceiverPuzzle(new Vector3(-6, 0, 35));

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

            // Drawer (Slide)
            GameObject drawer = CreateInteractableObject("Drawer", center + new Vector3(-0.5f, 0.85f, -0.5f), new Vector3(0.8f, 0.2f, 0.8f), new Color(0.5f, 0.2f, 0.1f));
            var drawerScript = drawer.AddComponent<Door>();
            SerializedObject soDrawer = new SerializedObject(drawerScript);
            soDrawer.FindProperty("_movementType").enumValueIndex = (int)DoorMovementType.Slide;
            soDrawer.FindProperty("_slideOffset").vector3Value = new Vector3(0, 0, -0.5f);
            soDrawer.FindProperty("_interactionPrompt").stringValue = "[E] Open Drawer";
            soDrawer.ApplyModifiedProperties();

            // Cabinet (Pivot)
            GameObject cabinetDoor = CreateInteractableObject("CabinetDoor", center + new Vector3(0.5f, 0.5f, -0.55f), new Vector3(0.8f, 0.8f, 0.1f), new Color(0.4f, 0.2f, 0.05f));
            GameObject hinge = new GameObject("CabinetHinge");
            hinge.transform.SetParent(cabinetDoor.transform);
            hinge.transform.localPosition = new Vector3(0.4f, 0, 0); // Hinge on the right edge
            
            var cabScript = cabinetDoor.AddComponent<Door>();
            SerializedObject soCab = new SerializedObject(cabScript);
            soCab.FindProperty("_movementType").enumValueIndex = (int)DoorMovementType.Pivot;
            soCab.FindProperty("_customPivot").objectReferenceValue = hinge.transform;
            soCab.FindProperty("_openAngle").floatValue = -90f; // Open outwards
            soCab.FindProperty("_interactionPrompt").stringValue = "[E] Open Cabinet";
            soCab.ApplyModifiedProperties();
        }

        private static void CreateKeyAndLockPuzzle(Vector3 center)
        {
            // Wall for Pivot Door (Left)
            GameObject wallL1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallL1.transform.position = center + new Vector3(-3f, 1.5f, 2);
            wallL1.transform.localScale = new Vector3(1, 3, 0.5f);
            
            GameObject wallR1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallR1.transform.position = center + new Vector3(0f, 1.5f, 2);
            wallR1.transform.localScale = new Vector3(1, 3, 0.5f);

            GameObject wallT1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallT1.transform.position = center + new Vector3(-1.5f, 2.75f, 2);
            wallT1.transform.localScale = new Vector3(2, 0.5f, 0.5f);

            // Locked Pivot Door
            GameObject pivotDoor = CreateInteractableObject("LockedPivotDoor", center + new Vector3(-1.5f, 1.25f, 2), new Vector3(1.5f, 2.5f, 0.2f), new Color(0.8f, 0.2f, 0.2f));
            GameObject hinge = new GameObject("CustomPivot");
            hinge.transform.SetParent(pivotDoor.transform);
            hinge.transform.localPosition = new Vector3(-0.75f, 0, 0);
            
            var pDoorScript = pivotDoor.AddComponent<Door>();
            SerializedObject soPDoor = new SerializedObject(pDoorScript);
            soPDoor.FindProperty("_isLocked").boolValue = true;
            soPDoor.FindProperty("_requiredItemId").stringValue = "key_museum_01";
            soPDoor.FindProperty("_customPivot").objectReferenceValue = hinge.transform;
            soPDoor.FindProperty("_interactionPrompt").stringValue = "[E] Open Pivot Door";
            soPDoor.ApplyModifiedProperties();

            // Wall for Sliding Door (Right)
            GameObject wallR2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallR2.transform.position = center + new Vector3(3f, 1.5f, 2);
            wallR2.transform.localScale = new Vector3(1, 3, 0.5f);

            GameObject wallT2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallT2.transform.position = center + new Vector3(1.5f, 2.75f, 2);
            wallT2.transform.localScale = new Vector3(2, 0.5f, 0.5f);

            // Unlocked Sliding Door
            GameObject slideDoor = CreateInteractableObject("SlidingDoor", center + new Vector3(1.5f, 1.25f, 2), new Vector3(1.5f, 2.5f, 0.2f), new Color(0.2f, 0.6f, 0.8f));
            var sDoorScript = slideDoor.AddComponent<Door>();
            SerializedObject soSDoor = new SerializedObject(sDoorScript);
            soSDoor.FindProperty("_movementType").enumValueIndex = (int)DoorMovementType.Slide;
            soSDoor.FindProperty("_slideOffset").vector3Value = new Vector3(1.5f, 0, 0);
            soSDoor.FindProperty("_interactionPrompt").stringValue = "[E] Open Sliding Door";
            soSDoor.ApplyModifiedProperties();

            // Key on a small table
            GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "Table";
            table.transform.position = center + new Vector3(-1.5f, 0.4f, 0);
            table.transform.localScale = new Vector3(1, 0.8f, 1);

            InventoryItemData keyData = AssetDatabase.LoadAssetAtPath<InventoryItemData>("Assets/_EscapeRoomTemplate/ScriptableObjects/Items/Key_Office.asset");
            if (keyData != null)
            {
                GameObject keyObj = CreateInteractableObject("Key_Museum", center + new Vector3(-1.5f, 0.85f, 0), new Vector3(0.2f, 0.1f, 0.2f), Color.yellow);
                var pickable = keyObj.AddComponent<PickableItem>();
                SerializedObject soKey = new SerializedObject(pickable);
                soKey.FindProperty("_itemData").objectReferenceValue = keyData;
                soKey.FindProperty("_destroyOnPickup").boolValue = true;
                soKey.FindProperty("_interactionPrompt").stringValue = "[E] Pick up Key";
                soKey.ApplyModifiedProperties();
                
                // Override required item id
                soPDoor.FindProperty("_requiredItemId").stringValue = keyData.ItemId;
                soPDoor.ApplyModifiedProperties();
            }
        }

        private static void CreateLorePuzzle(Vector3 center)
        {
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pedestal.transform.position = center + new Vector3(0, 0.5f, 0);
            pedestal.transform.localScale = new Vector3(2, 1, 1);

            // 1) In-world Readable Note
            GameObject noteObj1 = CreateInteractableObject("ClueNote_World", center + new Vector3(-0.5f, 1.05f, 0), new Vector3(0.4f, 0.05f, 0.4f), Color.white);
            var note1 = noteObj1.AddComponent<InteractableNote>();
            SerializedObject soNote1 = new SerializedObject(note1);
            soNote1.FindProperty("NoteContent").stringValue = "This note can be read directly in the world.\n\nYou just look at it and press E.";
            soNote1.FindProperty("_interactionPrompt").stringValue = "[E] Read Note";
            soNote1.ApplyModifiedProperties();

            // 2) Pickable Note (Inventory)
            string assetPath = "Assets/_EscapeRoomTemplate/ScriptableObjects/Items/ReadableNote_Demo.asset";
            InventoryItemData noteData = AssetDatabase.LoadAssetAtPath<InventoryItemData>(assetPath);
            if (noteData == null)
            {
                // Ensure directory exists
                if (!AssetDatabase.IsValidFolder("Assets/_EscapeRoomTemplate/ScriptableObjects/Items"))
                    System.IO.Directory.CreateDirectory("Assets/_EscapeRoomTemplate/ScriptableObjects/Items");

                noteData = ScriptableObject.CreateInstance<InventoryItemData>();
                var soItem = new SerializedObject(noteData);
                soItem.FindProperty("_itemId").stringValue = "note_demo_01";
                soItem.FindProperty("_displayName").stringValue = "Mysterious Letter";
                soItem.FindProperty("_description").stringValue = "A crumpled letter I found.";
                soItem.FindProperty("_isReadable").boolValue = true;
                soItem.FindProperty("_noteContent").stringValue = "This is a note you picked up!\n\nYou can read it from your inventory anytime.";
                soItem.ApplyModifiedProperties();

                AssetDatabase.CreateAsset(noteData, assetPath);
                AssetDatabase.SaveAssets();
            }

            GameObject noteObj2 = CreateInteractableObject("ClueNote_Inventory", center + new Vector3(0.5f, 1.05f, 0), new Vector3(0.4f, 0.05f, 0.4f), new Color(0.9f, 0.9f, 0.7f));
            var pickable = noteObj2.AddComponent<PickableItem>();
            SerializedObject soPickable = new SerializedObject(pickable);
            soPickable.FindProperty("_itemData").objectReferenceValue = noteData;
            soPickable.FindProperty("_destroyOnPickup").boolValue = true;
            soPickable.FindProperty("_interactionPrompt").stringValue = "[E] Pick up Letter";
            soPickable.ApplyModifiedProperties();
        }

        private static void CreateKeypadPuzzle(Vector3 center)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = center + new Vector3(0, 1.5f, 2);
            wall.transform.localScale = new Vector3(4, 3, 0.5f);

            // Add Door
            EscapeRoomRevolt.EditorTools.InteractableCreator.CreateDoor();
            GameObject safeDoor = UnityEditor.Selection.activeGameObject;
            var doorScript = safeDoor.GetComponent<Door>();
            if (safeDoor != null)
            {
                safeDoor.name = "SafeDoor";
                safeDoor.transform.position = center + new Vector3(-1f, 1.25f, 1.74f); // Standard door height is 2.5, so center Y is 1.25
                
                SerializedObject soDoor = new SerializedObject(doorScript);
                soDoor.FindProperty("_openAngle").floatValue = 90f;
                soDoor.FindProperty("_interactionPrompt").stringValue = "[E] Open Safe";
                soDoor.FindProperty("_isLocked").boolValue = true; // Locked initially
                soDoor.FindProperty("_lockedPrompt").stringValue = "Locked electronically.";
                soDoor.ApplyModifiedProperties();
            }

            // Add Cinematic Camera
            GameObject camObj = new GameObject("FeedbackCamera");
            camObj.transform.position = center + new Vector3(-1f, 1.5f, 0.5f); // Stand in middle, look at door
            camObj.transform.LookAt(safeDoor.transform);
            var cam = camObj.AddComponent<Camera>();
            camObj.SetActive(false);
            var cinematicCam = camObj.AddComponent<EscapeRoomRevolt.Systems.Animation.CinematicCamera>();

            EscapeRoomRevolt.EditorTools.InteractableCreator.CreateKeypadPanel();
            GameObject keypadObj = UnityEditor.Selection.activeGameObject;
            if (keypadObj != null)
            {
                keypadObj.name = "Keypad_Safe";
                keypadObj.transform.position = center + new Vector3(1f, 1.5f, 1.74f); // Placed to the right of the door
                keypadObj.transform.rotation = Quaternion.Euler(0, 180f, 0); // Flipped to face the room
                
                var codePuzzle = keypadObj.GetComponent<CodePanelPuzzle>();
                if (codePuzzle != null)
                {
                    SerializedObject soCode = new SerializedObject(codePuzzle);
                    soCode.FindProperty("_correctCode").stringValue = "1234";
                    soCode.FindProperty("_maxCodeLength").intValue = 4;
                    soCode.ApplyModifiedProperties();
                    
                    // Hook up events
                    UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(codePuzzle.OnSolvedEvent, doorScript.Unlock);
                    UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(codePuzzle.OnSolvedEvent, doorScript.ForceOpen);
                    UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(codePuzzle.OnSolvedEvent, cinematicCam.PlayCinematic);
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
            SerializedObject soTrigger = new SerializedObject(triggerScript);
            
            soTrigger.FindProperty("_playMode").enumValueIndex = (int)NarrativePlayMode.Once;
            
            SerializedProperty sequencesProp = soTrigger.FindProperty("_sequences");
            sequencesProp.arraySize = 1;
            SerializedProperty seq = sequencesProp.GetArrayElementAtIndex(0);
            
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_EscapeRoomTemplate/Audio/Voice/audio01.mp3");
            if (clip != null)
            {
                seq.FindPropertyRelative("audioClip").objectReferenceValue = clip;
            }

            SerializedProperty linesProp = seq.FindPropertyRelative("subtitleLines");
            linesProp.arraySize = 1;
            SerializedProperty line1 = linesProp.GetArrayElementAtIndex(0);
            line1.FindPropertyRelative("text").stringValue = "Ah... you made it. Welcome to the museum. Feel free to look around.";
            line1.FindPropertyRelative("duration").floatValue = 5f;
            
            soTrigger.ApplyModifiedProperties();
        }

        private static void CreateExaminePuzzle(Vector3 center)
        {
            EscapeRoomRevolt.Editor.ExamineChamberSetup.CreateExamineChamber();
            
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pedestal.transform.position = center + new Vector3(0, 0.5f, 0);
            pedestal.transform.localScale = new Vector3(1, 1, 1);

            InventoryItemData itemData = AssetDatabase.LoadAssetAtPath<InventoryItemData>("Assets/_EscapeRoomTemplate/ScriptableObjects/Items/Key_Office.asset");
            if (itemData != null)
            {
                // Ensure the item has a 3D model assigned for the Examine Chamber
                if (itemData.WorldPrefab == null)
                {
                    GameObject clauPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_EscapeRoomTemplate/Prefabs/clau.prefab");
                    if (clauPrefab != null)
                    {
                        SerializedObject soItem = new SerializedObject(itemData);
                        soItem.FindProperty("_worldPrefab").objectReferenceValue = clauPrefab;
                        soItem.ApplyModifiedProperties();
                        AssetDatabase.SaveAssets();
                    }
                }

                GameObject examineObj = CreateInteractableObject("ExamineObject", center + new Vector3(0, 1.2f, 0), new Vector3(0.3f, 0.3f, 0.3f), Color.cyan);
                var pickable = examineObj.AddComponent<PickableItem>();
                SerializedObject soPickable = new SerializedObject(pickable);
                soPickable.FindProperty("_itemData").objectReferenceValue = itemData;
                soPickable.FindProperty("_destroyOnPickup").boolValue = true;
                soPickable.FindProperty("_interactionPrompt").stringValue = "[E] Pick up to Examine";
                soPickable.ApplyModifiedProperties();
            }
        }

        private static void CreateItemReceiverPuzzle(Vector3 center)
        {
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pedestal.transform.position = center + new Vector3(0, 0.5f, 0);
            pedestal.transform.localScale = new Vector3(1, 1, 1);

            GameObject receiverObj = CreateInteractableObject("Receiver", center + new Vector3(0, 1.1f, 0), new Vector3(0.5f, 0.2f, 0.5f), Color.grey);
            var receiver = receiverObj.AddComponent<ItemReceiver>();
            
            // Add a locked door instead of a drawer to be more epic
            EscapeRoomRevolt.EditorTools.InteractableCreator.CreateDoor();
            GameObject doorObj = UnityEditor.Selection.activeGameObject;
            var doorScript = doorObj != null ? doorObj.GetComponent<Door>() : null;
            if (doorObj != null && doorScript != null)
            {
                doorObj.name = "SecretDoor";
                doorObj.transform.position = center + new Vector3(-1.5f, 1.25f, 0);
                
                SerializedObject soDoor = new SerializedObject(doorScript);
                soDoor.FindProperty("_isLocked").boolValue = true;
                soDoor.FindProperty("_lockedPrompt").stringValue = "It's firmly locked. A mechanism holds it.";
                soDoor.ApplyModifiedProperties();
            }

            // Add Cinematic Camera pointing to the door
            GameObject camObj = new GameObject("ReceiverFeedbackCamera");
            camObj.transform.position = center + new Vector3(0.5f, 1.5f, -1.5f);
            if (doorObj != null) camObj.transform.LookAt(doorObj.transform);
            var cam = camObj.AddComponent<Camera>();
            camObj.SetActive(false);
            var cinematicCam = camObj.AddComponent<EscapeRoomRevolt.Systems.Animation.CinematicCamera>();

            InventoryItemData itemData = AssetDatabase.LoadAssetAtPath<InventoryItemData>("Assets/_EscapeRoomTemplate/ScriptableObjects/Items/Key_Office.asset");
            if (itemData != null)
            {
                // Create a pickable key right here so the player doesn't have to walk to room 4
                GameObject keyTable = GameObject.CreatePrimitive(PrimitiveType.Cube);
                keyTable.name = "KeyTable";
                keyTable.transform.position = center + new Vector3(1.5f, 0.5f, 0);
                keyTable.transform.localScale = new Vector3(0.5f, 1f, 0.5f);

                GameObject keyObj = CreateInteractableObject("PickupKey", center + new Vector3(1.5f, 1.1f, 0), new Vector3(0.2f, 0.2f, 0.2f), Color.cyan);
                var pickable = keyObj.AddComponent<PickableItem>();
                SerializedObject soPickable = new SerializedObject(pickable);
                soPickable.FindProperty("_itemData").objectReferenceValue = itemData;
                soPickable.FindProperty("_destroyOnPickup").boolValue = true;
                soPickable.FindProperty("_interactionPrompt").stringValue = "[E] Pick up Key";
                soPickable.ApplyModifiedProperties();

                // Configure Receiver
                SerializedObject soReceiver = new SerializedObject(receiver);
                soReceiver.FindProperty("_requiredItem").objectReferenceValue = itemData;
                soReceiver.FindProperty("_interactionPrompt").stringValue = "[E] Insert Item";
                soReceiver.ApplyModifiedProperties();

                if (doorScript != null)
                {
                    // Hook up the events
                    UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(receiver.OnItemAccepted, doorScript.Unlock);
                    UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(receiver.OnItemAccepted, doorScript.ForceOpen);
                    UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(receiver.OnItemAccepted, cinematicCam.PlayCinematic);
                }
            }
        }

        private static void CreateLightSwitchPuzzle(Vector3 center)
        {
            // Wall for the switch
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = center + new Vector3(0, 1.5f, 2);
            wall.transform.localScale = new Vector3(4, 3, 0.5f);

            // Light Switch
            GameObject switchObj = CreateInteractableObject("LightSwitch", center + new Vector3(0, 1.2f, 1.74f), new Vector3(0.2f, 0.3f, 0.05f), Color.white);
            var trigger = switchObj.AddComponent<InteractableTrigger>();
            SerializedObject soTrigger = new SerializedObject(trigger);
            soTrigger.FindProperty("_prompt").stringValue = "[E] Toggle Light";
            soTrigger.FindProperty("_isToggle").boolValue = true;
            soTrigger.ApplyModifiedProperties();

            // The Light Object (parent)
            GameObject lightRoot = new GameObject("Spotlight_Root");
            lightRoot.transform.position = center + new Vector3(0, 3f, 0); // High up
            lightRoot.transform.rotation = Quaternion.Euler(90f, 0, 0); // Pointing down
            
            // The actual Light Component
            var lightComp = lightRoot.AddComponent<Light>();
            lightComp.type = LightType.Spot;
            lightComp.range = 10f;
            lightComp.spotAngle = 60f;
            lightComp.intensity = 50f; // High intensity for URP
            
            // Set initial state
            lightRoot.SetActive(false); 

            // Hook up events to activate/deactivate the GameObject
            UnityEngine.Events.UnityAction<bool> setActiveAction = new UnityEngine.Events.UnityAction<bool>(lightRoot.SetActive);
            UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(trigger.OnInteractEvent, setActiveAction, true);
            UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(trigger.OnInteractOffEvent, setActiveAction, false);
        }
    }
}
