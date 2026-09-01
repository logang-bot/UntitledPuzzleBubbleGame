using Game.Shooter;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class TrajectoryPredictorWallBounceTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void Simulate_ThirtyDegreeAngleHitsWallBeforeCeiling_ReflectsOnceThenReachesCeiling()
        {
            var predictor = new TrajectoryPredictor(new BoardBounds(-1f, 1f, 3f));

            var points = predictor.Simulate(origin: Vector2.zero, angleDegrees: 30f, maxBounces: 10);

            Assert.That(points.Count, Is.EqualTo(3));
            Assert.That(points[1].x, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(points[1].y, Is.EqualTo(1.7320508f).Within(Tolerance));
            Assert.That(points[2].x, Is.EqualTo(0.2679492f).Within(Tolerance));
            Assert.That(points[2].y, Is.EqualTo(3f).Within(Tolerance));
        }

        [Test]
        public void Simulate_FortyFiveDegreeAngleZigzagsTwiceBeforeCeiling_ReflectsTwice()
        {
            var predictor = new TrajectoryPredictor(new BoardBounds(-1f, 1f, 4f));

            var points = predictor.Simulate(origin: Vector2.zero, angleDegrees: 45f, maxBounces: 10);

            Assert.That(points.Count, Is.EqualTo(4));
            AssertPoint(points[1], 1f, 1f);
            AssertPoint(points[2], -1f, 3f);
            AssertPoint(points[3], 0f, 4f);
        }

        private static void AssertPoint(Vector2 point, float x, float y)
        {
            Assert.That(point.x, Is.EqualTo(x).Within(Tolerance));
            Assert.That(point.y, Is.EqualTo(y).Within(Tolerance));
        }
    }
}
