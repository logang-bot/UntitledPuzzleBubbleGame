using Game.Gameplay;
using NUnit.Framework;

namespace Game.Tests
{
    public class ScoreCalculatorTests
    {
        [TestCase(3, 60)]
        [TestCase(4, 120)]
        [TestCase(5, 200)]
        [TestCase(10, 900)]
        public void PointsForPop_ScalesQuadraticallyWithClusterSize(int bubbleCount, int expected)
        {
            Assert.That(ScoreCalculator.PointsForPop(bubbleCount), Is.EqualTo(expected));
        }

        [TestCase(1, 20)]
        [TestCase(5, 100)]
        [TestCase(12, 240)]
        public void PointsForDrop_ScalesLinearlyWithBubbleCount(int bubbleCount, int expected)
        {
            Assert.That(ScoreCalculator.PointsForDrop(bubbleCount), Is.EqualTo(expected));
        }
    }
}
