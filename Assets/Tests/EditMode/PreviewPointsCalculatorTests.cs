using System.Collections.Generic;
using Game.Shooter;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class PreviewPointsCalculatorTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void TrimToSurface_NoTargetCenter_ReturnsPointsUnchanged()
        {
            var points = new List<Vector2> { Vector2.zero, new Vector2(0f, 2f) };

            var result = PreviewPointsCalculator.TrimToSurface(points, null, cellWidth: 1f);

            CollectionAssert.AreEqual(points, result);
        }

        [Test]
        public void TrimToSurface_HeadOnApproach_MovesEndpointToExactlyHalfCellWidthFromCenter()
        {
            var points = new List<Vector2> { Vector2.zero, new Vector2(0f, 2f) };

            var result = PreviewPointsCalculator.TrimToSurface(points, targetCenter: new Vector2(0f, 3f), cellWidth: 1f);

            Assert.That(result[^1].x, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(result[^1].y, Is.EqualTo(2.5f).Within(Tolerance));
            Assert.That(result[0], Is.EqualTo(points[0])); // earlier points untouched
        }

        [Test]
        public void TrimToSurface_AngledApproach_MovesTowardTheActualCenterNotTheIncomingRay()
        {
            // The endpoint (3, 0) is NOT on the line from (0,0) through the
            // target center - a grazing/angled hit, per the reported bug: a
            // fix that extrapolates along the incoming segment's direction
            // (rather than aiming at the real center) lands in the wrong
            // place for exactly this case.
            var points = new List<Vector2> { Vector2.zero, new Vector2(3f, 0f) };
            var targetCenter = new Vector2(3f, 4f); // 4 units straight above the endpoint

            var result = PreviewPointsCalculator.TrimToSurface(points, targetCenter, cellWidth: 1f);

            // Moved straight up (toward the true center) by 4 - 0.5 = 3.5, landing exactly cellWidth/2 from center.
            Assert.That(result[^1].x, Is.EqualTo(3f).Within(Tolerance));
            Assert.That(result[^1].y, Is.EqualTo(3.5f).Within(Tolerance));
            Assert.That(Vector2.Distance(result[^1], targetCenter), Is.EqualTo(0.5f).Within(Tolerance));
        }

        [Test]
        public void TrimToSurface_DiagonalTargetFromOrigin_MovesToExactlyHalfCellWidthFromCenter()
        {
            var points = new List<Vector2> { Vector2.zero }; // a single point is enough - no ray direction needed
            var targetCenter = new Vector2(3f, 4f); // 3-4-5 triangle, distance 5 from the endpoint

            var result = PreviewPointsCalculator.TrimToSurface(points, targetCenter, cellWidth: 1f);

            // Moved 5 - 0.5 = 4.5 along the (0.6, 0.8) unit direction from (0,0).
            Assert.That(result[^1].x, Is.EqualTo(2.7f).Within(Tolerance));
            Assert.That(result[^1].y, Is.EqualTo(3.6f).Within(Tolerance));
        }

        [Test]
        public void TrimToSurface_AlreadyWithinHalfCellWidthOfCenter_ReturnsPointsUnchanged()
        {
            var points = new List<Vector2> { new Vector2(0f, 0.3f) };
            var targetCenter = Vector2.zero; // only 0.3 away, already inside the surface radius (0.5)

            var result = PreviewPointsCalculator.TrimToSurface(points, targetCenter, cellWidth: 1f);

            CollectionAssert.AreEqual(points, result);
        }

        [Test]
        public void TrimToSurface_EmptyPoints_ReturnsUnchanged()
        {
            var points = new List<Vector2>();

            var result = PreviewPointsCalculator.TrimToSurface(points, new Vector2(1f, 1f), cellWidth: 1f);

            CollectionAssert.AreEqual(points, result);
        }
    }
}
