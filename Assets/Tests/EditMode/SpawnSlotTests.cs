using Game.Menu;
using NUnit.Framework;

namespace Game.Tests
{
    public class SpawnSlotTests
    {
        private static readonly SpawnSlot Slot = new SpawnSlot { Id = 3, Period = 10f, Offset = 0.25f, VisibleShare = 0.5f };

        [Test]
        public void TryGetState_WithinVisibleShare_ReportsProgressAndCycle()
        {
            Assert.IsTrue(Slot.TryGetState(time: 12.5f, out var state));
            Assert.AreEqual(1, state.Cycle);
            Assert.AreEqual(1f, state.Progress, 1e-4f);
        }

        [Test]
        public void TryGetState_DuringGap_IsHidden()
        {
            Assert.IsFalse(Slot.TryGetState(time: 15f, out _));
        }

        [Test]
        public void TryGetState_NegativePhase_FloorsToPreviousCycle()
        {
            var slot = new SpawnSlot { Period = 10f, Offset = -0.5f, VisibleShare = 1f };
            Assert.IsTrue(slot.TryGetState(time: 2f, out var state));
            Assert.AreEqual(-1, state.Cycle);
        }

        [Test]
        public void RandomFor_SameCycle_IsDeterministic()
        {
            Assert.AreEqual(Slot.RandomFor(4).Next(), Slot.RandomFor(4).Next());
        }

        [Test]
        public void RandomFor_DifferentCycles_Differ()
        {
            Assert.AreNotEqual(Slot.RandomFor(4).Next(), Slot.RandomFor(5).Next());
        }
    }
}
