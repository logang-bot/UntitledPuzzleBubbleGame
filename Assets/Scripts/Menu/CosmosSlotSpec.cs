using UnityEngine;

namespace Game.Menu
{
    /// <summary>
    /// One drifting-cosmos slot: its spawn timing, the sprites it picks from
    /// (the hero slot cycles them in order, the others pick at random), and
    /// the y band its horizontal crossings stay in.
    /// </summary>
    public struct CosmosSlotSpec
    {
        public SpawnSlot Slot;
        public string[] Pool;
        public bool CyclesInOrder;
        public Vector2 Band;

        public string SpriteFor(int cycle, System.Random random)
        {
            if (!CyclesInOrder) return Pool[random.Next(Pool.Length)];
            return Pool[(cycle % Pool.Length + Pool.Length) % Pool.Length];
        }
    }
}
