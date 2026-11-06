using System;
using UnityEngine;

namespace Game.Art
{
    /// <summary>
    /// A 4-shade fur ramp, darkest to lightest, matching the 4 fur-key
    /// palette slots the pet sprites are drawn with.
    /// </summary>
    [Serializable]
    public struct FurRamp
    {
        public const int ShadeCount = 4;

        public Color Darkest;
        public Color Dark;
        public Color Light;
        public Color Lightest;

        public Color this[int shade] => shade switch
        {
            0 => Darkest,
            1 => Dark,
            2 => Light,
            _ => Lightest
        };

        public static FurRamp FromShades(Color[] shades) => new FurRamp
        {
            Darkest = shades[0],
            Dark = shades[1],
            Light = shades[2],
            Lightest = shades[3]
        };
    }
}
