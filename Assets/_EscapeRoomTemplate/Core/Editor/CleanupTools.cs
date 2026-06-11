using UnityEngine;
using UnityEditor;

namespace EscapeRoomRevolt.EditorTools
{
    public class CleanupTools
    {
        [MenuItem("EscapeRoom/4. Utils/Remove Missing Scripts", priority = 42)]
        public static void RemoveMissingScripts()
        {
            var gameObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
            int count = 0;

            foreach (var go in gameObjects)
            {
                int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                if (removed > 0)
                {
                    count += removed;
                    EditorUtility.SetDirty(go);
                }
            }

            if (count > 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                Debug.Log($"[CleanupTools] Removed {count} missing scripts from the scene.");
            }
            else
            {
                Debug.Log("[CleanupTools] No missing scripts found.");
            }
        }
    }
}
