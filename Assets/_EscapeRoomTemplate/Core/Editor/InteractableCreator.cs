using UnityEngine;
using UnityEditor;
using EscapeRoomRevolt.Systems.Interaction;
using EscapeRoomRevolt.Systems.Puzzle;
using EscapeRoomRevolt.Systems.Inventory;

namespace EscapeRoomRevolt.EditorTools
{
    public static class InteractableCreator
    {
        [MenuItem("EscapeRoom/Create/Door", priority = 10)]
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

        [MenuItem("EscapeRoom/Create/Cabinet", priority = 11)]
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

        [MenuItem("EscapeRoom/Create/Drawer", priority = 12)]
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

        [MenuItem("EscapeRoom/Create/Keypad Panel", priority = 13)]
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
            GameObject displayObj = new GameObject("Display");
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
            
            SerializedProperty highlightProp = soKeypad.FindProperty("_highlightRenderers");
            highlightProp.arraySize = 1;
            highlightProp.GetArrayElementAtIndex(0).objectReferenceValue = baseRenderer;

            soKeypad.ApplyModifiedProperties();

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
            GameObject textObj = new GameObject("Text");
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
            SerializedObject soBtn = new SerializedObject(btnScript);
            SerializedProperty btnHighlightProp = soBtn.FindProperty("_highlightRenderers");
            btnHighlightProp.arraySize = 1;
            btnHighlightProp.GetArrayElementAtIndex(0).objectReferenceValue = visualObj.GetComponent<Renderer>();
            soBtn.ApplyModifiedProperties();

            return logicObj;
        }

        [MenuItem("EscapeRoom/Create/Fixed Note", priority = 12)]
        public static void CreateNote()
        {
            GameObject logicObj = CreateBaseInteractable("NewNote", new Vector3(0.3f, 0.05f, 0.4f), Color.white);
            
            logicObj.AddComponent<InteractableNote>();

            FinalizeCreation(logicObj);
        }

        [MenuItem("EscapeRoom/Create/Pickable Item", priority = 13)]
        public static void CreatePickableItem()
        {
            GameObject logicObj = CreateBaseInteractable("NewPickableItem", new Vector3(0.2f, 0.2f, 0.2f), Color.yellow);
            
            logicObj.AddComponent<PickableItem>();

            FinalizeCreation(logicObj);
        }

        [MenuItem("EscapeRoom/Create/Generic Trigger (Button)", priority = 13)]
        public static void CreateTrigger()
        {
            GameObject logicObj = CreateBaseInteractable("NewTrigger", new Vector3(0.2f, 0.2f, 0.2f), Color.red);
            
            logicObj.AddComponent<InteractableTrigger>();

            FinalizeCreation(logicObj);
        }

        [MenuItem("EscapeRoom/Create/Item Receiver", priority = 14)]
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

        private static void FinalizeCreation(GameObject obj)
        {
            // Registrar perquè l'usuari pugui fer CTRL+Z
            Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
            Selection.activeGameObject = obj;
        }
    }
}
