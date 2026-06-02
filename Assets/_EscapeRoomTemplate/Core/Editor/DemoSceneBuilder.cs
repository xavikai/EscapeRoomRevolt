using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Player.PC;
using EscapeRoomRevolt.UI.PC;
using EscapeRoomRevolt.Systems.Interaction;
using EscapeRoomRevolt.Systems.Inventory;
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
            
            // 2. GameManager (Bootstrapper + EventBus logic)
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<Bootstrapper>();

            // 3. UI Canvas
            GameObject canvas = new GameObject("UI_Canvas");
            Canvas c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
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
            canvas.AddComponent<UIManager>();

            // 4. Geometria de l'habitació (Terra i Parets)
            GameObject env = new GameObject("Environment");
            
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(env.transform);
            floor.transform.localScale = new Vector3(10, 0.5f, 10);
            floor.transform.position = new Vector3(0, -0.25f, 0);

            // Taula
            GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "Table";
            table.transform.SetParent(env.transform);
            table.transform.localScale = new Vector3(2, 1, 1);
            table.transform.position = new Vector3(0, 0.5f, 3);

            // 5. Jugador
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_EscapeRoomTemplate/Prefabs/Player_PC.prefab");
            if (playerPrefab != null)
            {
                GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                player.transform.position = new Vector3(0, 0.5f, -2);
                
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
                GameObject keyObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                keyObj.name = "Key_Office";
                keyObj.transform.localScale = new Vector3(0.2f, 0.1f, 0.2f);
                keyObj.transform.position = new Vector3(0, 1.05f, 3); // Sobre la taula
                keyObj.GetComponent<Renderer>().sharedMaterial.color = Color.yellow; // Per distingir-la
                
                var pickable = keyObj.AddComponent<PickableItem>();
                SerializedObject soKey = new SerializedObject(pickable);
                soKey.FindProperty("_itemData").objectReferenceValue = keyData;
                soKey.FindProperty("_destroyOnPickup").boolValue = true;
                soKey.ApplyModifiedProperties();
            }

            // 7. La Porta Final
            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "Final Door";
            door.transform.localScale = new Vector3(1.5f, 2.5f, 0.2f);
            door.transform.position = new Vector3(0, 1.25f, 4.9f);
            door.GetComponent<Renderer>().sharedMaterial.color = Color.red;
            
            var doorScript = door.AddComponent<Door>();
            SerializedObject soDoor = new SerializedObject(doorScript);
            soDoor.FindProperty("_isLocked").boolValue = true;
            soDoor.FindProperty("_requiredItemId").stringValue = "key_office_01"; // El mateix ID que l'ítem
            soDoor.ApplyModifiedProperties();

            // 8. Guardar escena
            string scenePath = "Assets/_EscapeRoomTemplate/Scenes/LockedOffice.unity";
            if (!AssetDatabase.IsValidFolder("Assets/_EscapeRoomTemplate/Scenes"))
                AssetDatabase.CreateFolder("Assets/_EscapeRoomTemplate", "Scenes");

            EditorSceneManager.SaveScene(newScene, scenePath);
            Debug.Log($"[EscapeRoom] Escena Demo muntada i guardada a: {scenePath}");
        }
    }
}
