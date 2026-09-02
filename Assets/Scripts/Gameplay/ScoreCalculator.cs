namespace Game.Gameplay
{
    /// <summary>
    /// Pure scoring formula, weighted for cluster size and cascades: matches
    /// grow quadratically with bubble count (rewarding big pops), cascade
    /// drops pay a flat higher per-bubble bonus since they're free chain
    /// reaction points. See docs/features/core-gameplay/hud-and-level-flow.md.
    /// </summary>
    public static class ScoreCalculator
    {
        private const int PopPointsPerBubble = 10;
        private const int DropPointsPerBubble = 20;

        public static int PointsForPop(int bubbleCount) => PopPointsPerBubble * bubbleCount * (bubbleCount - 1);
        public static int PointsForDrop(int bubbleCount) => DropPointsPerBubble * bubbleCount;
    }
}
