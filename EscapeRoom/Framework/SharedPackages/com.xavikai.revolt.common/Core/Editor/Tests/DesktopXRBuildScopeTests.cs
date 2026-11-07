using System;
using EscapeRoomRevolt.EditorTools;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.XR.Management;

namespace EscapeRoomRevolt.Editor.Tests
{
    public sealed class DesktopXRBuildScopeTests
    {
        [Test]
        public void DesktopScope_RemovesLoadersAndRestoresAuthoringStateWhenBuildThrows()
        {
            var settings = ScriptableObject.CreateInstance<XRGeneralSettings>();
            var manager = ScriptableObject.CreateInstance<XRManagerSettings>();
            var loader = ScriptableObject.CreateInstance<TestXRLoader>();
            try
            {
                settings.Manager = manager;
                settings.InitManagerOnStart = true;
                manager.automaticLoading = true;
                manager.automaticRunning = true;
                Assert.That(manager.TrySetLoaders(new System.Collections.Generic.List<XRLoader> { loader }), Is.True);
                Assert.Throws<InvalidOperationException>(() =>
                {
                    using (new DesktopXRBuildScope(settings))
                    {
                        Assert.That(manager.activeLoaders, Is.Empty);
                        Assert.That(settings.InitManagerOnStart, Is.False);
                        Assert.That(manager.automaticLoading, Is.False);
                        Assert.That(manager.automaticRunning, Is.False);
                        throw new InvalidOperationException("Simulated build failure");
                    }
                });
                Assert.That(settings.InitManagerOnStart, Is.True);
                Assert.That(manager.activeLoaders, Is.EqualTo(new[] { loader }));
                Assert.That(manager.automaticLoading, Is.True);
                Assert.That(manager.automaticRunning, Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(settings);
                UnityEngine.Object.DestroyImmediate(manager);
                UnityEngine.Object.DestroyImmediate(loader);
            }
        }

        public sealed class TestXRLoader : XRLoader
        {
            public override T GetLoadedSubsystem<T>() => null;
        }
    }
}
