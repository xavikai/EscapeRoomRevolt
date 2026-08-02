using EscapeRoomRevolt.UI.Toolkit;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EscapeRoomRevolt.Core.Editor
{
    public static class InventoryUISetup
    {
        [MenuItem("Escape Room Framework/Documentation/Locate Gameplay HUD", priority = 902)]
        public static void SelectGameplayHud()
        {
            GameplayUIController controller = Object.FindAnyObjectByType<GameplayUIController>();
            if (controller != null)
            {
                Selection.activeGameObject = controller.gameObject;
                EditorGUIUtility.PingObject(controller.gameObject);
                return;
            }

            VisualTreeAsset hud = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_EscapeRoomTemplate/UI/Toolkit/GameplayHUD.uxml");
            Selection.activeObject = hud;
            Debug.LogWarning("No GameplayUIController found. Instantiate the GameManager prefab; it now owns the UI Toolkit HUD.");
        }
    }
}
