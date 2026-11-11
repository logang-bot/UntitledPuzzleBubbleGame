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
            var path = CrossingPath.Pick(new System.Random(1), HalfSize, new CrossingRoute(CrossingPath.FromLeft, CrossingRoute.FullHeight));
            Assert.AreEqual(-10f, path.Start.x);
            Assert.AreEqual(370f, path.End.x);
            Assert.IsTrue(path.MovesRight);
        }

        [Test]
        public void Pick_FromBottom_RisesOffTopEdge()
        {
            var path = CrossingPath.Pick(new System.Random(1), HalfSize, new CrossingRoute(CrossingPath.FromBottom, CrossingRoute.FullHeight));
            Assert.AreEqual(788f, path.Start.y);
            Assert.AreEqual(-8f, path.End.y);
        }

        [Test]
        public void Pick_Horizontal_StaysInsideBand()
        {
            for (var seed = 0; seed < 20; seed++)
            {
                var path = CrossingPath.Pick(new System.Random(seed), HalfSize, new CrossingRoute(CrossingPath.FromRight, new Vector2(250f, 500f)));
                Assert.That(path.Start.y, Is.InRange(250f, 500f));
                Assert.That(path.End.y, Is.InRange(250f, 500f));
            }
        }

        [Test]
        public void At_Halfway_IsMidpoint()
        {
            var path = new CrossingPath { Start = new Vector2(0f, 0f), End = new Vector2(100f, 50f) };
            Assert.AreEqual(new Vector2(50f, 25f), path.At(0.5f));
        }

        [Test]
        public void Leveled_KeepsStartHeight()
        {
            var path = new CrossingPath { Start = new Vector2(-10f, 300f), End = new Vector2(370f, 420f) }.Leveled();
            Assert.AreEqual(300f, path.End.y);
            Assert.AreEqual(380f, path.Length);
        }
    }
}
