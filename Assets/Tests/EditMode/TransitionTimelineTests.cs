using Game.Flow;
using NUnit.Framework;

namespace Game.Tests
{
    public class TransitionTimelineTests
    {
        [Test]
        public void ClosingFrame_AtStart_IsFullyOpenFrame()
        {
            Assert.AreEqual(0, TransitionTimeline.ClosingFrame(0f));
        }

        [Test]
        public void ClosingFrame_AtEndOfHalf_IsFullyDarkFrame()
        {
            Assert.AreEqual(9, TransitionTimeline.ClosingFrame(0.49f));
        }

        [Test]
        public void OpeningFrame_AtStart_IsFullyDarkFrame()
        {
            Assert.AreEqual(9, TransitionTimeline.OpeningFrame(0f));
        }

        [Test]
        public void OpeningFrame_PastHalf_ClampsToOpenFrame()
        {
            Assert.AreEqual(0, TransitionTimeline.OpeningFrame(0.8f));
        }

        [Test]
        public void IsHalfDone_AfterHalfASecond_IsTrue()
        {
            Assert.IsTrue(TransitionTimeline.IsHalfDone(0.5f));
            Assert.IsFalse(TransitionTimeline.IsHalfDone(0.49f));
        }
    }
}
