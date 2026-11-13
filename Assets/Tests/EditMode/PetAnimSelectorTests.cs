using Game.Deck;
using Game.Pets;
using NUnit.Framework;

namespace Game.Tests
{
    public class PetAnimSelectorTests
    {
        private static PetMoment Calm(PetRole role, PetSpecies species) => new PetMoment
        {
            Role = role,
            Species = species,
            Shape = species == PetSpecies.Cat ? "chonk" : "pointy",
            RightHanded = true,
            ShotTimeRemaining = 10f,
            SinceFire = 5f,
            SinceCheer = 5f,
            Time = 1f
        };

        [Test]
        public void Aimer_Idle_HoldsAndMirrorsForTheRightHand()
        {
            var pose = PetAnimSelector.Pick(Calm(PetRole.Aimer, PetSpecies.Cat));

            Assert.AreEqual("game_cat_chonk_aim_hold", pose.Id);
            Assert.IsTrue(pose.Flip);
        }

        [Test]
        public void DogAimer_Rotating_ShowsTheCrankStepFrame()
        {
            var moment = Calm(PetRole.Aimer, PetSpecies.Dog);
            moment.RotateDirection = -1;
            moment.CrankStep = -3;

            var pose = PetAnimSelector.Pick(moment);

            Assert.AreEqual("game_dog_pointy_aim_right", pose.Id);
            Assert.AreEqual(5, pose.FixedFrame);
        }

        [Test]
        public void CatAimer_TurningRightWithTheRightHand_ReachesLeft()
        {
            var moment = Calm(PetRole.Aimer, PetSpecies.Cat);
            moment.RotateDirection = 1;

            Assert.AreEqual("game_cat_chonk_aim_left", PetAnimSelector.Pick(moment).Id);
        }

        [Test]
        public void Firer_JustFired_PullsWithTheOppositeMirror()
        {
            var moment = Calm(PetRole.Firer, PetSpecies.Cat);
            moment.SinceFire = 0.2f;

            var pose = PetAnimSelector.Pick(moment);

            Assert.AreEqual("game_cat_chonk_pull", pose.Id);
            Assert.AreEqual(2, pose.FixedFrame);
            Assert.IsFalse(pose.Flip);
        }

        [Test]
        public void Firer_Hurrying_FacesFrontUnmirrored()
        {
            var moment = Calm(PetRole.Firer, PetSpecies.Dog);
            moment.ShotTimeRemaining = 2f;

            var pose = PetAnimSelector.Pick(moment);

            Assert.AreEqual("game_dog_pointy_hurry_firer", pose.Id);
            Assert.IsFalse(pose.Flip);
        }

        [Test]
        public void Panic_OutranksCheerAndFire()
        {
            var moment = Calm(PetRole.Firer, PetSpecies.Cat);
            moment.Panicking = true;
            moment.SinceCheer = 0f;
            moment.SinceFire = 0f;

            Assert.AreEqual("game_cat_chonk_panic", PetAnimSelector.Pick(moment).Id);
        }

        [Test]
        public void Lost_KnocksOutOnceThenNaps()
        {
            var moment = Calm(PetRole.Aimer, PetSpecies.Dog);
            moment.Outcome = PetOutcome.Lost;

            Assert.AreEqual("game_dog_pointy_knock_aimer", PetAnimSelector.Pick(moment).Id);
            moment.SinceOutcome = 1f;
            Assert.AreEqual("game_dog_pointy_ko_aimer", PetAnimSelector.Pick(moment).Id);
        }
    }
}
