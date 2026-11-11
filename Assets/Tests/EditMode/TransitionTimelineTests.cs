using Game.Flow;
using NUnit.Framework;

namespace Game.Tests
{
    public class TransitionTimelineTests
    {
        [Test]
        public void ClosingScale_AtStart_IsFullyOpen()
        {
            Assert.AreEqual(TransitionTimeline.OpenScale, TransitionTimeline.ClosingScale(0f), 1e-4f);
        }

        [Test]
        public void ClosingScale_AtEnd_IsShut()
        {
            Assert.AreEqual(0f, TransitionTimeline.ClosingScale(TransitionTimeline.CloseSeconds), 1e-4f);
        }

        [Test]
        public void ClosingScale_EasesIn_SoTheHoleShrinksSlowlyAtFirst()
        {
            var halfway = TransitionTimeline.ClosingScale(TransitionTimeline.CloseSeconds / 2f);
            Assert.Greater(halfway, TransitionTimeline.OpenScale / 2f);
        }

        [Test]
        public void OpeningScale_RunsFromShutToFullyOpen()
        {
            Assert.AreEqual(0f, TransitionTimeline.OpeningScale(0f), 1e-4f);
            Assert.AreEqual(TransitionTimeline.OpenScale, TransitionTimeline.OpeningScale(TransitionTimeline.OpenSeconds), 1e-4f);
        }

        [Test]
        public void Scales_PastTheirPhase_StayClamped()
        {
            Assert.AreEqual(0f, TransitionTimeline.ClosingScale(5f), 1e-4f);
            Assert.AreEqual(TransitionTimeline.OpenScale, TransitionTimeline.OpeningScale(5f), 1e-4f);
        }
    }
}
