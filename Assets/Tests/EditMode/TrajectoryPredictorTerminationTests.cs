using Game.Shooter;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class TrajectoryPredictorTerminationTests
    {
        [Test]
        public void Simulate_PathReachesCeiling_LastPointYEqualsCeilingY()
        {
            var predictor = new TrajectoryPredictor(new BoardBounds(-10f, 10f, 5f));

            var points = predictor.Simulate(origin: Vector2.zero, angleDegrees: 0f, maxBounces: 10);

            Assert.That(points[^1].y, Is.EqualTo(5f).Within(0.0001f));
        }

        [Test]
        public void Simulate_ShallowAngleNarrowBoardExceedsMaxBounces_TruncatesAtCap()
        {
            var predictor = new TrajectoryPredictor(new BoardBounds(-0.1f, 0.1f, 100f));

            var points = predictor.Simulate(origin: Vector2.zero, angleDegrees: 80f, maxBounces: 5);

            Assert.That(points.Count, Is.EqualTo(7));
            Assert.That(points[^1].y, Is.LessThan(100f));
        }
    }
}
