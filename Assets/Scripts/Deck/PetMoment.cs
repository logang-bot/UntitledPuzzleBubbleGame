using Game.Pets;

namespace Game.Deck
{
    /// <summary>Everything a launcher pet reacts to this frame (PetAnimSelector picks its strip from it).</summary>
    public struct PetMoment
    {
        public PetRole Role;
        public PetSpecies Species;
        public string Shape;
        public bool RightHanded;
        public int RotateDirection;
        public int CrankStep;
        public float ShotTimeRemaining;
        public float SinceFire;
        public float SinceCheer;
        public bool Panicking;
        public PetOutcome Outcome;
        public float SinceOutcome;
        public float Time;
    }
}
