using System;

namespace Game.Menu
{
    /// <summary>
    /// A repeating spawn slot from the main-menu design: every Period seconds
    /// it starts a new cycle, is busy for the first VisibleShare of it (a
    /// sprite crossing the screen), then idles. Each cycle gets its own
    /// deterministic random stream, so a cycle's picks never change mid-flight.
    /// </summary>
    public struct SpawnSlot
    {
        public int Id;
        public float Period;
        public float Offset;
        public float VisibleShare;

        /// <summary>
        /// s = t / Period + Offset; cycle = floor(s); progress = frac(s) / VisibleShare,
        /// which runs 0→1 while the sprite is on screen and exceeds 1 during the gap.
        /// </summary>
        public bool TryGetState(float time, out SlotState state)
        {
            var s = time / Period + Offset;
            var cycle = (int)Math.Floor(s);
            state = new SlotState { Cycle = cycle, Progress = (float)(s - cycle) / VisibleShare };
            return state.Progress <= 1f;
        }

        public Random RandomFor(int cycle) => new Random(unchecked(Id * 7919 + cycle * 104729 + 13));
    }
}
