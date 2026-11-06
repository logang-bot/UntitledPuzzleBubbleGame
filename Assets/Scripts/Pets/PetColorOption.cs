using System;
using Game.Art;
using UnityEngine;

namespace Game.Pets
{
    [Serializable]
    public class PetColorOption
    {
        public string Name;
        public Color[] Ramp = { Color.white, Color.white, Color.white, Color.white };

        public FurRamp ToRamp() => FurRamp.FromShades(Ramp);
    }
}
