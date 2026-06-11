using UnityEngine;
using UnityEditor;
using EscapeRoomRevolt.Systems.Inventory;

namespace EscapeRoomRevolt.Editor
{
    public static class ExamineChamberSetup
    {
        [MenuItem("EscapeRoom/1. Setup/Create Examine Chamber", priority = 13)]
        public static void CreateExamineChamber()
        {
            // 1. Check if it already exists
            if (Object.FindObjectOfType<ExamineChamber>() != null)
            {
                Debug.LogWarning("An Examine Chamber already exists in the scene.");
                return;
            }

            // 2. Create the Chamber root
            GameObject chamber = new GameObject("ExamineChamber");
            chamber.transform.position = new Vector3(0, 1000, 0); // Hide it far away
            var chamberComp = chamber.AddComponent<ExamineChamber>();

            // 3. Create the Spawn Point
            GameObject spawnPoint = new GameObject("SpawnPoint");
            spawnPoint.transform.SetParent(chamber.transform);
            spawnPoint.transform.localPosition = Vector3.zero;

            // Link the spawn point via SerializedObject since the field is private
            var so = new SerializedObject(chamberComp);
            so.FindProperty("_spawnPoint").objectReferenceValue = spawnPoint.transform;
            so.ApplyModifiedProperties();

            // 4. Create the Camera
            GameObject camObj = new GameObject("ExamineCamera");
            camObj.transform.SetParent(chamber.transform);
            camObj.transform.localPosition = new Vector3(0, 0, -2);
            var cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0f); // Transparent background if possible

            // Create a dedicated RenderTexture
            string rtPath = "Assets/_EscapeRoomTemplate/Settings/ExamineRenderTexture.renderTexture";
            RenderTexture rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(rtPath);
            if (rt == null)
            {
                // Ensure directory exists
                if (!AssetDatabase.IsValidFolder("Assets/_EscapeRoomTemplate/Settings"))
                {
                    AssetDatabase.CreateFolder("Assets/_EscapeRoomTemplate", "Settings");
                }

                rt = new RenderTexture(1024, 1024, 24);
                rt.name = "ExamineRenderTexture";
                AssetDatabase.CreateAsset(rt, rtPath);
                AssetDatabase.SaveAssets();
            }

            cam.targetTexture = rt;

            // 5. Create a Light
            GameObject lightObj = new GameObject("ExamineLight");
            lightObj.transform.SetParent(chamber.transform);
            lightObj.transform.localPosition = new Vector3(1, 2, -1);
            lightObj.transform.localRotation = Quaternion.Euler(50, -30, 0);
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;

            // Select it to show the user
            Selection.activeGameObject = chamber;
            
            Debug.Log("Examine Chamber created successfully! You can now use the ExamineRenderTexture in your UI.");
        }
    }
}
