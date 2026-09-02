using Game.Grid;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class DifficultyCurveConfigTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void ForLevel_Level1_ReturnsStartingValues()
        {
            var curve = ScriptableObject.CreateInstance<DifficultyCurveConfig>();

            var config = curve.ForLevel(1);

            Assert.That(config.ColorCount, Is.EqualTo(3));
            Assert.That(config.Density, Is.EqualTo(0.55f).Within(Tolerance));
            Assert.That(config.HeadroomRows, Is.EqualTo(6));
            Assert.That(config.CeilingDropIntervalSeconds, Is.EqualTo(20f).Within(Tolerance));
        }

        [Test]
        public void ForLevel_HighLevel_ClampsToMaxColorCount()
        {
            var curve = ScriptableObject.CreateInstance<DifficultyCurveConfig>();

            var config = curve.ForLevel(1000);

            Assert.That(config.ColorCount, Is.EqualTo(6));
        }

        [Test]
        public void ForLevel_HighLevel_ClampsToMinHeadroomRows()
        {
            var curve = ScriptableObject.CreateInstance<DifficultyCurveConfig>();

            var config = curve.ForLevel(1000);

            Assert.That(config.HeadroomRows, Is.EqualTo(3));
        }

        [Test]
        public void ForLevel_HighLevel_ClampsToMinCeilingInterval()
        {
            var curve = ScriptableObject.CreateInstance<DifficultyCurveConfig>();

            var config = curve.ForLevel(1000);

            Assert.That(config.CeilingDropIntervalSeconds, Is.EqualTo(8f).Within(Tolerance));
        }
    }
}
