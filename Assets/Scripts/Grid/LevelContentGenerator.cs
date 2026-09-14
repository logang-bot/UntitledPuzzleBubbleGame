namespace Game.Grid
{
    /// <summary>
    /// Routes level generation to the curated PatternLevelGenerator (when a
    /// pattern is supplied) or the procedural LevelGenerator (otherwise —
    /// e.g. level 6+, which has no catalog entry). See
    /// docs/features/level-content/specs/2026-09-11-curated-levels-and-transitions-design.md.
    /// </summary>
    public static class LevelContentGenerator
    {
        public static GridModel Generate(GridModel grid, int levelNumber, DifficultyConfig difficulty, (LevelPatternPlan Plan, PatternGenerationSettings Settings)? pattern)
        {
            return pattern.HasValue
                ? PatternLevelGenerator.Generate(grid, pattern.Value.Plan, difficulty, pattern.Value.Settings)
                : LevelGenerator.Generate(grid, levelNumber, difficulty);
        }
    }
}
