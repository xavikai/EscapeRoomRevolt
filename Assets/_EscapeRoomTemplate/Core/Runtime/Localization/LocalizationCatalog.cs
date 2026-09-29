using System;
using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomRevolt.Core.Localization
{
    [Serializable]
    public sealed class LocalizedString
    {
        [Tooltip("ISO-ish language code, e.g. \"es\" or \"en\".")]
        public string languageCode = "es";
        [TextArea] public string text = "";
    }

    [Serializable]
    public sealed class LocalizationEntry
    {
        [Tooltip("Looked up verbatim by LocalizationService.Tr(key). By convention in this project the key is the original Spanish text, so a missing translation still shows something readable instead of a raw identifier.")]
        public string key = "";
        public List<LocalizedString> translations = new List<LocalizedString>();
    }

    /// <summary>
    /// A localizable string table. One asset is enough for a whole project; call sites look up a
    /// key via LocalizationService.Tr(key) rather than referencing this catalog directly.
    /// </summary>
    [CreateAssetMenu(fileName = "LocalizationCatalog", menuName = "Escape Room Framework/Localization Catalog")]
    public sealed class LocalizationCatalog : ScriptableObject
    {
        [Tooltip("Used when the current language has no entry for a key.")]
        public string fallbackLanguageCode = "es";
        public List<LocalizationEntry> entries = new List<LocalizationEntry>();

        /// <summary>Every distinct language code that appears in at least one entry, fallback language first. Drives the Settings language dropdown without hardcoding a language list in UI code.</summary>
        public List<string> AvailableLanguages()
        {
            var languages = new List<string> { fallbackLanguageCode };
            foreach (LocalizationEntry entry in entries)
                foreach (LocalizedString translation in entry.translations)
                    if (!languages.Contains(translation.languageCode)) languages.Add(translation.languageCode);
            return languages;
        }

        /// <summary>Returns the translation for key/languageCode, falling back to fallbackLanguageCode, then to the key itself so a missing entry is visible instead of blank.</summary>
        public string Get(string key, string languageCode)
        {
            if (string.IsNullOrEmpty(key)) return key;

            LocalizationEntry entry = Find(key);
            if (entry == null) return key;

            LocalizedString match = entry.translations.Find(t => t.languageCode == languageCode);
            if (match == null && languageCode != fallbackLanguageCode)
                match = entry.translations.Find(t => t.languageCode == fallbackLanguageCode);
            return match != null && !string.IsNullOrEmpty(match.text) ? match.text : key;
        }

        /// <summary>True when the catalog has an entry for key. Used by SceneTextLocalizer to leave free text alone.</summary>
        public bool HasKey(string key) => !string.IsNullOrEmpty(key) && Find(key) != null;

        // Tr() runs every frame for HUD prompts, so lookups go through a dictionary instead of a linear search.
        [System.NonSerialized] private Dictionary<string, LocalizationEntry> _index;
        [System.NonSerialized] private int _indexedCount = -1;

        private LocalizationEntry Find(string key)
        {
            if (_index == null || _indexedCount != entries.Count)
            {
                _index = new Dictionary<string, LocalizationEntry>();
                foreach (LocalizationEntry e in entries)
                    if (e != null && !string.IsNullOrEmpty(e.key) && !_index.ContainsKey(e.key)) _index.Add(e.key, e);
                _indexedCount = entries.Count;
            }
            return _index.TryGetValue(key, out LocalizationEntry found) ? found : null;
        }

        private void OnValidate() => _index = null;
    }
}
