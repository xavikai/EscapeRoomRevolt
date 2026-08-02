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
            File.WriteAllText(Path.Combine(Application.persistentDataPath, FileName), JsonUtility.ToJson(Data, true));
        }

        private void Load()
        {
            string path = Path.Combine(Application.persistentDataPath, FileName);
            if (File.Exists(path)) Data = JsonUtility.FromJson<GameSettingsData>(File.ReadAllText(path)) ?? new GameSettingsData();
            Apply();
        }

        private void Apply()
        {
            AudioListener.volume = Data.masterVolume;
            if (Data.qualityLevel >= 0 && Data.qualityLevel < QualitySettings.names.Length)
                QualitySettings.SetQualityLevel(Data.qualityLevel, true);
            Screen.fullScreen = Data.fullscreen;
        }
    }
}
