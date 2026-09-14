using Game.Grid;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class DifficultyCurveConfigTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void ForLevel_Level1_ReturnsFirstKeyframeValues()
        {
            var curve = ScriptableObject.CreateInstance<DifficultyCurveConfig>();

            var config = curve.ForLevel(1);

            Assert.That(config.ColorCount, Is.EqualTo(3));
            Assert.That(config.Density, Is.EqualTo(0.35f).Within(Tolerance));
            Assert.That(config.HeadroomRows, Is.EqualTo(9));
            Assert.That(config.CeilingDropIntervalSeconds, Is.EqualTo(20f).Within(Tolerance));
        }

        [Test]
        public void ForLevel_Level2_MatchesDensityCurveKeyframe()
        {
            var curve = ScriptableObject.CreateInstance<DifficultyCurveConfig>();

            var config = curve.ForLevel(2);

            Assert.That(config.Density, Is.EqualTo(0.55f).Within(Tolerance));
        }

        [Test]
        public void ForLevel_Level6_MatchesHigherDensityKeyframeForNonPatternFallback()
        {
            var curve = ScriptableObject.CreateInstance<DifficultyCurveConfig>();

            var config = curve.ForLevel(6);

            Assert.That(config.Density, Is.EqualTo(0.8f).Within(Tolerance));
        }

        [Test]
        public void ForLevel_LastColorCountKeyframe_ReachesMaxColorCount()
        {
            var curve = ScriptableObject.CreateInstance<DifficultyCurveConfig>();

            var config = curve.ForLevel(30);

            Assert.That(config.ColorCount, Is.EqualTo(6));
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
