using Game.Motion;
using NUnit.Framework;

namespace Game.Tests
{
    public class MotionClipTests
    {
        [TestCase(EaseKind.Linear)]
        [TestCase(EaseKind.InQuad)]
        [TestCase(EaseKind.OutQuad)]
        [TestCase(EaseKind.InOutSine)]
        [TestCase(EaseKind.InCubic)]
        [TestCase(EaseKind.OutCubic)]
        [TestCase(EaseKind.OutBack)]
        public void Ease_StartsAtZeroAndEndsAtOne(EaseKind kind)
        {
            Assert.AreEqual(0f, Ease.Apply(kind, 0f), 1e-4f);
            Assert.AreEqual(1f, Ease.Apply(kind, 1f), 1e-4f);
        }

        [Test]
        public void Ease_OutBack_OvershootsBeforeSettling()
        {
            Assert.Greater(Ease.Apply(EaseKind.OutBack, 0.7f), 1f);
        }

        [Test]
        public void KeyTrack_HoldsItsEndsOutsideTheKeys()
        {
            var track = new KeyTrack(new MotionKey(0.2f, 5f), new MotionKey(1f, 9f));
            Assert.AreEqual(5f, track.Evaluate(0f));
            Assert.AreEqual(9f, track.Evaluate(3f));
        }

        [Test]
        public void KeyTrack_EasesWithTheDestinationKeysCurve()
        {
            var track = new KeyTrack(new MotionKey(0f, 0f), new MotionKey(1f, 10f, EaseKind.InQuad));
            Assert.AreEqual(2.5f, track.Evaluate(0.5f), 1e-4f);
        }

        [Test]
        public void SineTrack_StartsAtBasePlusPhase()
        {
            Assert.AreEqual(1f, new SineTrack(2f, 4f, (0f, 1f)).Evaluate(0f), 1e-4f);
            Assert.AreEqual(3f, new SineTrack(2f, 4f, (0.25f, 1f)).Evaluate(0f), 1e-4f);
        }

        [Test]
        public void PanelPop_WaitsForItsDelay_ThenSettlesAtRest()
        {
            Assert.AreEqual(0f, MenuMotions.PanelPop.Evaluate(0.1f).Alpha, 1e-4f);
            var settled = MenuMotions.PanelPop.Evaluate(1f);
            Assert.AreEqual(1f, settled.Scale.x, 1e-4f);
            Assert.IsFalse(MenuMotions.PanelPop.IsFinished(0.6f));
            Assert.IsTrue(MenuMotions.PanelPop.IsFinished(0.7f));
        }

        [Test]
        public void Clip_WithoutTracks_EvaluatesToRest()
        {
            var pose = new MotionClip().Evaluate(2f);
            Assert.AreEqual(1f, pose.Scale.y);
            Assert.AreEqual(1f, pose.Alpha);
            Assert.AreEqual(0f, pose.Offset.y);
        }

        [Test]
        public void BannerDrop_StartsAboveAndLands()
        {
            Assert.AreEqual(-140f, MenuMotions.BannerDrop.Evaluate(0f).Offset.y, 1e-4f);
            Assert.AreEqual(0f, MenuMotions.BannerDrop.Evaluate(0.55f).Offset.y, 1e-4f);
        }
    }
}
