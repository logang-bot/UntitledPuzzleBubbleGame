using System;

namespace Game.Grid
{
    [Serializable]
    public sealed class LevelPatternPlan
    {
        public int LevelNumber;
        public PatternType[] Regions;
    }
}
