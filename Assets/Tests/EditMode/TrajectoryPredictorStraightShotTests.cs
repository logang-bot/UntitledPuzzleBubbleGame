using Game.Shooter;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class TrajectoryPredictorStraightShotTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void Simulate_StraightUpAngleWithNoWallsInRange_ReturnsOriginAndCeilingHit()
        {
            var predictor = new TrajectoryPredictor(new BoardBounds(-10f, 10f, 5f));

            var points = predictor.Simulate(origin: Vector2.zero, angleDegrees: 0f, maxBounces: 10);

            Assert.That(points.Count, Is.EqualTo(2));
            Assert.That(points[1].x, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(points[1].y, Is.EqualTo(5f).Within(Tolerance));
        }
    }
}
