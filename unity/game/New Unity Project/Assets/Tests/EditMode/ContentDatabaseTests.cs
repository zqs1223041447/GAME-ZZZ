using System;
using System.IO;
using NUnit.Framework;
using UnityEngine.TestTools;
using Game.Runtime.Content;

namespace Game.Tests.EditMode
{
    public sealed class ContentDatabaseTests
    {
        string _temp;

        [SetUp]
        public void SetUp()
        {
            _temp = Path.Combine(Path.GetTempPath(), "game-zzz-s0-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_temp);
        }

        [TearDown]
        public void TearDown()
        {
            if (!string.IsNullOrEmpty(_temp) && Directory.Exists(_temp))
                Directory.Delete(_temp, true);
        }

        [Test]
        public void ValidDummy_Imports()
        {
            File.WriteAllText(
                Path.Combine(_temp, "dummy_ok.json"),
                "{ \"id\": \"monster.dummy\", \"displayName\": \"Dummy\" }");

            bool ok = ContentDatabase.TryLoadAll(_temp, out ContentDatabase db, out var errors);

            Assert.IsTrue(ok);
            Assert.AreEqual(0, errors.Count);
            Assert.AreEqual(1, db.Count);
            Assert.IsTrue(ContentId.TryParse("monster.dummy", out ContentId id));
            Assert.IsTrue(db.TryGet(id, out ContentRecord record));
            Assert.AreEqual("Dummy", record.DisplayName);
        }

        [Test]
        public void MissingId_FailsAndErrorContainsFileName()
        {
            File.WriteAllText(
                Path.Combine(_temp, "dummy_bad.json"),
                "{ \"displayName\": \"NoId\" }");

            bool ok = ContentDatabase.TryLoadAll(_temp, out ContentDatabase db, out var errors);

            Assert.IsFalse(ok);
            Assert.AreEqual(0, db.Count);
            Assert.IsTrue(ContainsFileName(errors, "dummy_bad.json"), string.Join("; ", errors));
        }

        [Test]
        public void MissingDirectory_Fails()
        {
            string missing = Path.Combine(_temp, "does-not-exist");
            bool ok = ContentDatabase.TryLoadAll(missing, out ContentDatabase db, out var errors);

            Assert.IsFalse(ok);
            Assert.AreEqual(0, db.Count);
            Assert.Greater(errors.Count, 0);
        }

        [Test]
        public void CorruptJson_FailsAndErrorContainsFileName()
        {
            LogAssert.ignoreFailingMessages = true;
            File.WriteAllText(Path.Combine(_temp, "broken.json"), "{ this is not json");

            bool ok = ContentDatabase.TryLoadAll(_temp, out ContentDatabase db, out var errors);

            Assert.IsFalse(ok);
            Assert.AreEqual(0, db.Count);
            Assert.IsTrue(ContainsFileName(errors, "broken.json"), string.Join("; ", errors));
        }

        [Test]
        public void DuplicateId_FailsEntireLoad()
        {
            const string body = "{ \"id\": \"monster.dummy\", \"displayName\": \"Dummy\" }";
            File.WriteAllText(Path.Combine(_temp, "a.json"), body);
            File.WriteAllText(Path.Combine(_temp, "b.json"), body);

            bool ok = ContentDatabase.TryLoadAll(_temp, out ContentDatabase db, out var errors);

            Assert.IsFalse(ok);
            Assert.AreEqual(0, db.Count);
            Assert.Greater(errors.Count, 0);
        }

        static bool ContainsFileName(System.Collections.Generic.List<string> errors, string fileName)
        {
            for (int i = 0; i < errors.Count; i++)
            {
                if (errors[i] != null && errors[i].IndexOf(fileName) >= 0)
                    return true;
            }

            return false;
        }
    }
}
