namespace Game.Grid
{
    /// <summary>
    /// Produces a populated GridModel for a level, per the difficulty knobs in
    /// DifficultyConfig. See docs/features/core-gameplay/level-generation.md.
    /// </summary>
    public static class LevelGenerator
    {
        public static GridModel Generate(GridModel grid, int levelNumber, DifficultyConfig config)
        {
            var rng = new System.Random(levelNumber);
            var initialRowCount = InitialRowCount(grid, config);
            for (var row = 0; row < initialRowCount; row++)
                FillRow(grid, row, config, rng);
            return grid;
        }

        private static int InitialRowCount(GridModel grid, DifficultyConfig config) =>
            grid.Rows - config.HeadroomRows;

        private static void FillRow(GridModel grid, int row, DifficultyConfig config, System.Random rng)
        {
            for (var col = 0; col < grid.Cols; col++)
            {
                if (rng.NextDouble() > config.Density) continue;
                PlaceWithoutInstantMatch(grid, (row, col), config, rng);
            }
        }

        private static void PlaceWithoutInstantMatch(GridModel grid, (int Row, int Col) cell, DifficultyConfig config, System.Random rng)
        {
            var startIndex = rng.Next(0, config.ColorCount);
            for (var offset = 0; offset < config.ColorCount; offset++)
            {
                var color = BubbleColorPalette.AllColors[(startIndex + offset) % config.ColorCount];
                grid.PlaceBubble(cell.Row, cell.Col, color);
                if (MatchResolver.FindMatchGroup(grid, cell).Count == 0) return;
            }
            grid.ClearCell(cell.Row, cell.Col);
        }
    }
}
