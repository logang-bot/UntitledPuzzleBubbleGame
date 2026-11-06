using Game.Art;

namespace Game.Pets
{
    /// <summary>What a pet's sprites need: species, the shape's sprite id and the fur ramp.</summary>
    public struct PetLook
    {
        public PetSpecies Species;
        public string ShapeArtId;
        public FurRamp Ramp;

        public string SpeciesId => Species == PetSpecies.Cat ? "cat" : "dog";

        public static PetLook From(PetAppearanceCatalog catalog, PetSpecies species, PetProfile profile)
        {
            var appearance = catalog.For(species);
            return new PetLook
            {
                Species = species,
                ShapeArtId = appearance.Shapes[CyclicIndex.Clamp(profile.ShapeIndex, appearance.Shapes.Count)].ArtId,
                Ramp = appearance.Colors[CyclicIndex.Clamp(profile.ColorIndex, appearance.Colors.Count)].ToRamp()
            };
        }

        public string PortraitId(string animation) => $"{SpeciesId}_{ShapeArtId}_{animation}";
    }
}
