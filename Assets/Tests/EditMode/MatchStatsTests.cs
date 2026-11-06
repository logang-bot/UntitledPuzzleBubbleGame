using Game.Battle;
using Game.Gameplay;
using NUnit.Framework;

namespace Game.Tests
{
    public class MatchStatsTests
    {
        [Test]
        public void RecordPoppedAndDropped_AccumulatesScoreAndPopped()
        {
            var stats = new MatchStats();
            stats.RecordPopped(5);
            stats.RecordDropped(3);
            Assert.AreEqual(ScoreCalculator.PointsForPop(5) + ScoreCalculator.PointsForDrop(3), stats.Score);
            Assert.AreEqual(8, stats.Popped);
        }

        [Test]
        public void BestChain_IsMostClearedBySingleShot()
        {
            var stats = new MatchStats();
            stats.RecordShot();
            stats.RecordPopped(3);
            stats.RecordDropped(4);
            stats.RecordShot();
            stats.RecordPopped(5);
            Assert.AreEqual(7, stats.BestChain);
            Assert.AreEqual(2, stats.Shots);
        }

        [Test]
        public void Reset_ClearsEverything()
        {
            var stats = new MatchStats();
            stats.RecordShot();
            stats.RecordPopped(4);
            stats.Reset();
            Assert.AreEqual(0, stats.Score + stats.Popped + stats.BestChain + stats.Shots);
        }
    }
}
