using System.Collections.Generic;
using System.Linq;
using Game.Motion;

namespace Game.BoardArt
{
    /// <summary>Level build-in timing from the motion spec: bubbles scale in top to bottom, left to right.</summary>
    public static class BuildInSchedule
    {
        public const float PerRowSeconds = 0.07f;
        public const float PerColumnSeconds = 0.012f;

        public static float DelayFor((int Row, int Col) cell) => cell.Row * PerRowSeconds + cell.Col * PerColumnSeconds;

        public static float TotalSeconds(IEnumerable<(int Row, int Col)> cells)
        {
            return cells.Select(DelayFor).DefaultIfEmpty(0f).Max() + GameMotions.BuildInSeconds;
        }
    }
}
