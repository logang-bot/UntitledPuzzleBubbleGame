using UnityEngine;

namespace Game.Pets
{
    public static class PetProfileStore
    {
        private const string AimerKey = "Pet.Aimer";

        public static PetProfile Load(PetSpecies species) => new PetProfile
        {
            Name = PlayerPrefs.GetString(Key(species, "Name"), DefaultName(species)),
            ColorIndex = PlayerPrefs.GetInt(Key(species, "Color"), 0),
            ShapeIndex = PlayerPrefs.GetInt(Key(species, "Shape"), 0)
        };

        public static void Save(PetSpecies species, PetProfile profile)
        {
            PlayerPrefs.SetString(Key(species, "Name"), profile.Name);
            PlayerPrefs.SetInt(Key(species, "Color"), profile.ColorIndex);
            PlayerPrefs.SetInt(Key(species, "Shape"), profile.ShapeIndex);
            PlayerPrefs.Save();
        }

        public static PetSpecies LoadAimer() => (PetSpecies)PlayerPrefs.GetInt(AimerKey, (int)PetSpecies.Cat);

        public static void SaveAimer(PetSpecies aimer)
        {
            PlayerPrefs.SetInt(AimerKey, (int)aimer);
            PlayerPrefs.Save();
        }

        private static string Key(PetSpecies species, string field) => $"Pet.{species}.{field}";

        private static string DefaultName(PetSpecies species) => species == PetSpecies.Cat ? "Mochi" : "Biscuit";
    }
}
