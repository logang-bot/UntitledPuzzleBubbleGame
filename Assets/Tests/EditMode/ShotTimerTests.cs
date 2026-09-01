using Game.Gameplay;
using NUnit.Framework;

namespace Game.Tests
{
    public class ShotTimerTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void Tick_PartialElapsed_DecrementsTimeRemaining()
        {
            var timer = new ShotTimer(duration: 8f);

            timer.Tick(deltaTime: 3f);

            Assert.That(timer.TimeRemaining, Is.EqualTo(5f).Within(Tolerance));
        }

        [Test]
        public void Tick_ExactlyExpired_ReturnsTrue()
        {
            var timer = new ShotTimer(duration: 8f);

            var expired = timer.Tick(deltaTime: 8f);

            Assert.That(expired, Is.True);
        }

        [Test]
        public void Tick_NotYetExpired_ReturnsFalse()
        {
            var timer = new ShotTimer(duration: 8f);

            var expired = timer.Tick(deltaTime: 3f);

            Assert.That(expired, Is.False);
        }

        [Test]
        public void Reset_AfterPartialTick_RestoresDuration()
        {
            var timer = new ShotTimer(duration: 8f);
            timer.Tick(deltaTime: 5f);

            timer.Reset();

            Assert.That(timer.TimeRemaining, Is.EqualTo(8f).Within(Tolerance));
        }
    }
}
