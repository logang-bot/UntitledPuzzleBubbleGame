namespace Game.Pets
{
    /// <summary>
    /// The two-step New Game flow: step 1 edits the aimer, step 2 the firer
    /// (always the other pet). The CAT/DOG toggle names the pet shown on the
    /// current step, so flipping it on step 2 swaps the roles.
    /// See docs/features/game-flow/overview.md.
    /// </summary>
    public class CustomizationFlow
    {
        public CustomizationStep Step { get; private set; } = CustomizationStep.Aimer;
        public PetSpecies Aimer { get; private set; }

        public PetSpecies Edited => Step == CustomizationStep.Aimer ? Aimer : Other(Aimer);

        public CustomizationFlow(PetSpecies aimer)
        {
            Aimer = aimer;
        }

        public static PetSpecies Other(PetSpecies species) => species == PetSpecies.Cat ? PetSpecies.Dog : PetSpecies.Cat;

        public static bool CanAdvance(string petName) => !string.IsNullOrWhiteSpace(petName);

        public void SelectSpecies(PetSpecies shown)
        {
            Aimer = Step == CustomizationStep.Aimer ? shown : Other(shown);
        }

        public void Next() => Step = CustomizationStep.Firer;

        /// <summary>Returns false when already on the first step, i.e. the screen should be left.</summary>
        public bool TryStepBack()
        {
            if (Step == CustomizationStep.Aimer) return false;
            Step = CustomizationStep.Aimer;
            return true;
        }
    }
}
