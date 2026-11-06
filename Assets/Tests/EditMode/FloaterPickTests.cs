using Game.Menu;
using Game.Pets;
using NUnit.Framework;

namespace Game.Tests
{
    public class FloaterPickTests
    {
        [Test]
        public void Roll_SameSeed_GivesSamePick()
        {
            var a = FloaterPick.Roll(new System.Random(42));
            var b = FloaterPick.Roll(new System.Random(42));
            Assert.AreEqual(a.SpriteId("round"), b.SpriteId("round"));
            Assert.AreEqual(a.ColorwayIndex, b.ColorwayIndex);
        }

        [Test]
        public void Roll_ManySeeds_StaysInsideAssetRanges()
        {
            for (var seed = 0; seed < 500; seed++)
            {
                var pick = FloaterPick.Roll(new System.Random(seed));
                Assert.That(pick.ShapeIndex, Is.InRange(0, FloaterPick.ShapeCount - 1));
                Assert.That(pick.ColorwayIndex, Is.InRange(0, FloaterPick.ColorwayCount - 1));
                Assert.That(pick.QuarterTurns, Is.InRange(0, 3));
            }
        }

        [Test]
        public void Roll_ManySeeds_SmallSizeIsRarest()
        {
            var counts = new int[3];
            for (var seed = 0; seed < 2000; seed++)
                counts[(int)FloaterPick.Roll(new System.Random(seed)).Size]++;
            Assert.Less(counts[(int)FloaterSize.S], counts[(int)FloaterSize.M]);
            Assert.Less(counts[(int)FloaterSize.S], counts[(int)FloaterSize.L]);
        }

        [Test]
        public void SpriteId_MatchesFloaterLegendNaming()
        {
            var pick = new FloaterPick { Species = PetSpecies.Dog, PoseIndex = 5, Size = FloaterSize.L };
            Assert.AreEqual("fl_dog_short_tumble_L", pick.SpriteId("short"));
        }
    }
}
