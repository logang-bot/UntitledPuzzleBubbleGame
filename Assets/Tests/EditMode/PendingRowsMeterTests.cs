using Game.Battle;
using NUnit.Framework;

namespace Game.Tests
{
    public class PendingRowsMeterTests
    {
        [Test]
        public void ConsumeWholeRows_BelowOne_ReturnsZero()
        {
            var meter = new PendingRowsMeter();
            meter.Add(0.6f);

            var rows = meter.ConsumeWholeRows();

            Assert.AreEqual(0, rows);
        }

        [Test]
        public void ConsumeWholeRows_AtLeastOne_ReturnsWholeCountAndKeepsRemainder()
        {
            var meter = new PendingRowsMeter();
            meter.Add(2.3f);

            var rows = meter.ConsumeWholeRows();

            Assert.AreEqual(2, rows);
        }

        [Test]
        public void ConsumeWholeRows_AfterConsuming_RemainderCarriesForward()
        {
            var meter = new PendingRowsMeter();
            meter.Add(1.5f);
            meter.ConsumeWholeRows();

            meter.Add(0.6f);
            var rows = meter.ConsumeWholeRows();

            Assert.AreEqual(1, rows);
        }

        [Test]
        public void ConsumeWholeRows_CalledTwiceWithoutAdding_SecondCallReturnsZero()
        {
            var meter = new PendingRowsMeter();
            meter.Add(3f);
            meter.ConsumeWholeRows();

            var rows = meter.ConsumeWholeRows();

            Assert.AreEqual(0, rows);
        }

        [Test]
        public void Reset_AfterAdding_ConsumeWholeRowsReturnsZero()
        {
            var meter = new PendingRowsMeter();
            meter.Add(0.8f);

            meter.Reset();
            meter.Add(0.6f);
            var rows = meter.ConsumeWholeRows();

            Assert.AreEqual(0, rows);
        }
    }
}
