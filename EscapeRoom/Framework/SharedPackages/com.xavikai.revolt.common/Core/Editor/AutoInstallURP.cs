using UnityEditor;
using UnityEngine;

namespace EscapeRoomRevolt.EditorTools
{
    /// <summary>Dependency check only. Commercial packages must never modify Package Manager on domain reload.</summary>
    public static class AutoInstallURP
    {
        [MenuItem("Escape Room Framework/Validation/Check Render Pipeline Dependency", priority = 704)]
        public static void CheckDependency()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                Debug.LogWarning("[Escape Room Framework] URP is not available. Install and configure it manually through Package Manager before using the supplied renderer features.");
                return;
            }

            Debug.Log("[Escape Room Framework] URP dependency is available. No project settings were modified.");
        }
    }
}
