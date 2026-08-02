#if UNITY_EDITOR
using EscapeRoomRevolt.Core.Settings;
using UnityEditor;
using UnityEngine;

namespace EscapeRoomRevolt.EditorTools
{
    public static class GenreFeatureSetup
    {
        private const string Root = "Escape Room Framework/Configuration/";
        private const string SettingsPath = "Assets/_EscapeRoomTemplate/Resources/GenreFeatureSettings.asset";

        [MenuItem(Root + "Use Escape Room Profile", priority = 1)]
        public static void UseEscapeRoomProfile() => ApplyProfile(GameGenre.EscapeRoom);

        [MenuItem(Root + "Use Survival Horror Profile", priority = 2)]
        public static void UseSurvivalHorrorProfile() => ApplyProfile(GameGenre.SurvivalHorror);

        [MenuItem(Root + "Use Custom Hybrid Profile", priority = 3)]
        public static void UseCustomHybridProfile() => ApplyProfile(GameGenre.CustomHybrid);

        [MenuItem(Root + "Select Genre Feature Settings", priority = 10)]
        public static void SelectSettings()
        {
            GenreFeatureSettings settings = EnsureSettings();
            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
        }

        [MenuItem(Root + "Use Escape Room Profile", true)]
        private static bool ValidateEscapeRoomProfile() => SetChecked(GameGenre.EscapeRoom);

        [MenuItem(Root + "Use Survival Horror Profile", true)]
        private static bool ValidateSurvivalHorrorProfile() => SetChecked(GameGenre.SurvivalHorror);

        [MenuItem(Root + "Use Custom Hybrid Profile", true)]
        private static bool ValidateCustomHybridProfile() => SetChecked(GameGenre.CustomHybrid);

        private static void ApplyProfile(GameGenre genre)
        {
            GenreFeatureSettings settings = EnsureSettings();
            Undo.RecordObject(settings, "Change Game Genre Profile");
            settings.SetProfile(genre);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
            Debug.Log($"[Escape Room Framework] Project genre set to {genre}. Changes apply on the next Play session.", settings);
        }

        private static bool SetChecked(GameGenre genre)
        {
            GenreFeatureSettings settings = AssetDatabase.LoadAssetAtPath<GenreFeatureSettings>(SettingsPath);
            bool selected = settings != null && settings.Genre == genre;
            string item = genre switch
            {
                GameGenre.EscapeRoom => Root + "Use Escape Room Profile",
                GameGenre.SurvivalHorror => Root + "Use Survival Horror Profile",
                _ => Root + "Use Custom Hybrid Profile"
            };
            Menu.SetChecked(item, selected);
            return true;
        }

        private static GenreFeatureSettings EnsureSettings()
        {
            GenreFeatureSettings settings = AssetDatabase.LoadAssetAtPath<GenreFeatureSettings>(SettingsPath);
            if (settings != null) return settings;

            settings = ScriptableObject.CreateInstance<GenreFeatureSettings>();
            AssetDatabase.CreateAsset(settings, SettingsPath);
            AssetDatabase.SaveAssets();
            return settings;
        }
    }
}
#endif
