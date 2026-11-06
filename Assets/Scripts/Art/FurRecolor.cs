using Game.Pets;
using UnityEngine;

namespace Game.Art
{
    /// <summary>
    /// Drives the Game/UI/FurRecolor shader: each recolored Image gets its
    /// own material instance whose cat or dog ramp holds the chosen colorway.
    /// </summary>
    public static class FurRecolor
    {
        public static Material CreateInstance(Material template) => new Material(template) { name = template.name + " (Pet)" };

        public static void Apply(Material material, PetSpecies species, FurRamp ramp)
        {
            var prefix = species == PetSpecies.Cat ? "_CatRamp" : "_DogRamp";
            for (var shade = 0; shade < FurRamp.ShadeCount; shade++)
                material.SetColor(prefix + shade, ramp[shade]);
        }
    }
}
