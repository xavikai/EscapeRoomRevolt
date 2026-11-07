using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Core.Flow;
using EscapeRoomRevolt.Core.Settings;
using EscapeRoomRevolt.Systems.Inventory;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace EscapeRoomRevolt.Systems.Tests
{
    public sealed class ProductCompositionTests
    {
        private Scene _loadedScene;
        private HashSet<int> _existing;

        [UnityTest]
        public IEnumerator ProductGameplay_StartsWithItsOwnServicesAndHUD()
        {
            ProductConfiguration product = ProductConfiguration.Current;
            if (product == null) Assert.Ignore("Consumer composition test; legacy authoring project has no product configuration.");
            Assert.That(Resources.Load<TMPro.TMP_Settings>("TMP Settings"), Is.Not.Null, "Implicit TMP Resources defaults must be shipped.");
            Assert.That(TMPro.TMP_Settings.defaultStyleSheet, Is.Not.Null);
            Assert.That(TMPro.TMP_Settings.defaultFontAsset, Is.Not.Null);
            _existing = new HashSet<int>(Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include)
                .Select(component => component.gameObject.GetInstanceID()));
            var flow = Resources.Load<GameFlowSettings>("GameFlowSettings");
            string target = product.DesktopScenes.First(scene => System.IO.Path.GetFileNameWithoutExtension(scene) == flow.FirstGameplayScene);
            yield return SceneManager.LoadSceneAsync(target, LoadSceneMode.Additive);
            _loadedScene = SceneManager.GetSceneByPath(target);
            for (int frame = 0; frame < 5; frame++) yield return null;
            Assert.That(GameContext.IsInitialized, Is.True);
            Assert.That(InventoryManager.Instance, Is.Not.Null);
            Assert.That(EscapeRoomRevolt.Player.PlayerPlatformRegistry.Current, Is.Not.Null);
            bool survival = GameFeatures.Genre == GameGenre.SurvivalHorror;
            if (survival)
            {
                Assert.That(UnityEngine.AI.NavMesh.CalculateTriangulation().vertices.Length, Is.GreaterThan(0), "Survival navigation must survive scene/build reloads.");
                foreach (var agent in Object.FindObjectsByType<UnityEngine.AI.NavMeshAgent>().Where(agent => agent.isActiveAndEnabled))
                    Assert.That(agent.isOnNavMesh, Is.True, "Enemy spawned outside persistent navigation: " + agent.name);
            }
            Assert.That(GameModules.Difficulty != null, Is.EqualTo(survival));
            var hud = Object.FindObjectsByType<UIDocument>(FindObjectsInactive.Include)
                .Select(document => document.rootVisualElement).First(root => root?.Q("gameplay-root") != null);
            Assert.That(hud.Q("sanity-hud") != null, Is.EqualTo(survival));
            Assert.That(hud.Q("vitals-hud") != null, Is.EqualTo(survival));
            Assert.That(hud.Q("flashlight-hud"), Is.Not.Null, "Reusable flashlight presentation belongs to Common.");
            Assert.That(ProductConfiguration.PersistentDataDirectory.EndsWith(product.ProductId), Is.True);
            foreach (var label in _loadedScene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<TMPro.TextMeshPro>()).Where(label => label.isActiveAndEnabled && !string.IsNullOrWhiteSpace(label.text)))
            {
                // Force the rendering path that dereferences TMP's implicit default style sheet.
                // Some authored subtitle labels intentionally produce no visible glyphs.
                label.ForceMeshUpdate(true, true);
            }
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_loadedScene.IsValid() && _loadedScene.isLoaded) yield return SceneManager.UnloadSceneAsync(_loadedScene);
            if (_existing != null)
            {
                var created = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include)
                    .Where(component => component != null && component.GetType().Namespace?.StartsWith("EscapeRoomRevolt.") == true
                        && !_existing.Contains(component.gameObject.GetInstanceID()))
                    .Select(component => component.gameObject).Distinct().ToArray();
                foreach (GameObject owner in created) if (owner != null) Object.Destroy(owner);
                yield return null;
            }
            _existing = null;
        }
    }
}
