using System;
using EscapeRoomRevolt.Core.Settings;
using UnityEngine;

namespace EscapeRoomRevolt.Core.Localization
{
    /// <summary>
    /// Resolves localization keys against a LocalizationCatalog and persists the chosen language
    /// via GameSettingsData.languageCode. Created automatically by Bootstrapper. Call sites use the
    /// static Tr(key) helper rather than reaching for Instance directly.
    /// </summary>
    public sealed class LocalizationService : MonoBehaviour
    {
        private const string ResourcePath = "DefaultLocalizationCatalog";

        public static LocalizationService Instance { get; private set; }

        [SerializeField] private LocalizationCatalog _catalog;
        private string _languageCode;

        public string CurrentLanguage => _languageCode;
        public System.Collections.Generic.List<string> AvailableLanguages() =>
            _catalog != null ? _catalog.AvailableLanguages() : new System.Collections.Generic.List<string> { "es" };
        public event Action LanguageChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (_catalog == null) _catalog = Resources.Load<LocalizationCatalog>(ResourcePath);
            _languageCode = GameSettingsService.Instance != null && !string.IsNullOrEmpty(GameSettingsService.Instance.Data.languageCode)
                ? GameSettingsService.Instance.Data.languageCode
                : (_catalog != null ? _catalog.fallbackLanguageCode : "es");
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void SetLanguage(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode) || _languageCode == languageCode) return;
            _languageCode = languageCode;
            if (GameSettingsService.Instance != null)
            {
                GameSettingsService.Instance.Data.languageCode = languageCode;
                GameSettingsService.Instance.ApplyAndSave(GameSettingsService.Instance.Data);
            }
            LanguageChanged?.Invoke();
        }

        public string Get(string key) => _catalog != null ? _catalog.Get(key, _languageCode) : key;

        /// <summary>Shorthand for Instance.Get(key); returns the key itself if no service exists yet (e.g. an isolated test scene), so a missing service degrades to the original literal instead of throwing.</summary>
        public static string Tr(string key) => Instance != null ? Instance.Get(key) : key;
    }
}
