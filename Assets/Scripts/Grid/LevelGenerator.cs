using System.Linq;

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
            RescueRowZeroIfOrphaned(grid, config, rng);
            ClearFloatingCells(grid);
            return grid;
        }

        // Row 0's per-cell density roll can come up entirely empty by chance,
        // which would disconnect every bubble below it from the ceiling and
        // have ClearFloatingCells wipe the whole level. Force one bubble into
        // row 0 to rescue them - but only when there's something below worth
        // rescuing; a genuinely empty level (e.g. zero density) stays empty.
        private static void RescueRowZeroIfOrphaned(GridModel grid, DifficultyConfig config, System.Random rng)
        {
            if (grid.Cols == 0 || RowHasAnyOccupied(grid, row: 0) || !grid.OccupiedCells().Any()) return;
            var col = rng.Next(0, grid.Cols);
            PlaceWithoutInstantMatch(grid, (0, col), config, rng);
        }

        private static bool RowHasAnyOccupied(GridModel grid, int row)
        {
            for (var col = 0; col < grid.Cols; col++)
                if (grid.IsOccupied(row, col)) return true;
            return false;
        }

        private static void ClearFloatingCells(GridModel grid)
        {
            foreach (var cell in MatchResolver.FindFloatingCells(grid))
                grid.ClearCell(cell.Row, cell.Col);
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
