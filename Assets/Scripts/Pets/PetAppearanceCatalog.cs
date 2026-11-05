using UnityEngine;

namespace Game.Pets
{
    [CreateAssetMenu(menuName = "Game/Pet Appearance Catalog")]
    public class PetAppearanceCatalog : ScriptableObject
    {
        [SerializeField] private PetSpeciesAppearance cat = new();
        [SerializeField] private PetSpeciesAppearance dog = new();

        public PetSpeciesAppearance For(PetSpecies species) => species == PetSpecies.Cat ? cat : dog;
    }
}
