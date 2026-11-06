using Game.Results;
using NUnit.Framework;

namespace Game.Tests
{
    public class ParticleMotionTests
    {
        [Test]
        public void Wrap_PastEnd_ComesBackFromStart()
        {
            Assert.AreEqual(5f, ParticleMotion.Wrap(value: 205f, min: 0f, span: 200f), 1e-4f);
        }

        [Test]
        public void Wrap_BelowMin_WrapsFromEnd()
        {
            Assert.AreEqual(600f, ParticleMotion.Wrap(value: -25f, min: -20f, span: 625f), 1e-4f);
        }

        [Test]
        public void BurstFrame_DuringBurst_ReturnsFrame()
        {
            Assert.AreEqual(2, ParticleMotion.BurstFrame(time: 0.2f, period: 1.6f, frames: (6, 12f)));
        }

        [Test]
        public void BurstFrame_AfterBurst_IsHidden()
        {
            Assert.AreEqual(-1, ParticleMotion.BurstFrame(time: 1f, period: 1.6f, frames: (6, 12f)));
        }
    }
}
