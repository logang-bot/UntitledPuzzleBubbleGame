using Game.Grid;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class PlayfieldFrameTests
    {
        private const float Tolerance = 0.0001f;
        private static readonly float WorldPerPt = PlayfieldFrame.WorldPerPtFor(1f);

        [Test]
        public void DangerY_Solo_SitsTheDeckHeightAboveTheViewBottom()
        {
            var frame = new PlayfieldFrame(PlayfieldSpec.Solo, WorldPerPt, raisePt: 0f);

            Assert.That(frame.DangerY(Vector2.zero, orthographicSize: 10f), Is.EqualTo(-10f + 169f * WorldPerPt).Within(Tolerance));
        }

        [Test]
        public void Pointer_Solo_IsCenteredAboveTheViewBottom()
        {
            var frame = new PlayfieldFrame(PlayfieldSpec.Solo, WorldPerPt, raisePt: 0f);
            var pointer = frame.Pointer(new Vector2(3f, 0f), orthographicSize: 10f);

            Assert.That(pointer.x, Is.EqualTo(3f).Within(Tolerance));
            Assert.That(pointer.y, Is.EqualTo(-10f + 63.33f * WorldPerPt).Within(Tolerance));
        }

        [Test]
        public void BoardTopY_Solo_LeavesTheCeilingBandAtTheTop()
        {
            var frame = new PlayfieldFrame(PlayfieldSpec.Solo, WorldPerPt, raisePt: 0f);

            Assert.That(frame.BoardTopY(Vector2.zero, orthographicSize: 10f), Is.EqualTo(10f - 40f * WorldPerPt).Within(Tolerance));
        }

        [Test]
        public void DangerY_SoloRaised_LiftsTheDeck()
        {
            var flat = new PlayfieldFrame(PlayfieldSpec.Solo, WorldPerPt, raisePt: 0f);
            var raised = new PlayfieldFrame(PlayfieldSpec.Solo, WorldPerPt, raisePt: 12f);

            Assert.That(raised.DangerY(Vector2.zero, 10f) - flat.DangerY(Vector2.zero, 10f), Is.EqualTo(12f * WorldPerPt).Within(Tolerance));
        }

        [Test]
        public void DangerY_BattleHalf_IgnoresTheRaise()
        {
            var flat = new PlayfieldFrame(PlayfieldSpec.BattleHalf, WorldPerPt, raisePt: 0f);
            var raised = new PlayfieldFrame(PlayfieldSpec.BattleHalf, WorldPerPt, raisePt: 12f);

            Assert.That(raised.DangerY(Vector2.zero, 10f), Is.EqualTo(flat.DangerY(Vector2.zero, 10f)).Within(Tolerance));
        }

        [Test]
        public void RaisePt_GestureBarTallerThanTheAllowance_ReturnsTheExcess()
        {
            Assert.That(PlayfieldFrame.RaisePt(safeBottomPx: 102f, screenWidthPx: 1080f), Is.EqualTo(12f).Within(Tolerance));
        }

        [Test]
        public void RaisePt_NoGestureBar_ReturnsZero()
        {
            Assert.That(PlayfieldFrame.RaisePt(safeBottomPx: 0f, screenWidthPx: 1080f), Is.EqualTo(0f));
        }
    }
}
