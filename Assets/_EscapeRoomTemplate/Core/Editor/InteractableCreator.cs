using UnityEngine;
using UnityEditor;
using EscapeRoomRevolt.Systems.Interaction;
using EscapeRoomRevolt.Systems.Puzzle;
using EscapeRoomRevolt.Systems.Inventory;
using EscapeRoomRevolt.Systems.Survival;

namespace EscapeRoomRevolt.EditorTools
{
    public static class InteractableCreator
    {
        [MenuItem("Escape Room Framework/Create/Interactables/Door", priority = 101)]
        public static void CreateDoor()
        {
            GameObject logicObj = CreateBaseInteractable("NewDoor", new Vector3(1.5f, 2.5f, 0.2f), new Color(0.8f, 0.4f, 0.2f));
            
            // Afegir pivot costum
            GameObject customPivot = new GameObject("CustomPivot");
            customPivot.transform.SetParent(logicObj.transform);
            customPivot.transform.localPosition = new Vector3(-0.75f, 0, 0);
            
            Door doorScript = logicObj.AddComponent<Door>();
            SerializedObject so = new SerializedObject(doorScript);
            so.FindProperty("_customPivot").objectReferenceValue = customPivot.transform;
            so.ApplyModifiedProperties();

            FinalizeCreation(logicObj);
        }

        [MenuItem("Escape Room Framework/Create/Interactables/Cabinet", priority = 102)]
        public static void CreateCabinet()
        {
            GameObject logicObj = CreateBaseInteractable("NewCabinet", new Vector3(0.6f, 0.8f, 0.05f), new Color(0.4f, 0.25f, 0.1f));
            
            GameObject customPivot = new GameObject("CustomPivot");
            customPivot.transform.SetParent(logicObj.transform);
            customPivot.transform.localPosition = new Vector3(-0.3f, 0, 0);
            
            Door doorScript = logicObj.AddComponent<Door>();
            SerializedObject so = new SerializedObject(doorScript);
            so.FindProperty("_customPivot").objectReferenceValue = customPivot.transform;
            so.FindProperty("_movementType").enumValueIndex = (int)DoorMovementType.Pivot;
            so.ApplyModifiedProperties();

            FinalizeCreation(logicObj);
        }

        [MenuItem("Escape Room Framework/Create/Interactables/Drawer", priority = 103)]
        public static void CreateDrawer()
        {
            GameObject logicObj = CreateBaseInteractable("NewDrawer", new Vector3(0.8f, 0.2f, 0.8f), new Color(0.4f, 0.25f, 0.1f));
            
            Door doorScript = logicObj.AddComponent<Door>();
            SerializedObject so = new SerializedObject(doorScript);
            so.FindProperty("_movementType").enumValueIndex = (int)DoorMovementType.Slide;
            so.FindProperty("_slideOffset").vector3Value = new Vector3(0, 0, 0.8f);
            so.ApplyModifiedProperties();

            FinalizeCreation(logicObj);
        }

        [MenuItem("Escape Room Framework/Create/Puzzles/Keypad Panel", priority = 201)]
        public static void CreateKeypadPanel()
        {
            GameObject logicObj = CreateBaseInteractable("NewKeypad", new Vector3(0.2f, 0.25f, 0.025f), new Color(0.15f, 0.15f, 0.15f));
            
            CodePanelPuzzle codePuzzle = logicObj.AddComponent<CodePanelPuzzle>();
            InteractableKeypad keypadScript = logicObj.AddComponent<InteractableKeypad>();

            // Generate 3D Keypad layout on the front face (Z = +0.013f since scale Z is 0.025)
            GameObject keypadRoot = new GameObject("Keypad_Root");
            keypadRoot.transform.SetParent(logicObj.transform);
            keypadRoot.transform.localPosition = new Vector3(0, 0, 0.013f); // Slightly in front of the surface

            // 1. Create Display LCD
            GameObject displayObj = new GameObject("Display", typeof(RectTransform));
            displayObj.transform.SetParent(keypadRoot.transform);
            displayObj.transform.localPosition = new Vector3(0, 0.09f, 0); // Position it cleanly at the top
            displayObj.transform.localRotation = Quaternion.Euler(0, 180f, 0); // Rotate to face outward
            
            var tmpText = displayObj.AddComponent<TMPro.TextMeshPro>();
            
            // Adjust RectTransform
            RectTransform rtDisplay = displayObj.GetComponent<RectTransform>();
            rtDisplay.sizeDelta = new Vector2(0.175f, 0.04f);

            tmpText.text = "----";
            tmpText.alignment = TMPro.TextAlignmentOptions.Center;
            tmpText.color = Color.green;
            tmpText.enableAutoSizing = true;
            tmpText.fontSizeMin = 0.02f;
            tmpText.fontSizeMax = 0.4f;
            
            // Link display to puzzle
            SerializedObject soPuzzle = new SerializedObject(codePuzzle);
            soPuzzle.FindProperty("_display3D").objectReferenceValue = tmpText;

            // 2. Create Grid of 3D Buttons (11 buttons + 1 LED)
            string[] keys = {
                "1", "2", "3",
                "4", "5", "6",
                "7", "8", "9",
                "C", "0", "" // Empty string will be the LED
            };

            float startX = 0.075f; // +X is left when facing -Z
            float startY = 0.05f;
            float spacingX = -0.075f; // go towards -X (right)
            float spacingY = 0.05f;

            for (int i = 0; i < 12; i++)
            {
                int row = i / 3;
                int col = i % 3;
                string value = keys[i];

                if (string.IsNullOrEmpty(value))
                {
                    // Create Status LED in this slot
                    GameObject ledObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    ledObj.name = "StatusLED";
                    ledObj.transform.SetParent(keypadRoot.transform);
                    ledObj.transform.localPosition = new Vector3(startX + (col * spacingX), startY - (row * spacingY), 0.005f); // Stick out slightly
                    ledObj.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                    
                    Material ledMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    ledMat.color = Color.red;
                    ledMat.EnableKeyword("_EMISSION");
                    ledMat.SetColor("_EmissionColor", Color.red * 0.5f); // Slight red glow by default
                    ledObj.GetComponent<Renderer>().sharedMaterial = ledMat;
                    
                    soPuzzle.FindProperty("_statusLed").objectReferenceValue = ledObj.GetComponent<Renderer>();
                    soPuzzle.ApplyModifiedProperties();
                    continue;
                }

                GameObject btn = CreateKeypadButton(value, codePuzzle);
                btn.transform.SetParent(keypadRoot.transform);
                btn.transform.localPosition = new Vector3(startX + (col * spacingX), startY - (row * spacingY), 0);
            }

            // 3. Create Focus Camera
            GameObject focusCamObj = new GameObject("FocusCamera");
            focusCamObj.transform.SetParent(logicObj.transform);
            // Position it 0.5 units away from the keypad, facing it (Z+)
            focusCamObj.transform.localPosition = new Vector3(0, 0, 0.4f); 
            focusCamObj.transform.localRotation = Quaternion.Euler(0, 180f, 0);
            
            Camera fCam = focusCamObj.AddComponent<Camera>();
            fCam.fieldOfView = 40f; // Zoomed in FoV
            focusCamObj.SetActive(false); // Default off

            // Link camera and specific renderers to InteractableKeypad
            SerializedObject soKeypad = new SerializedObject(keypadScript);
            soKeypad.FindProperty("_focusCamera").objectReferenceValue = fCam;
            
            // Only highlight the main keypad base, NOT the buttons or text
            Renderer baseRenderer = logicObj.transform.Find("NewKeypad_Visuals")?.GetComponent<Renderer>();
            if (baseRenderer == null) baseRenderer = logicObj.GetComponentInChildren<Renderer>(); // fallback

            soKeypad.ApplyModifiedProperties();
            ConfigureOutlineTarget(logicObj, baseRenderer);

            FinalizeCreation(logicObj);
        }

        private static GameObject CreateKeypadButton(string value, CodePanelPuzzle puzzle)
        {
            GameObject logicObj = new GameObject($"Btn_{value}_Logic");
            
            // Visuals
            GameObject visualObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visualObj.name = value + "_Visuals";
            visualObj.transform.SetParent(logicObj.transform);
            visualObj.transform.localPosition = Vector3.zero;
            visualObj.transform.localScale = new Vector3(0.04f, 0.03f, 0.015f);
            
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = Color.grey;
            visualObj.GetComponent<Renderer>().material = mat;

            // Text
            GameObject textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(logicObj.transform);
            textObj.transform.localPosition = new Vector3(0, 0, 0.008f); // Slightly in front of the button (Z+)
            textObj.transform.localRotation = Quaternion.Euler(0, 180f, 0); // Rotate to face outward
            var tmp = textObj.AddComponent<TMPro.TextMeshPro>();
            
            // Adjust RectTransform to match button face size
            RectTransform rt = textObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0.05f, 0.04f);

            tmp.text = value;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 0.02f;
            tmp.fontSizeMax = 0.25f;

            // Logic
            KeypadButton3D btnScript = logicObj.AddComponent<KeypadButton3D>();
            btnScript.SetTargetPuzzle(puzzle);
            
            if (value == "C") btnScript.SetAction(KeypadButtonAction.Clear, "");
            else if (value == "OK") btnScript.SetAction(KeypadButtonAction.Submit, "");
            else btnScript.SetAction(KeypadButtonAction.Digit, value);

            // Only highlight the button cube itself
            ConfigureOutlineTarget(logicObj, visualObj.GetComponent<Renderer>());

            return logicObj;
        }

        [MenuItem("Escape Room Framework/Create/Inventory/Examine Hotspot", priority = 151)]
        public static void CreateExamineHotspot()
        {
            GameObject hotspotObj = new GameObject("ExamineHotspot");
            GameObject parent = Selection.activeGameObject;
            if (parent != null)
            {
                hotspotObj.transform.SetParent(parent.transform, false);
            }
            else
            {
                SceneView view = SceneView.lastActiveSceneView;
                if (view != null) hotspotObj.transform.position = view.pivot;
            }

            SphereCollider collider = hotspotObj.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = .15f;
            hotspotObj.AddComponent<ExamineHotspot>();

            FinalizeCreation(hotspotObj);
            Debug.Log("[Escape Room Framework] Examine Hotspot created" + (parent != null ? $" under '{parent.name}'." : ".")
                + " Place it as a child of an item's WorldPrefab, sized to cover the area players should click while examining.");
        }

        // Placement and Sliding now live in PuzzleCreator, which builds the pieces and wiring too
        // instead of leaving a bare controller for the designer to finish by hand.

        [MenuItem("Escape Room Framework/Create/Puzzles/Pipe Puzzle", priority = 205)]
        public static void CreatePipePuzzle()
        {
            GameObject logicObj = new GameObject("NewPipePuzzle");
            SceneView view = SceneView.lastActiveSceneView;
            if (view != null) logicObj.transform.position = view.pivot;

            PipePuzzle puzzle = logicObj.AddComponent<PipePuzzle>();
            SerializedObject so = new SerializedObject(puzzle);
            SerializedProperty tiles = so.FindProperty("_tiles");
            tiles.arraySize = 2;

            SerializedProperty tileA = tiles.GetArrayElementAtIndex(0);
            tileA.FindPropertyRelative("tileId").stringValue = "pipe_a";
            tileA.FindPropertyRelative("row").intValue = 0;
            tileA.FindPropertyRelative("column").intValue = 0;
            tileA.FindPropertyRelative("openSides").intValue = (int)PipeSide.East;
            tileA.FindPropertyRelative("startingRotationSteps").intValue = 2;

            SerializedProperty tileB = tiles.GetArrayElementAtIndex(1);
            tileB.FindPropertyRelative("tileId").stringValue = "pipe_b";
            tileB.FindPropertyRelative("row").intValue = 0;
            tileB.FindPropertyRelative("column").intValue = 1;
            tileB.FindPropertyRelative("openSides").intValue = (int)PipeSide.West;
            tileB.FindPropertyRelative("startingRotationSteps").intValue = 2;

            so.FindProperty("_sourceTileId").stringValue = "pipe_a";
            so.FindProperty("_sinkTileId").stringValue = "pipe_b";
            so.ApplyModifiedProperties();

            FinalizeCreation(logicObj);
            Debug.Log("[Escape Room Framework] Pipe Puzzle created with two example segments, both rotated 180° away from solved. "
                + "Call RotateTile(tileId) from whatever interactable represents each segment in your scene; rotate each one twice to connect pipe_a to pipe_b.");
        }

        [MenuItem("Escape Room Framework/Create/Puzzles/Multi-Stage Puzzle", priority = 203)]
        public static void CreateMultiStagePuzzle()
        {
            PuzzleCreator.CreateMultiStageChainPuzzle();
        }

        [MenuItem("Escape Room Framework/Create/Interactables/Note", priority = 104)]
        public static void CreateNote()
        {
            GameObject logicObj = CreateBaseInteractable("NewNote", new Vector3(0.3f, 0.05f, 0.4f), Color.white);
            
            logicObj.AddComponent<InteractableNote>();

            FinalizeCreation(logicObj);
        }

        [MenuItem("Escape Room Framework/Create/Interactables/Pickable Item", priority = 105)]
        public static void CreatePickableItem()
        {
            GameObject logicObj = CreateBaseInteractable("NewPickableItem", new Vector3(0.2f, 0.2f, 0.2f), Color.yellow);
            
            logicObj.AddComponent<PickableItem>();

            FinalizeCreation(logicObj);
        }

        [MenuItem("Escape Room Framework/Create/Interactables/Generic Trigger", priority = 106)]
        public static void CreateTrigger()
        {
            GameObject logicObj = CreateGenericButton();
            FinalizeCreation(logicObj);
        }

        public static GameObject CreateGenericButton()
        {
            GameObject logicObj = CreateBaseInteractable("NewTrigger", new Vector3(0.2f, 0.2f, 0.2f), Color.red);
            logicObj.AddComponent<InteractableTrigger>();
            return logicObj;
        }

        [MenuItem("Escape Room Framework/Create/Interactables/Item Receiver", priority = 107)]
        public static void CreateItemReceiver()
        {
            GameObject logicObj = CreateBaseInteractable("NewItemReceiver", new Vector3(0.5f, 0.5f, 0.5f), Color.gray);
            
            ItemReceiver receiver = logicObj.AddComponent<ItemReceiver>();

            // Create automatic spawn point for the 3D visual feedback
            GameObject spawnPoint = new GameObject("SpawnPoint");
            spawnPoint.transform.SetParent(logicObj.transform, false);
            // Move it slightly up so it's resting on top of the box
            spawnPoint.transform.localPosition = new Vector3(0f, 0.3f, 0f);

            // Link the spawn point to the script
            UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(receiver);
            so.FindProperty("_spawnLocation").objectReferenceValue = spawnPoint.transform;
            so.ApplyModifiedProperties();

            FinalizeCreation(logicObj);
        }

        [MenuItem("Escape Room Framework/Create/Triggers/Narrative Trigger", priority = 301)]
        public static void CreateNarrativeTrigger()
        {
            GameObject logicObj = new GameObject("NewNarrativeTrigger");
            
            SceneView view = SceneView.lastActiveSceneView;
            if (view != null) logicObj.transform.position = view.pivot;

            BoxCollider col = logicObj.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(2, 2, 2);

            logicObj.AddComponent<AudioSource>();
            logicObj.AddComponent<NarrativeTrigger>();

            FinalizeCreation(logicObj);
        }

        [MenuItem("Escape Room Framework/Create/Triggers/Event Trigger Zone", priority = 303)]
        public static void CreateEventTriggerZone()
        {
            GameObject logicObj = new GameObject("NewEventTriggerZone");

            SceneView view = SceneView.lastActiveSceneView;
            if (view != null) logicObj.transform.position = view.pivot;

            BoxCollider col = logicObj.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(2f, 2f, 2f);
            logicObj.AddComponent<EventTriggerZone>();

            FinalizeCreation(logicObj);
        }

        [MenuItem("Escape Room Framework/Create/Interactables/Lever", priority = 108)]
        public static void CreateLever()
        {
            GameObject logicObj = CreateBaseInteractable("NewLever", new Vector3(0.1f, 0.4f, 0.1f), new Color(0.6f, 0.2f, 0.2f));
            
            // The visual part should be offset so it pivots at the base
            Transform visuals = logicObj.transform.Find("NewLever_Visuals");
            if (visuals != null)
            {
                visuals.localPosition = new Vector3(0, 0.2f, 0); // Offset up so pivot is at base
            }

            var toggle = logicObj.AddComponent<InteractableToggle>();
            SerializedObject so = new SerializedObject(toggle);
            so.FindProperty("_movementType").enumValueIndex = (int)ToggleMovementType.Rotate;
            so.FindProperty("_offAngles").vector3Value = new Vector3(-45f, 0f, 0f);
            so.FindProperty("_onAngles").vector3Value = new Vector3(45f, 0f, 0f);
            so.FindProperty("_visualTransform").objectReferenceValue = logicObj.transform; // Rotate the whole logic object or the base? Better rotate logicObj! Wait, if we rotate LogicObj, the collider rotates. But visually, maybe rotate visuals? If we rotate logic object, the pivot is 0,0,0.
            so.ApplyModifiedProperties();

            FinalizeCreation(logicObj);
        }

        [MenuItem("Escape Room Framework/Create/Interactables/Switch", priority = 109)]
        public static void CreateSwitch()
        {
            GameObject logicObj = CreateBaseInteractable("NewSwitch", new Vector3(0.2f, 0.4f, 0.1f), new Color(0.2f, 0.6f, 0.2f));
            
            var toggle = logicObj.AddComponent<InteractableToggle>();
            SerializedObject so = new SerializedObject(toggle);
            so.FindProperty("_movementType").enumValueIndex = (int)ToggleMovementType.Slide;
            so.FindProperty("_offPosition").vector3Value = new Vector3(0f, 0f, 0f);
            so.FindProperty("_onPosition").vector3Value = new Vector3(0f, -0.2f, 0f);
            so.FindProperty("_visualTransform").objectReferenceValue = logicObj.transform.Find("NewSwitch_Visuals");
            so.ApplyModifiedProperties();

            FinalizeCreation(logicObj);
        }

        [MenuItem("Escape Room Framework/Create/Interactables/Multi-Position Lever", priority = 110)]
        public static void CreateMultiPositionLever()
        {
            GameObject logicObj = CreateBaseInteractable("NewCyclerLever", new Vector3(0.1f, 0.4f, 0.1f), new Color(0.6f, 0.4f, 0.2f));

            Transform visuals = logicObj.transform.Find("NewCyclerLever_Visuals");
            if (visuals != null)
            {
                visuals.localPosition = new Vector3(0, 0.2f, 0); // Offset up so pivot is at base
            }

            var positioner = logicObj.AddComponent<SteppedPositioner>();
            logicObj.AddComponent<InteractableCycler>();
            SerializedObject so = new SerializedObject(positioner);
            so.FindProperty("_movementType").enumValueIndex = (int)SteppedMovementType.Rotate;
            SerializedProperty positions = so.FindProperty("_positions");
            positions.arraySize = 3;
            Vector3[] exampleAngles = { new Vector3(-45f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(45f, 0f, 0f) };
            for (int i = 0; i < 3; i++)
            {
                SerializedProperty element = positions.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("prompt").stringValue = "Cycle";
                element.FindPropertyRelative("rotation").vector3Value = exampleAngles[i];
            }
            so.FindProperty("_visualTransform").objectReferenceValue = logicObj.transform;
            so.ApplyModifiedProperties();

            FinalizeCreation(logicObj);
            Debug.Log("[Escape Room Framework] Multi-Position Lever created with 3 example positions. "
                + "Add/remove entries in Positions to change how many states it cycles through, for Rotate or Slide.");
        }

        [MenuItem("Escape Room Framework/Create/Triggers/Hint Zone", priority = 302)]
        public static void CreateHintZone()
        {
            GameObject logicObj = new GameObject("NewHintZone");
            
            SceneView view = SceneView.lastActiveSceneView;
            if (view != null) logicObj.transform.position = view.pivot;

            BoxCollider col = logicObj.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(3, 3, 3);

            logicObj.AddComponent<EscapeRoomRevolt.Systems.Hint.HintZoneTrigger>();

            FinalizeCreation(logicObj);
        }

        [MenuItem("Escape Room Framework/Create/Interactables/Physics Grabbable", priority = 110)]
        public static void CreatePhysicsGrabbable()
        {
            // We use CreateBaseInteractable which creates a Logic parent and Visuals child (with collider).
            // However, Rigidbody works best when it's on the same object as the collider or at the root.
            // Since LogicObj is the root, we'll put Rigidbody there, but it needs a collider. 
            // CreateBaseInteractable puts the Collider on the Visuals child. Rigidbody will compound it, which is fine.
            GameObject logicObj = CreateBaseInteractable("NewGrabbable", new Vector3(0.5f, 0.5f, 0.5f), new Color(0.3f, 0.5f, 0.8f));
            
            Rigidbody rb = logicObj.AddComponent<Rigidbody>();
            rb.mass = 2f;

            // PhysicsGrabbable requires Rigidbody and Collider. Since collider is on child, we'll move it to root.
            Transform visual = logicObj.transform.Find("NewGrabbable_Visuals");
            if (visual != null)
            {
                Collider childCol = visual.GetComponent<Collider>();
                if (childCol != null)
                {
                    GameObject.DestroyImmediate(childCol);
                }
            }
            logicObj.AddComponent<BoxCollider>();
            
            logicObj.AddComponent<PhysicsGrabbable>();

            FinalizeCreation(logicObj);
        }

        [MenuItem("Escape Room Framework/Create/Survival/Hiding Spot", priority = 401)]
        public static void CreateHidingSpot()
        {
            GameObject logicObj = new GameObject("NewHidingSpot");
            SceneView view = SceneView.lastActiveSceneView;
            if (view != null) logicObj.transform.position = view.pivot;

            BoxCollider trigger = logicObj.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(.7f, 1.9f, .7f);
            trigger.center = new Vector3(0f, .95f, 0f);
            logicObj.AddComponent<UnityEngine.AI.NavMeshObstacle>().carving = true;

            GameObject insideAnchor = new GameObject("InsideAnchor");
            insideAnchor.transform.SetParent(logicObj.transform, false);
            insideAnchor.transform.localPosition = new Vector3(0f, 0f, .1f);

            GameObject exitAnchor = new GameObject("ExitAnchor");
            exitAnchor.transform.SetParent(logicObj.transform, false);
            exitAnchor.transform.localPosition = new Vector3(0f, 0f, 1f);

            GameObject inspectionAnchor = new GameObject("InspectionAnchor");
            inspectionAnchor.transform.SetParent(logicObj.transform, false);
            inspectionAnchor.transform.localPosition = new Vector3(0f, 1.4f, .7f);

            GameObject modelSocket = new GameObject("ModelSocket");
            modelSocket.transform.SetParent(logicObj.transform, false);

            GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            placeholder.name = "Placeholder_ReplaceMe";
            placeholder.transform.SetParent(modelSocket.transform, false);
            placeholder.transform.localPosition = new Vector3(0f, .95f, 0f);
            placeholder.transform.localScale = new Vector3(.7f, 1.9f, .7f);
            Object.DestroyImmediate(placeholder.GetComponent<Collider>());
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader != null)
            {
                Material mat = new Material(urpShader) { color = new Color(.3f, .22f, .15f) };
                placeholder.GetComponent<Renderer>().material = mat;
            }

            HidingSpot hidingSpot = logicObj.AddComponent<HidingSpot>();
            SerializedObject hidingSo = new SerializedObject(hidingSpot);
            hidingSo.FindProperty("_insideAnchor").objectReferenceValue = insideAnchor.transform;
            hidingSo.FindProperty("_exitAnchor").objectReferenceValue = exitAnchor.transform;
            hidingSo.FindProperty("_inspectionAnchor").objectReferenceValue = inspectionAnchor.transform;
            hidingSo.ApplyModifiedProperties();

            ReplaceableModelSlot modelSlot = logicObj.AddComponent<ReplaceableModelSlot>();
            SerializedObject slotSo = new SerializedObject(modelSlot);
            slotSo.FindProperty("_modelSocket").objectReferenceValue = modelSocket.transform;
            slotSo.FindProperty("_placeholderVisual").objectReferenceValue = placeholder;
            slotSo.ApplyModifiedProperties();

            FinalizeCreation(logicObj);
            Debug.Log("[Escape Room Framework] Hiding Spot created (defaults to Kind = Locker; change it in the Inspector for a bed/container/custom spot). "
                + "Resize the trigger and reposition InsideAnchor/ExitAnchor/InspectionAnchor to match your model, then assign a replacement model prefab on ReplaceableModelSlot.");
        }

        private static GameObject CreateBaseInteractable(string name, Vector3 visualsScale, Color color)
        {
            GameObject logicObj = new GameObject(name + "_Logic");
            
            // Posicionar davant la càmera de l'escena
            SceneView view = SceneView.lastActiveSceneView;
            if (view != null)
            {
                logicObj.transform.position = view.pivot;
            }

            // Visuals (Mesh + Collider al fill)
            GameObject visualObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visualObj.name = name + "_Visuals";
            visualObj.transform.SetParent(logicObj.transform);
            visualObj.transform.localPosition = Vector3.zero;
            visualObj.transform.localRotation = Quaternion.identity;
            visualObj.transform.localScale = visualsScale;

            // Assignar Material URP
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader != null)
            {
                Material mat = new Material(urpShader);
                mat.color = color;
                visualObj.GetComponent<Renderer>().material = mat;
            }

            return logicObj;
        }

        private static void ConfigureOutlineTarget(GameObject owner, params Renderer[] renderers)
        {
            SelectionOutlineTarget target = owner.GetComponent<SelectionOutlineTarget>();
            if (target == null) target = owner.AddComponent<SelectionOutlineTarget>();

            SerializedObject serializedTarget = new SerializedObject(target);
            SerializedProperty rendererProperty = serializedTarget.FindProperty("_renderers");
            rendererProperty.arraySize = renderers.Length;
            for (int i = 0; i < renderers.Length; i++)
                rendererProperty.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
            serializedTarget.ApplyModifiedProperties();
        }

        private static void FinalizeCreation(GameObject obj)
        {
            // Registrar perquè l'usuari pugui fer CTRL+Z
            Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
            Selection.activeGameObject = obj;
        }
    }
}
