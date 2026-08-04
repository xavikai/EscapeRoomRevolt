using EscapeRoomRevolt.Core.Localization;
using NUnit.Framework;
using UnityEngine;

namespace EscapeRoomRevolt.Core.Tests
{
    public class LocalizationCatalogTests
    {
        private LocalizationCatalog _catalog;

        [SetUp]
        public void SetUp()
        {
            _catalog = ScriptableObject.CreateInstance<LocalizationCatalog>();
            _catalog.fallbackLanguageCode = "es";

            var entry = new LocalizationEntry { key = "Continuar" };
            entry.translations.Add(new LocalizedString { languageCode = "es", text = "Continuar" });
            entry.translations.Add(new LocalizedString { languageCode = "en", text = "Continue" });
            _catalog.entries.Add(entry);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_catalog);

        [Test]
        public void Get_ReturnsExactLanguageTranslation()
        {
            Assert.AreEqual("Continue", _catalog.Get("Continuar", "en"));
        }

        [Test]
        public void Get_FallsBackToFallbackLanguage_WhenRequestedLanguageMissing()
        {
            Assert.AreEqual("Continuar", _catalog.Get("Continuar", "fr"));
        }

        [Test]
        public void Get_ReturnsKeyItself_WhenKeyNotInCatalog()
        {
            Assert.AreEqual("Unknown Key", _catalog.Get("Unknown Key", "en"));
        }

        [Test]
        public void Get_ReturnsKeyItself_ForNullOrEmptyKey()
        {
            Assert.AreEqual("", _catalog.Get("", "en"));
            Assert.IsNull(_catalog.Get(null, "en"));
        }

        [Test]
        public void AvailableLanguages_IncludesFallbackFirstThenEveryDistinctCodeOnce()
        {
            var languages = _catalog.AvailableLanguages();

            Assert.AreEqual("es", languages[0]);
            CollectionAssert.AreEquivalent(new[] { "es", "en" }, languages);
        }
    }
}
