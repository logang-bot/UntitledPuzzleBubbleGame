using Game.Pets;
using Game.Settings;

namespace Game.Deck
{
    /// <summary>
    /// What a deck shows: solo follows the saved aimer and hand (the crank
    /// and lever when the dog aims, the wheel and button when the cat aims,
    /// with the other pet firing); Battle halves are right-handed with one
    /// pet each, the cat at the bottom and the dog at the top.
    /// </summary>
    public readonly struct DeckSetup
    {
        public PetSpecies Aimer { get; }
        public bool HasFirer { get; }
        public DeckGeometry Geometry { get; }
        public PetSpecies Firer => Aimer == PetSpecies.Cat ? PetSpecies.Dog : PetSpecies.Cat;
        public MechanismKind Kind => Aimer == PetSpecies.Dog ? MechanismKind.Crank : MechanismKind.Wheel;

        private DeckSetup(PetSpecies aimer, bool hasFirer, DeckGeometry geometry)
        {
            Aimer = aimer;
            HasFirer = hasFirer;
            Geometry = geometry;
        }

        public static DeckSetup For(DeckPlacement placement, PetSpecies soloAimer, Handedness hand)
        {
            if (placement == DeckPlacement.Solo)
                return new DeckSetup(soloAimer, true, new DeckGeometry(hand == Handedness.Right, DeckLayout.SoloMechanismTop));
            var aimer = placement == DeckPlacement.BattleTop ? PetSpecies.Dog : PetSpecies.Cat;
            return new DeckSetup(aimer, false, new DeckGeometry(true, DeckLayout.BattleMechanismTop));
        }

        public MechanismParts NewParts() => new MechanismParts { Kind = Kind, HasFirer = HasFirer, Geometry = Geometry };
    }
}
