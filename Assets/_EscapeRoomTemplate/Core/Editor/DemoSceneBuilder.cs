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
using TMPro;

namespace EscapeRoomRevolt.EditorTools
{
    public class DemoSceneBuilder
    {
        [MenuItem("EscapeRoom / Build Demo Scene (Locked Office)")]
        public static void CreateDemoScene()
        {
            if (!EditorUtility.DisplayDialog("Crear Escena Demo", "Això crearà i sobreescriurà l'escena 'LockedOffice'. Vols continuar?", "Sí", "Cancel·la"))
                return;

            // 1. Crear nova escena
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // 2. GameManager (Bootstrapper + EventBus logic + Inventory)
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<Bootstrapper>();
            gm.AddComponent<InventoryManager>();

            // 3. UI Canvas
            GameObject canvas = new GameObject("UI_Canvas");
            Canvas c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // EventSystem per la UI
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            
            GameObject crosshair = new GameObject("Crosshair");
            crosshair.transform.SetParent(canvas.transform);
            var img = crosshair.AddComponent<UnityEngine.UI.Image>();
            img.rectTransform.sizeDelta = new Vector2(4, 4);
            img.rectTransform.anchoredPosition = Vector2.zero;

            GameObject promptObj = new GameObject("PromptText");
            promptObj.transform.SetParent(canvas.transform);
            var text = promptObj.AddComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.rectTransform.sizeDelta = new Vector2(400, 50);
            text.rectTransform.anchoredPosition = new Vector2(0, -40);
            
            var promptUI = canvas.AddComponent<InteractionPromptUI>();
            SerializedObject soPrompt = new SerializedObject(promptUI);
            soPrompt.FindProperty("_promptText").objectReferenceValue = text;
            soPrompt.FindProperty("_promptContainer").objectReferenceValue = promptObj;
            soPrompt.ApplyModifiedProperties();

            // UIManager
            UIManager uiManager = canvas.AddComponent<UIManager>();
            
            // Keypad UI (Basic Canvas Layout for Demo)
            GameObject keypadPanel = new GameObject("Keypad_Panel");
            keypadPanel.transform.SetParent(canvas.transform);
            var keypadRect = keypadPanel.AddComponent<RectTransform>();
            keypadRect.sizeDelta = new Vector2(300, 400);
            keypadRect.anchoredPosition = Vector2.zero;
            
            // Fons del teclat
            var bgImage = keypadPanel.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            GameObject displayObj = new GameObject("DisplayText");
            displayObj.transform.SetParent(keypadPanel.transform);
            var displayText = displayObj.AddComponent<TextMeshProUGUI>();
            displayText.alignment = TextAlignmentOptions.Center;
            displayText.fontSize = 36;
            displayText.rectTransform.sizeDelta = new Vector2(280, 50);
            displayText.rectTransform.anchoredPosition = new Vector2(0, 150);
            displayText.text = "0000";

            var keypadScript = keypadPanel.AddComponent<KeypadUI>();
            SerializedObject soKeypad = new SerializedObject(keypadScript);
            soKeypad.FindProperty("_displayText").objectReferenceValue = displayText;
            soKeypad.ApplyModifiedProperties();

            SerializedObject soUI = new SerializedObject(uiManager);
            soUI.FindProperty("_keypadPanel").objectReferenceValue = keypadPanel;
            keypadPanel.SetActive(false);

            // Note UI Panel
            GameObject notePanel = new GameObject("Note_Panel");
            notePanel.transform.SetParent(canvas.transform);
            var noteRect = notePanel.AddComponent<RectTransform>();
            noteRect.sizeDelta = new Vector2(500, 300);
            noteRect.anchoredPosition = Vector2.zero;

            var noteBg = notePanel.AddComponent<UnityEngine.UI.Image>();
            noteBg.color = new Color(0.9f, 0.9f, 0.8f, 1f); // Color paper

            GameObject noteTextObj = new GameObject("NoteText");
            noteTextObj.transform.SetParent(notePanel.transform);
            var noteTextDisplay = noteTextObj.AddComponent<TextMeshProUGUI>();
            noteTextDisplay.alignment = TextAlignmentOptions.Center;
            noteTextDisplay.fontSize = 24;
            noteTextDisplay.color = Color.black;
            noteTextDisplay.rectTransform.sizeDelta = new Vector2(460, 260);
            noteTextDisplay.rectTransform.anchoredPosition = Vector2.zero;

            var noteScript = notePanel.AddComponent<NoteReaderUI>();
            SerializedObject soNoteUI = new SerializedObject(noteScript);
            soNoteUI.FindProperty("_noteTextDisplay").objectReferenceValue = noteTextDisplay;
            soNoteUI.ApplyModifiedProperties();

            soUI.FindProperty("_noteReaderPanel").objectReferenceValue = notePanel;
            soUI.ApplyModifiedProperties();
            notePanel.SetActive(false);

            // 4. Geometria de l'habitació (Terra i Parets)
            GameObject env = new GameObject("Environment");
            
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(env.transform);
            floor.transform.localScale = new Vector3(10, 0.5f, 10);
            floor.transform.position = new Vector3(0, -0.25f, 0);
            AssignURPMaterial(floor, Color.gray);

            // Taula
            GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "Table";
            table.transform.SetParent(env.transform);
            table.transform.localScale = new Vector3(2, 1, 1);
            table.transform.position = new Vector3(0, 0.5f, 3);
            AssignURPMaterial(table, new Color(0.6f, 0.3f, 0.1f));

            // 5. Jugador
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_EscapeRoomTemplate/Prefabs/Player_PC.prefab");
            if (playerPrefab != null)
            {
                GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                player.transform.position = new Vector3(0, 0.5f, -2);
                
                // Forçar la capa d'interacció a "Everything" perquè sempre funcioni al generar
                var interactionManager = player.GetComponentInChildren<InteractionManager>();
                if (interactionManager != null)
                {
                    SerializedObject soIM = new SerializedObject(interactionManager);
                    soIM.FindProperty("_interactableLayer").intValue = ~0; // -1 or ~0 in bitmask means Everything
                    soIM.ApplyModifiedProperties();
                }

                // Esborrem la Main Camera per defecte
                GameObject defaultCam = GameObject.Find("Main Camera");
                if (defaultCam != null && defaultCam.transform.parent == null)
                    GameObject.DestroyImmediate(defaultCam);
            }
            else
            {
                Debug.LogWarning("Cal generar el prefab del Player_PC primer!");
            }

            // 6. La Clau
            InventoryItemData keyData = AssetDatabase.LoadAssetAtPath<InventoryItemData>("Assets/_EscapeRoomTemplate/ScriptableObjects/Items/Key_Office.asset");
            if (keyData != null)
            {
                GameObject keyObj = CreateInteractableObject("Key_Office", new Vector3(0, 1.05f, 3), new Vector3(0.2f, 0.1f, 0.2f), Color.yellow);
                
                var pickable = keyObj.AddComponent<PickableItem>();
                SerializedObject soKey = new SerializedObject(pickable);
                soKey.FindProperty("_itemData").objectReferenceValue = keyData;
                soKey.FindProperty("_destroyOnPickup").boolValue = true;
                soKey.ApplyModifiedProperties();
            }

            // 6.5 La Nota amb la Pista
            GameObject noteObj = CreateInteractableObject("ClueNote", new Vector3(0.5f, 1.05f, 3), new Vector3(0.3f, 0.05f, 0.4f), Color.white);
            var note = noteObj.AddComponent<FixedNote>();
            SerializedObject soNote = new SerializedObject(note);
            soNote.FindProperty("_content").stringValue = "The boss changed the safe code again. It's the year the company was founded: 1984.";
            soNote.ApplyModifiedProperties();

            // 6.6 La Caixa Forta
            GameObject safeObj = CreateInteractableObject("Safe", new Vector3(-2, 0.4f, 4), new Vector3(0.8f, 0.8f, 0.8f), Color.black);
            
            var codePuzzle = safeObj.AddComponent<CodePanelPuzzle>();
            SerializedObject soCode = new SerializedObject(codePuzzle);
            soCode.FindProperty("_correctCode").stringValue = "1984";
            soCode.FindProperty("_maxCodeLength").intValue = 4;
            soCode.ApplyModifiedProperties();
            
            safeObj.AddComponent<InteractableKeypad>();

            // 7. La Porta Final
            GameObject door = CreateInteractableObject("FinalDoor", new Vector3(0, 1.25f, 4.9f), new Vector3(1.5f, 2.5f, 0.2f), new Color(0.8f, 0.2f, 0.2f));

            // Pivot personalitzat per moure lliurement
            GameObject hinge = new GameObject("CustomPivot");
            hinge.transform.SetParent(door.transform);
            hinge.transform.localPosition = new Vector3(-0.75f, 0, 0); // Extrem esquerre per defecte

            var doorScript = door.AddComponent<Door>();
            SerializedObject soDoor = new SerializedObject(doorScript);
            soDoor.FindProperty("_isLocked").boolValue = true;
            soDoor.FindProperty("_requiredItemId").stringValue = "key_office_01";
            soDoor.FindProperty("_customPivot").objectReferenceValue = hinge.transform;
            soDoor.ApplyModifiedProperties();

            // 8. Guardar escena
            string scenePath = "Assets/_EscapeRoomTemplate/Scenes/LockedOffice.unity";
            if (!AssetDatabase.IsValidFolder("Assets/_EscapeRoomTemplate/Scenes"))
                AssetDatabase.CreateFolder("Assets/_EscapeRoomTemplate", "Scenes");

            EditorSceneManager.SaveScene(newScene, scenePath);
            Debug.Log($"[EscapeRoom] Escena Demo muntada i guardada a: {scenePath}");
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
            // 1. Parent (Logic)
            GameObject logicObj = new GameObject(name + "_Logic");
            logicObj.transform.position = position;
            logicObj.transform.localScale = Vector3.one; // EVITEM DISTORSIONS
            
            // 2. Child (Visuals + Mesh + Collider)
            GameObject visualObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visualObj.name = name + "_Visuals";
            
            visualObj.transform.SetParent(logicObj.transform);
            visualObj.transform.localPosition = Vector3.zero;
            visualObj.transform.localRotation = Quaternion.identity;
            visualObj.transform.localScale = scale; // L'escala s'aplica als visuals

            AssignURPMaterial(visualObj, color);

            return logicObj;
        }
    }
}
