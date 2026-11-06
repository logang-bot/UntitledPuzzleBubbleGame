using Game.Pets;
using NUnit.Framework;

namespace Game.Tests
{
    public class CustomizationFlowTests
    {
        [Test]
        public void Start_EditsTheSavedAimer()
        {
            var flow = new CustomizationFlow(PetSpecies.Dog);
            Assert.AreEqual(CustomizationStep.Aimer, flow.Step);
            Assert.AreEqual(PetSpecies.Dog, flow.Edited);
        }

        [Test]
        public void SelectSpecies_OnAimerStep_MakesItTheAimer()
        {
            var flow = new CustomizationFlow(PetSpecies.Cat);
            flow.SelectSpecies(PetSpecies.Dog);
            Assert.AreEqual(PetSpecies.Dog, flow.Aimer);
        }

        [Test]
        public void Next_EditsTheOtherPetAsFirer()
        {
            var flow = new CustomizationFlow(PetSpecies.Cat);
            flow.Next();
            Assert.AreEqual(CustomizationStep.Firer, flow.Step);
            Assert.AreEqual(PetSpecies.Dog, flow.Edited);
        }

        [Test]
        public void SelectSpecies_OnFirerStep_SwapsRoles()
        {
            var flow = new CustomizationFlow(PetSpecies.Cat);
            flow.Next();
            flow.SelectSpecies(PetSpecies.Cat);
            Assert.AreEqual(PetSpecies.Dog, flow.Aimer);
            Assert.AreEqual(PetSpecies.Cat, flow.Edited);
        }

        [Test]
        public void TryStepBack_OnFirerStep_ReturnsToAimer()
        {
            var flow = new CustomizationFlow(PetSpecies.Cat);
            flow.Next();
            Assert.IsTrue(flow.TryStepBack());
            Assert.AreEqual(CustomizationStep.Aimer, flow.Step);
        }

        [Test]
        public void TryStepBack_OnAimerStep_SignalsLeavingTheScreen()
        {
            Assert.IsFalse(new CustomizationFlow(PetSpecies.Cat).TryStepBack());
        }

        [Test]
        public void CanAdvance_BlankName_IsFalse()
        {
            Assert.IsFalse(CustomizationFlow.CanAdvance("   "));
            Assert.IsTrue(CustomizationFlow.CanAdvance("Mochi"));
        }
    }
}
