using Game.Grid;
using NUnit.Framework;

namespace Game.Tests
{
    public class BubbleSpawnMotionTests
    {
        [Test]
        public void ScaleForProgress_AtStart_IsZero()
        {
            Assert.That(BubbleSpawnMotion.ScaleForProgress(0f), Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void ScaleForProgress_AtEnd_IsOne()
        {
            Assert.That(BubbleSpawnMotion.ScaleForProgress(1f), Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void ScaleForProgress_IsMonotonicallyIncreasing()
        {
            var previous = BubbleSpawnMotion.ScaleForProgress(0f);
            for (var i = 1; i <= 10; i++)
            {
                var current = BubbleSpawnMotion.ScaleForProgress(i / 10f);
                Assert.That(current, Is.GreaterThanOrEqualTo(previous));
                previous = current;
            }
        }
    }
}
