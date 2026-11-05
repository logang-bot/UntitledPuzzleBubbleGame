using Game.Pets;
using NUnit.Framework;

namespace Game.Tests
{
    public class CyclicIndexTests
    {
        [Test]
        public void Step_ForwardInsideRange_AdvancesByOne()
        {
            Assert.AreEqual(2, CyclicIndex.Step(index: 1, delta: 1, count: 4));
        }

        [Test]
        public void Step_ForwardPastLast_WrapsToFirst()
        {
            Assert.AreEqual(0, CyclicIndex.Step(index: 3, delta: 1, count: 4));
        }

        [Test]
        public void Step_BackwardPastFirst_WrapsToLast()
        {
            Assert.AreEqual(3, CyclicIndex.Step(index: 0, delta: -1, count: 4));
        }

        [Test]
        public void Step_SingleOption_StaysAtZero()
        {
            Assert.AreEqual(0, CyclicIndex.Step(index: 0, delta: 1, count: 1));
        }

        [Test]
        public void Step_NoOptions_ReturnsZero()
        {
            Assert.AreEqual(0, CyclicIndex.Step(index: 0, delta: 1, count: 0));
        }

        [Test]
        public void Clamp_IndexBeyondCount_ReturnsLastValidIndex()
        {
            Assert.AreEqual(3, CyclicIndex.Clamp(index: 9, count: 4));
        }

        [Test]
        public void Clamp_NegativeIndex_ReturnsZero()
        {
            Assert.AreEqual(0, CyclicIndex.Clamp(index: -2, count: 4));
        }
    }
}
