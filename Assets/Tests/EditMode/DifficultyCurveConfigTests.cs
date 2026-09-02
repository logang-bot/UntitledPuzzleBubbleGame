using Game.Grid;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class DifficultyCurveConfigTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void ForLevel_Level1_ReturnsLevel1OverrideValues()
        {
            var curve = ScriptableObject.CreateInstance<DifficultyCurveConfig>();

            var config = curve.ForLevel(1);

            Assert.That(config.ColorCount, Is.EqualTo(3));
            Assert.That(config.Density, Is.EqualTo(0.35f).Within(Tolerance));
            Assert.That(config.HeadroomRows, Is.EqualTo(9));
            Assert.That(config.CeilingDropIntervalSeconds, Is.EqualTo(20f).Within(Tolerance));
        }

        [Test]
        public void ForLevel_Level2_UsesRampNotLevel1Override()
        {
            var curve = ScriptableObject.CreateInstance<DifficultyCurveConfig>();

            var config = curve.ForLevel(2);

            // startDensity(0.55) + densityIncreasePerLevel(0.02) * (level 2 - 1) - i.e. the
            // normal ramp step, proving level 1's override doesn't leak into level 2.
            Assert.That(config.Density, Is.EqualTo(0.57f).Within(Tolerance));
            Assert.That(config.HeadroomRows, Is.EqualTo(6));
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
