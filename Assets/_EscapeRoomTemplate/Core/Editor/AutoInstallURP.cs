using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace EscapeRoomRevolt.EditorTools
{
    [InitializeOnLoad]
    public class AutoInstallURP
    {
        static AddRequest Request;
        
        static AutoInstallURP()
        {
            // Evitar que s'instal·li múltiples vegades cada cop que Unity compila
            if (SessionState.GetBool("URP_Installed", false)) return;
            
            Debug.Log("[IA] Començant instal·lació automàtica de URP...");
            Request = Client.Add("com.unity.render-pipelines.universal");
            EditorApplication.update += Progress;
        }

        static void Progress()
        {
            if (Request.IsCompleted)
            {
                if (Request.Status == StatusCode.Success)
                    Debug.Log("[IA] ✅ URP Instal·lat correctament de forma automàtica!");
                else if (Request.Status >= StatusCode.Failure)
                    Debug.Log("[IA] ❌ Error instal·lant URP: " + Request.Error.message);

                EditorApplication.update -= Progress;
                SessionState.SetBool("URP_Installed", true);
            }
        }
    }
}
