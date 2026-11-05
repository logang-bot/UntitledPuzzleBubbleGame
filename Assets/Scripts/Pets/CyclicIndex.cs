using UnityEngine;

namespace Game.Pets
{
    public static class CyclicIndex
    {
        /// <summary>
        /// Moves <paramref name="index"/> by <paramref name="delta"/> and wraps it into
        /// [0, count) using ((i % n) + n) % n, which stays non-negative for negative i.
        /// Returns 0 when there are no options.
        /// </summary>
        public static int Step(int index, int delta, int count)
        {
            if (count <= 0) return 0;
            return ((index + delta) % count + count) % count;
        }

        public static int Clamp(int index, int count) => count <= 0 ? 0 : Mathf.Clamp(index, 0, count - 1);
    }
}
