using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using EscapeRoomRevolt.Core.Save;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EscapeRoomRevolt.Core.Tests
{
    public class SaveRecoveryTests
    {
        private string _path;
        private static readonly MethodInfo Read = typeof(SaveManager).GetMethod("TryReadSlot", BindingFlags.NonPublic | BindingFlags.Static);

        [SetUp]
        public void SetUp() => _path = Path.Combine(Path.GetTempPath(), "escape-room-audit-" + Guid.NewGuid() + ".json");

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_path)) File.Delete(_path);
            if (File.Exists(_path + ".bak")) File.Delete(_path + ".bak");
        }

        private SaveGameData ReadSlot() => (SaveGameData)Read.Invoke(null, new object[] { _path });
        private static SaveGameData ValidData()
        {
            var data = new SaveGameData { slotId = "audit", runSeed = 123 };
            data.keys.Add("door");
            data.values.Add("{\"open\":true}");
            return data;
        }

        [Test]
        public void ValidSlot_RoundTripsStateAndRunSeed()
        {
            File.WriteAllText(_path, JsonUtility.ToJson(ValidData()));
            var result = ReadSlot();
            Assert.That(result.runSeed, Is.EqualTo(123));
            Assert.That(result.values[0], Is.EqualTo("{\"open\":true}"));
        }

        [Test]
        public void MissingPrimary_RecoversBackup()
        {
            File.WriteAllText(_path + ".bak", JsonUtility.ToJson(ValidData()));
            LogAssert.Expect(LogType.Warning, new Regex("recovered from its backup"));
            Assert.That(ReadSlot().slotId, Is.EqualTo("audit"));
        }

        [Test]
        public void MismatchedStateLists_RecoversBackup()
        {
            File.WriteAllText(_path + ".bak", JsonUtility.ToJson(ValidData()));
            var broken = ValidData();
            broken.values.Clear();
            File.WriteAllText(_path, JsonUtility.ToJson(broken));
            LogAssert.Expect(LogType.Warning, new Regex("recovered from its backup"));
            Assert.That(ReadSlot().values.Count, Is.EqualTo(1));
        }

        [TestCase(0)]
        [TestCase(4)]
        public void UnsupportedVersion_IsRejected(int version)
        {
            var data = ValidData();
            data.version = version;
            File.WriteAllText(_path, JsonUtility.ToJson(data));
            Assert.That(ReadSlot(), Is.Null);
        }

        [Test]
        public void DuplicateIds_AreRejected()
        {
            var data = ValidData();
            data.keys.Add("door");
            data.values.Add("{}");
            File.WriteAllText(_path, JsonUtility.ToJson(data));
            Assert.That(ReadSlot(), Is.Null);
        }

        [Test]
        public void EmptyId_IsRejected()
        {
            var data = ValidData();
            data.keys[0] = " ";
            File.WriteAllText(_path, JsonUtility.ToJson(data));
            Assert.That(ReadSlot(), Is.Null);
        }

        [Test]
        public void MissingSlotAndBackup_ReturnsNull() => Assert.That(ReadSlot(), Is.Null);
    }
}
