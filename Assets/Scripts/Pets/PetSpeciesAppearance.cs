using System;
using System.Collections.Generic;

namespace Game.Pets
{
    [Serializable]
    public class PetSpeciesAppearance
    {
        public List<PetColorOption> Colors = new();
        public List<PetShapeOption> Shapes = new();
    }
}
