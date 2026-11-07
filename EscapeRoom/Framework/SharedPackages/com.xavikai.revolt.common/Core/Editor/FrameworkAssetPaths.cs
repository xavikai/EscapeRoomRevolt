#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EscapeRoomRevolt.EditorTools
{
    /// <summary>Project overrides take precedence over immutable shared package templates.</summary>
    public static class FrameworkAssetPaths
    {
        private const string LegacyRoot = "Assets/_EscapeRoomTemplate/";
        public static string Resolve(string path)
        {
            if (AssetDatabase.LoadMainAssetAtPath(path) != null || !path.StartsWith(LegacyRoot)) return path;
            string relative = path.Substring(LegacyRoot.Length);
            if (relative.StartsWith("Systems/Survival/"))
                return "Packages/com.xavikai.revolt.survival/" + relative.Substring("Systems/Survival/".Length);
            return "Packages/com.xavikai.revolt.common/" + relative;
        }

        public static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(Resolve(path));
    }
}
#endif
