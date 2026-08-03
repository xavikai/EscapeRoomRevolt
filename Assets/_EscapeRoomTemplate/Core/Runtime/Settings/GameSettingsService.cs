using System;
using System.IO;
using UnityEngine;

namespace EscapeRoomRevolt.Core.Settings
{
    [Serializable]
    public class GameSettingsData
    {
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float musicVolume = 1f;
        [Range(0f, 1f)] public float sfxVolume = 1f;
        [Range(0.1f, 3f)] public float mouseSensitivity = 1f;
        public int qualityLevel = -1;
        public bool fullscreen = true;
        public bool subtitles = true;
        public bool reduceFlashes;
        [Tooltip("Caps intensity on any camera-shake effect that checks this flag. No shake effect exists in the base template yet — this is an integration point for buyer-authored content.")]
        public bool reduceScreenShake;
        [Tooltip("Halves the volume of horror-event stingers and enemy audio tells.")]
        public bool reduceLoudSounds;
        [Tooltip("Lets buyer-authored gore content tone itself down. No gore content exists in the base template — this is an integration point.")]
        public bool reduceGore;
        [Tooltip("Slightly slows enemy chase speed and shortens how long they remember your last position.")]
        public bool chaseAssistance;
        [Tooltip("Caps the intensity of the PC camera head bob while walking/running.")]
        public bool reduceHeadBob;
        [Tooltip("Forces the menu to a fixed high-contrast palette (black/white/yellow), overriding any assigned MenuThemeSettings so accessibility always wins over branding.")]
        public bool highContrastMode;
        [TextArea] public string bindingOverridesJson = string.Empty;
    }

    /// <summary>Persists player preferences separately from save games.</summary>
    public sealed class GameSettingsService : MonoBehaviour
    {
        public static GameSettingsService Instance { get; private set; }
        public GameSettingsData Data { get; private set; } = new GameSettingsData();

        private const string FileName = "settings.json";

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        public void ApplyAndSave(GameSettingsData data)
        {
            Data = data ?? new GameSettingsData();
            Apply();
            try
            {
                File.WriteAllText(Path.Combine(Application.persistentDataPath, FileName), JsonUtility.ToJson(Data, true));
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[GameSettingsService] Could not save settings.json: {exception.Message}");
            }
        }

        private void Load()
        {
            string path = Path.Combine(Application.persistentDataPath, FileName);
            if (File.Exists(path))
            {
                try
                {
                    Data = JsonUtility.FromJson<GameSettingsData>(File.ReadAllText(path)) ?? new GameSettingsData();
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"[GameSettingsService] Could not read settings.json, using defaults: {exception.Message}");
                    Data = new GameSettingsData();
                }
            }
            Apply();
        }

        private void Apply()
        {
            if (Data.qualityLevel >= 0 && Data.qualityLevel < QualitySettings.names.Length)
                QualitySettings.SetQualityLevel(Data.qualityLevel, true);
            Screen.fullScreen = Data.fullscreen;
        }
    }
}
