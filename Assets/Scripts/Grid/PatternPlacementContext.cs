namespace Game.Grid
{
    public sealed class PatternPlacementContext
    {
        public GridModel Grid { get; set; }
        public PatternGenerationSettings Settings { get; set; }
        public int ColorCount { get; set; }
        public System.Random Rng { get; set; }
    }
}
