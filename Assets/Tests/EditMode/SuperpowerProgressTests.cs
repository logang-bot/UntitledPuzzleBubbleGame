using Game.Superpowers;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class SuperpowerProgressTests
    {
        private const string Key = "HighestLevelReached";

        private int? _savedValue;

        [SetUp]
        public void SetUp()
        {
            _savedValue = PlayerPrefs.HasKey(Key) ? PlayerPrefs.GetInt(Key) : (int?)null;
            PlayerPrefs.DeleteKey(Key);
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(Key);
            if (_savedValue.HasValue) PlayerPrefs.SetInt(Key, _savedValue.Value);
        }

        [Test]
        public void HighestLevelReached_NoValueSaved_DefaultsToOne()
        {
            Assert.AreEqual(1, SuperpowerProgress.HighestLevelReached);
        }

        [Test]
        public void HighestLevelReached_AfterSettingValue_PersistsAndReturnsIt()
        {
            SuperpowerProgress.HighestLevelReached = 7;

            Assert.AreEqual(7, SuperpowerProgress.HighestLevelReached);
        }
    }
}
