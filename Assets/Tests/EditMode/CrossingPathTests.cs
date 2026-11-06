using Game.Menu;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class CrossingPathTests
    {
        private static readonly Vector2 HalfSize = new Vector2(10f, 8f);

        [Test]
        public void Pick_FromLeft_StartsOffLeftEdgeAndEndsOffRightEdge()
        {
            var path = CrossingPath.Pick(new System.Random(1), HalfSize, CrossingPath.FromLeft);
            Assert.AreEqual(-10f, path.Start.x);
            Assert.AreEqual(280f, path.End.x);
            Assert.IsTrue(path.MovesRight);
        }

        [Test]
        public void Pick_FromBottom_RisesOffTopEdge()
        {
            var path = CrossingPath.Pick(new System.Random(1), HalfSize, CrossingPath.FromBottom);
            Assert.AreEqual(593f, path.Start.y);
            Assert.AreEqual(-8f, path.End.y);
        }

        [Test]
        public void At_Halfway_IsMidpoint()
        {
            var path = new CrossingPath { Start = new Vector2(0f, 0f), End = new Vector2(100f, 50f) };
            Assert.AreEqual(new Vector2(50f, 25f), path.At(0.5f));
        }

        [Test]
        public void SquashedIntoBigBand_KeepsStartInsideBand()
        {
            for (var seed = 0; seed < 20; seed++)
            {
                var path = CrossingPath.Pick(new System.Random(seed), HalfSize, CrossingPath.FromRight).SquashedIntoBigBand();
                Assert.That(path.Start.y, Is.InRange(250f, 320f));
                Assert.That(path.End.y - path.Start.y, Is.InRange(-20f, 20f));
            }
        }
    }
}
