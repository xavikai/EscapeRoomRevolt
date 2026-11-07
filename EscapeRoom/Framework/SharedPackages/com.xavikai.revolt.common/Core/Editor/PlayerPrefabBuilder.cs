using UnityEngine;
using UnityEditor;
using EscapeRoomRevolt.Player.PC;
using EscapeRoomRevolt.Systems.Interaction;
using EscapeRoomRevolt.Systems.Inventory;

namespace EscapeRoomRevolt.EditorTools
{
    public class PlayerPrefabBuilder
    {
        // Kept as an internal legacy migration helper. It must not appear in the menu because
        // it overwrites the production prefab with an incomplete PC-only hierarchy.
        public static void CreatePlayerPrefab()
        {
            // 1. Creem l'objecte arrel
            GameObject playerRoot = new GameObject("Player_PC");
            
            // 2. Afegim CharacterController amb mides realistes (1.8m d'alçada)
            CharacterController cc = playerRoot.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0, 0.9f, 0);

            // 3. Afegim els scripts del jugador
            PlayerMovement movement = playerRoot.AddComponent<PlayerMovement>();
            playerRoot.AddComponent<PlayerInputHandler>();

            // 4. Creem la càmera
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.transform.SetParent(playerRoot.transform);
            cameraObj.transform.localPosition = new Vector3(0, 1.6f, 0); // Alçada dels ulls

            // 5. Afegim components a la càmera
            Camera cam = cameraObj.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cameraObj.AddComponent<AudioListener>();
            cameraObj.AddComponent<InteractionManager>();

            // Vinculem la càmera a l'script de moviment a través de l'Editor API
            SerializedObject so = new SerializedObject(movement);
            so.FindProperty("_playerCamera").objectReferenceValue = cameraObj.transform;
            so.ApplyModifiedProperties();

            // 6. Guardem com a Prefab
            string path = "Assets/_EscapeRoomTemplate/Prefabs/Player_PC.prefab";
            
            // Creem la carpeta si no existeix
            if (!AssetDatabase.IsValidFolder("Assets/_EscapeRoomTemplate/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets/_EscapeRoomTemplate", "Prefabs");
            }

            PrefabUtility.SaveAsPrefabAsset(playerRoot, path);
            GameObject.DestroyImmediate(playerRoot); // Esborrem l'objecte temporal de l'escena

            Debug.Log($"[EscapeRoom] Prefab 'Player_PC' generat correctament a: {path}");
        }

        // Legacy demo generator. The commercial sample assets are versioned and must not be recreated in place.
        public static void GenerateItems()
        {
            string folderPath = "Assets/_EscapeRoomTemplate/ScriptableObjects/Items";
            if (!AssetDatabase.IsValidFolder("Assets/_EscapeRoomTemplate/ScriptableObjects"))
                AssetDatabase.CreateFolder("Assets/_EscapeRoomTemplate", "ScriptableObjects");
            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder("Assets/_EscapeRoomTemplate/ScriptableObjects", "Items");

            // 1. Clau (Key)
            InventoryItemData keyItem = ScriptableObject.CreateInstance<InventoryItemData>();
            keyItem.name = "Key_Office";
            // Ens aprofitem de l'Editor per omplir camps privats o públics
            SerializedObject keySo = new SerializedObject(keyItem);
            keySo.FindProperty("_itemId").stringValue = "key_office_01";
            keySo.FindProperty("_displayName").stringValue = "Rusty Key";
            keySo.FindProperty("_description").stringValue = "An old key. Probably opens a drawer.";
            keySo.FindProperty("_isStackable").boolValue = false;
            keySo.ApplyModifiedProperties();
            
            AssetDatabase.CreateAsset(keyItem, $"{folderPath}/Key_Office.asset");

            // 2. Fusible (Fuse)
            InventoryItemData fuseItem = ScriptableObject.CreateInstance<InventoryItemData>();
            fuseItem.name = "Fuse_15A";
            SerializedObject fuseSo = new SerializedObject(fuseItem);
            fuseSo.FindProperty("_itemId").stringValue = "fuse_15A";
            fuseSo.FindProperty("_displayName").stringValue = "15A Fuse";
            fuseSo.FindProperty("_description").stringValue = "A standard 15 amp fuse for an electrical panel.";
            fuseSo.FindProperty("_isStackable").boolValue = false;
            fuseSo.ApplyModifiedProperties();

            AssetDatabase.CreateAsset(fuseItem, $"{folderPath}/Fuse_15A.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[EscapeRoom] Items generats correctament a: {folderPath}");
        }
    }
}
