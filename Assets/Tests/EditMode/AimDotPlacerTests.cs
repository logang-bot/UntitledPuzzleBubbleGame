using System.Collections.Generic;
using Game.Shooter;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class AimDotPlacerTests
    {
        private const float Tolerance = 0.0001f;
        private static readonly List<Vector2> StraightUp = new() { Vector2.zero, new Vector2(0f, 10f) };

        [Test]
        public void Place_StraightPath_SpacesDotsFromTheStartGap()
        {
            var dots = new List<(Vector2 Position, float Alpha)>();
            AimDotPlacer.Place(StraightUp, new AimDotSpacing(start: 1f, gap: 2f, scroll: 0f), dots);

            Assert.AreEqual(5, dots.Count);
            Assert.That(dots[1].Position.y, Is.EqualTo(3f).Within(Tolerance));
        }

        [Test]
        public void Place_Scrolled_ShiftsDotsForwardWithinOneGap()
        {
            var dots = new List<(Vector2 Position, float Alpha)>();
            AimDotPlacer.Place(StraightUp, new AimDotSpacing(start: 1f, gap: 2f, scroll: 5f), dots);

            Assert.That(dots[0].Position.y, Is.EqualTo(2f).Within(Tolerance));
        }

        [Test]
        public void Place_DotsFadeTowardTheEnd()
        {
            var dots = new List<(Vector2 Position, float Alpha)>();
            AimDotPlacer.Place(StraightUp, new AimDotSpacing(start: 0f, gap: 10f, scroll: 0f), dots);

            Assert.That(dots[0].Alpha, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(dots[1].Alpha, Is.EqualTo(AimDotPlacer.EndAlpha).Within(Tolerance));
        }

        [Test]
        public void PointAt_PastABend_FollowsTheSecondSegment()
        {
            var bent = new List<Vector2> { Vector2.zero, new Vector2(3f, 0f), new Vector2(3f, 4f) };

            Assert.That(AimDotPlacer.PointAt(bent, 5f), Is.EqualTo(new Vector2(3f, 2f)));
        }
    }
}
