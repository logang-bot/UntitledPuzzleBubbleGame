using System.Collections.Generic;
using Game.Superpowers;
using NUnit.Framework;

namespace Game.Tests
{
    public class SuperpowerChargeTrackerTests
    {
        [Test]
        public void ResetForLevel_HighestLevelBelowUnlock_AbilityNotUnlocked()
        {
            var tracker = new SuperpowerChargeTracker();
            var definitions = new List<SuperpowerDefinition>
            {
                new() { Id = SuperpowerId.Freeze, UnlockLevel = 3, ChargesPerLevel = 1 }
            };

            tracker.ResetForLevel(definitions, highestLevelReached: 2);

            Assert.AreEqual(0, tracker.Remaining(SuperpowerId.Freeze));
            CollectionAssert.DoesNotContain(new List<SuperpowerId>(tracker.UnlockedIds), SuperpowerId.Freeze);
        }

        [Test]
        public void ResetForLevel_HighestLevelAtOrAboveUnlock_GrantsConfiguredCharges()
        {
            var tracker = new SuperpowerChargeTracker();
            var definitions = new List<SuperpowerDefinition>
            {
                new() { Id = SuperpowerId.Bomb, UnlockLevel = 6, ChargesPerLevel = 2 }
            };

            tracker.ResetForLevel(definitions, highestLevelReached: 6);

            Assert.AreEqual(2, tracker.Remaining(SuperpowerId.Bomb));
        }

        [Test]
        public void TryConsume_HasCharge_DecrementsAndReturnsTrue()
        {
            var tracker = new SuperpowerChargeTracker();
            var definitions = new List<SuperpowerDefinition>
            {
                new() { Id = SuperpowerId.Freeze, UnlockLevel = 1, ChargesPerLevel = 1 }
            };
            tracker.ResetForLevel(definitions, highestLevelReached: 1);

            var consumed = tracker.TryConsume(SuperpowerId.Freeze);

            Assert.IsTrue(consumed);
            Assert.AreEqual(0, tracker.Remaining(SuperpowerId.Freeze));
        }

        [Test]
        public void TryConsume_NoChargesRemaining_ReturnsFalse()
        {
            var tracker = new SuperpowerChargeTracker();

            var consumed = tracker.TryConsume(SuperpowerId.Freeze);

            Assert.IsFalse(consumed);
        }
    }
}
