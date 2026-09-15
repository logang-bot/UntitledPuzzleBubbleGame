using Game.Battle;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class BattleAttackConfigTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void AttackForPop_MinimumMatchSize_ReturnsQuadraticValue()
        {
            var config = ScriptableObject.CreateInstance<BattleAttackConfig>();

            var attack = config.AttackForPop(bubbleCount: 3);

            Assert.That(attack, Is.EqualTo(0.3f).Within(Tolerance));
        }

        [Test]
        public void AttackForPop_LargerMatch_ScalesFasterThanLinear()
        {
            var config = ScriptableObject.CreateInstance<BattleAttackConfig>();

            var smallMatch = config.AttackForPop(bubbleCount: 3);
            var largeMatch = config.AttackForPop(bubbleCount: 6);

            Assert.That(largeMatch, Is.GreaterThan(smallMatch * 2f));
        }

        [Test]
        public void AttackForDrop_FiveBubbles_ReturnsLinearValue()
        {
            var config = ScriptableObject.CreateInstance<BattleAttackConfig>();

            var attack = config.AttackForDrop(bubbleCount: 5);

            Assert.That(attack, Is.EqualTo(0.75f).Within(Tolerance));
        }
    }
}
