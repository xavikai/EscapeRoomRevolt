using UnityEngine;
using UnityEditor;
using EscapeRoomRevolt.Systems.Interaction;

namespace EscapeRoomRevolt.EditorTools
{
    public class SaveSystemTools
    {
        [MenuItem("EscapeRoom / Fix Missing Save IDs")]
        public static void FixSaveIds()
        {
            int count = 0;
            var interactables = GameObject.FindObjectsByType<InteractableBase>(FindObjectsInactive.Include);
            
            foreach (var item in interactables)
            {
                SerializedObject so = new SerializedObject(item);
                SerializedProperty saveIdProp = so.FindProperty("_saveId");
                
                if (saveIdProp != null && string.IsNullOrEmpty(saveIdProp.stringValue))
                {
                    saveIdProp.stringValue = System.Guid.NewGuid().ToString();
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(item);
                    count++;
                }
            }
            
            if (count > 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                Debug.Log($"[SaveSystem] Assigned unique GUIDs to {count} interactable objects.");
            }
            else
            {
                Debug.Log("[SaveSystem] All interactable objects already have unique Save IDs.");
            }
        }
    }
}
