using Game.Art;
using NUnit.Framework;

namespace Game.Tests
{
    public class FrameClockTests
    {
        [Test]
        public void LoopingFrame_PastLastFrame_WrapsToStart()
        {
            Assert.AreEqual(1, FrameClock.LoopingFrame(elapsed: 0.85f, fps: 6f, frameCount: 4));
        }

        [Test]
        public void LoopingFrame_StaticSprite_StaysOnFirstFrame()
        {
            Assert.AreEqual(0, FrameClock.LoopingFrame(elapsed: 5f, fps: 0f, frameCount: 3));
        }

        [Test]
        public void OnceFrame_PastLastFrame_HoldsLastFrame()
        {
            Assert.AreEqual(3, FrameClock.OnceFrame(elapsed: 2f, fps: 10f, frameCount: 4));
        }

        [Test]
        public void IsFinished_BeforeAllFramesPlayed_IsFalse()
        {
            Assert.IsFalse(FrameClock.IsFinished(elapsed: 0.35f, fps: 10f, frameCount: 4));
        }

        [Test]
        public void IsFinished_AfterAllFramesPlayed_IsTrue()
        {
            Assert.IsTrue(FrameClock.IsFinished(elapsed: 0.4f, fps: 10f, frameCount: 4));
        }
    }
}
