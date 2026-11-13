using UnityEngine;

namespace Game.BoardArt
{
    /// <summary>A stable pseudo-random value in [0, 1) for an (index, salt) pair, so seeded layouts repeat.</summary>
    public static class Seed
    {
        public static float Unit(int index, int salt)
        {
            var wave = Mathf.Sin(index * 12.9898f + salt * 78.233f) * 43758.5453f;
            return wave - Mathf.Floor(wave);
        }
    }
}
