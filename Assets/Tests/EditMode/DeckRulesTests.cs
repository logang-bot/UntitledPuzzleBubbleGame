using Game.Art;
using Game.Deck;
using Game.Pets;
using Game.Settings;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class DeckRulesTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void Geometry_RightHand_MirrorsXAndShiftsToTheMechanism()
        {
            var right = new DeckGeometry(rightHanded: true, mechanismTop: 640f);

            Assert.That(right.Point(new Vector2(60f, 10f)), Is.EqualTo(new Vector2(300f, 650f)));
            Assert.That(right.Rect(new Rect(0f, 28f, 43.33f, 76f)).x, Is.EqualTo(316.67f).Within(Tolerance));
        }

        [Test]
        public void Geometry_PetFrame_RightAlignsWhenMirrored()
        {
            var right = new DeckGeometry(rightHanded: true, mechanismTop: 0f);

            Assert.That(right.PetLeft(artX: 50f, artWidth: 40f), Is.EqualTo(360f - 90f * DeckLayout.PtPerArtPx).Within(Tolerance));
        }

        [Test]
        public void Setup_DogAims_UsesTheCrankWithTheCatFiring()
        {
            var setup = DeckSetup.For(DeckPlacement.Solo, PetSpecies.Dog, Handedness.Left);

            Assert.AreEqual(MechanismKind.Crank, setup.Kind);
            Assert.AreEqual(PetSpecies.Cat, setup.Firer);
            Assert.IsFalse(setup.Geometry.RightHanded);
        }

        [Test]
        public void Setup_BattleTop_IsTheDogAloneOnTheRightHand()
        {
            var setup = DeckSetup.For(DeckPlacement.BattleTop, PetSpecies.Cat, Handedness.Left);

            Assert.AreEqual(PetSpecies.Dog, setup.Aimer);
            Assert.IsFalse(setup.HasFirer);
            Assert.IsTrue(setup.Geometry.RightHanded);
        }

        [Test]
        public void ShotClock_LightsOnePipPerSecondLeft()
        {
            Assert.AreEqual(12, ShotClockDisplay.LitPips(12f));
            Assert.AreEqual(5, ShotClockDisplay.LitPips(4.2f));
            Assert.AreEqual(0, ShotClockDisplay.LitPips(0f));
        }

        [Test]
        public void ShotClock_UnlitPip_IsOff()
        {
            Assert.AreEqual(GameColors.PipOff, ShotClockDisplay.PipColor(index: 6, remaining: 5.5f, time: 0f));
        }

        [Test]
        public void ShotClock_HurriesOnlyInTheLastFourSeconds()
        {
            Assert.IsFalse(ShotClockDisplay.IsHurrying(4.01f));
            Assert.IsTrue(ShotClockDisplay.IsHurrying(4f));
            Assert.IsFalse(ShotClockDisplay.IsHurrying(0f));
            Assert.AreEqual(1.26f, ShotClockDisplay.Urgency(1));
        }
    }
}
