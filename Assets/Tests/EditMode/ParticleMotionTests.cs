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
        public void Wave_AtQuarterPhase_PeaksAtOne()
        {
            Assert.AreEqual(1f, ParticleMotion.Wave(time: 0f, period: 2f, phase: 0.25f), 1e-4f);
        }

        [Test]
        public void BurstFrame_DuringBurst_ReturnsFrame()
        {
            Assert.AreEqual(3, ParticleMotion.BurstFrame(age: 0.2f, frames: (10, 15f)));
        }

        [Test]
        public void BurstFrame_AfterOrBeforeBurst_IsHidden()
        {
            Assert.AreEqual(-1, ParticleMotion.BurstFrame(age: 1f, frames: (10, 15f)));
            Assert.AreEqual(-1, ParticleMotion.BurstFrame(age: -0.1f, frames: (10, 15f)));
        }
    }
}
