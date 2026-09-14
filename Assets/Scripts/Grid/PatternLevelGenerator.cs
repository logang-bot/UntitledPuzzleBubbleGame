using System.Collections.Generic;
using System.Linq;

namespace Game.Grid
{
    /// <summary>
    /// Produces a populated GridModel from a curated LevelPatternPlan
    /// (levels 1-5). See
    /// docs/features/level-content/specs/2026-09-11-curated-levels-and-transitions-design.md.
    /// </summary>
    public static class PatternLevelGenerator
    {
        public static GridModel Generate(GridModel grid, LevelPatternPlan plan, DifficultyConfig difficulty, PatternGenerationSettings settings)
        {
            var context = NewContext(grid, plan, difficulty, settings);
            foreach (var band in Bands(context, plan, difficulty.HeadroomRows))
                PlaceRegion(context, band.RowRange, band.Type);
            ConnectFloatingCellsToCeiling(grid);
            return grid;
        }

        private static PatternPlacementContext NewContext(GridModel grid, LevelPatternPlan plan, DifficultyConfig difficulty, PatternGenerationSettings settings) => new PatternPlacementContext
        {
            Grid = grid,
            Settings = settings,
            ColorCount = difficulty.ColorCount,
            Rng = new System.Random(plan.LevelNumber),
        };

        private static void PlaceRegion(PatternPlacementContext context, (int Start, int EndExclusive) rowRange, PatternType type)
        {
            switch (type)
            {
                case PatternType.HexBlob: HexBlobPlacer.Place(context, rowRange); break;
                case PatternType.VerticalStripe: StripePlacer.PlaceVertical(context, rowRange); break;
                case PatternType.HorizontalStripe: StripePlacer.PlaceHorizontal(context, rowRange); break;
            }
        }

        private static IEnumerable<(PatternType Type, (int Start, int EndExclusive) RowRange)> Bands(PatternPlacementContext context, LevelPatternPlan plan, int headroomRows)
        {
            if (plan.Regions == null || plan.Regions.Length == 0) yield break;

            var fillableRows = context.Grid.Rows - headroomRows;
            var regionCount = plan.Regions.Length;
            var totalGapRows = context.Settings.RegionGapRows * (regionCount - 1);
            var bandHeight = (fillableRows - totalGapRows) / regionCount;
            var row = 0;
            foreach (var region in plan.Regions)
            {
                yield return (region, (row, row + bandHeight));
                row += bandHeight + context.Settings.RegionGapRows;
            }
        }

        // HexBlobPlacer's gap-separated blobs and RegionGapRows's empty band
        // gaps are both intentional (see the spec), so disconnection from the
        // ceiling is the norm here, not a rare accident like in LevelGenerator
        // - deleting on disconnect would gut most of a pattern level's
        // content. Bridge each disconnected component to the ceiling instead,
        // with a single same-color vertical stem.
        private static void ConnectFloatingCellsToCeiling(GridModel grid)
        {
            var floating = MatchResolver.FindFloatingCells(grid);
            while (floating.Count > 0)
            {
                var seed = floating.OrderBy(cell => cell.Row).ThenBy(cell => cell.Col).First();
                var component = FloodFill.Run(grid, new[] { seed }, cell => grid.IsOccupied(cell.Row, cell.Col));
                BridgeUpward(grid, component.OrderBy(cell => cell.Row).ThenBy(cell => cell.Col).First());
                floating = MatchResolver.FindFloatingCells(grid);
            }
        }

        // (row - 1, same column) is a valid hex neighbor for both even and
        // odd rows (see GridModel's EvenRowOffsets/OddRowOffsets, both
        // include (-1, 0)), so a straight column line is always hex-connected.
        private static void BridgeUpward(GridModel grid, (int Row, int Col) cell)
        {
            var color = grid.GetColor(cell.Row, cell.Col);
            for (var row = cell.Row - 1; row >= 0 && !grid.IsOccupied(row, cell.Col); row--)
                grid.PlaceBubble(row, cell.Col, color);
        }
    }
}
