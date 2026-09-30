using System.Collections.Generic;
using EscapeRoomRevolt.Core.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeRoomRevolt.UI
{
    /// <summary>
    /// Translates the static 3D texts of a scene (signs, labels, notes written directly on a
    /// TextMeshPro or TextMesh component) without adding anything to the scene.
    ///
    /// On every scene load it looks at each text: if its content is a key of the localization
    /// catalog, it is shown in the current language and updated when the language changes.
    /// Texts that code writes at runtime are left alone: a text is only re-translated while it
    /// still shows the value this class wrote last. Free text that is not in the catalog is
    /// never touched, so writing a sign in any language is still fine — it just won't translate.
    /// </summary>
    public static class SceneTextLocalizer
    {
        private sealed class Entry
        {
            public Object target;
            public string key;
            public string applied;
        }

        private static readonly List<Entry> _entries = new List<Entry>();
        private static LocalizationService _subscribed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Scan();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Scan();

        /// <summary>Registers every catalog-key text currently loaded. Safe to call again (e.g. after instantiating a prefab with signs).</summary>
        public static void Scan()
        {
            LocalizationService service = LocalizationService.Instance;
            if (service == null) return;
            Subscribe(service);

            _entries.RemoveAll(e => e.target == null);
            var known = new HashSet<Object>();
            foreach (Entry e in _entries) known.Add(e.target);

            foreach (TMP_Text text in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Register(service, text, text.text, known);
            foreach (TextMesh mesh in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Register(service, mesh, mesh.text, known);
        }

        private static void Register(LocalizationService service, Object target, string current, HashSet<Object> known)
        {
            if (known.Contains(target) || !service.HasKey(current)) return;
            var entry = new Entry { target = target, key = current, applied = current };
            _entries.Add(entry);
            Apply(entry);
        }

        private static void Subscribe(LocalizationService service)
        {
            if (_subscribed == service) return;
            if (_subscribed != null) _subscribed.LanguageChanged -= RefreshAll;
            _subscribed = service;
            service.LanguageChanged += RefreshAll;
        }

        private static void RefreshAll()
        {
            _entries.RemoveAll(e => e.target == null);
            foreach (Entry entry in _entries) Apply(entry);
        }

        private static void Apply(Entry entry)
        {
            string value = LocalizationService.Tr(entry.key);
            switch (entry.target)
            {
                case TMP_Text text:
                    if (text.text != entry.applied) return; // code changed it since: not ours any more
                    text.text = value;
                    break;
                case TextMesh mesh:
                    if (mesh.text != entry.applied) return;
                    mesh.text = value;
                    break;
            }
            entry.applied = value;
        }
    }
}
