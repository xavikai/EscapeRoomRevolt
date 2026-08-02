using UnityEngine;
using UnityEditor;
using EscapeRoomRevolt.Systems.Interaction;

namespace EscapeRoomRevolt.EditorTools
{
    public class SaveSystemTools
    {
        [MenuItem("Escape Room Framework/Validation/Validate Save IDs", priority = 702)]
        public static void FixSaveIds()
        {
            int count = 0;
            var interactables = GameObject.FindObjectsByType<InteractableBase>(FindObjectsInactive.Include);
            
            var usedIds = new System.Collections.Generic.HashSet<string>();
            int duplicates = 0;

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

                string finalId = string.IsNullOrEmpty(item.SaveId) ? item.name : item.SaveId;
                if (!usedIds.Add(finalId))
                {
                    Debug.LogError($"[SaveSystem] CRITICAL ERROR: Duplicate Save ID found: '{finalId}' on GameObject '{item.name}'. This will cause objects to disappear when loading! Please select this object and change its Save ID in the Inspector.");
                    duplicates++;
                }
            }
            
            if (count > 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                Debug.Log($"[SaveSystem] Assigned unique GUIDs to {count} interactable objects.");
            }
            else if (duplicates == 0)
            {
                Debug.Log("[SaveSystem] All interactable objects already have unique Save IDs.");
            }
        }
    }
}
