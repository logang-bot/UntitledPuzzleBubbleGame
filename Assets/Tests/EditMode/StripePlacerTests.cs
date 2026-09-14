using Game.Grid;
using NUnit.Framework;

namespace Game.Tests
{
    public class StripePlacerTests
    {
        [Test]
        public void PlaceVertical_CyclesColorsEveryStripeWidthColumns()
        {
            var grid = new GridModel(rows: 3, cols: 9);

            StripePlacer.PlaceVertical(NewContext(grid, colorCount: 3), (0, 3));

            for (var col = 0; col < grid.Cols; col++)
                Assert.That(grid.GetColor(0, col), Is.EqualTo(BubbleColorPalette.AllColors[(col / 3) % 3]));
        }

        [Test]
        public void PlaceHorizontal_CyclesColorsEveryStripeWidthRows()
        {
            var grid = new GridModel(rows: 9, cols: 3);

            StripePlacer.PlaceHorizontal(NewContext(grid, colorCount: 3), (0, 9));

            for (var row = 0; row < grid.Rows; row++)
                Assert.That(grid.GetColor(row, 0), Is.EqualTo(BubbleColorPalette.AllColors[(row / 3) % 3]));
        }

        [Test]
        public void PlaceVertical_FillsOnlyCellsWithinBand()
        {
            var grid = new GridModel(rows: 5, cols: 6);

            StripePlacer.PlaceVertical(NewContext(grid, colorCount: 2), (1, 4));

            for (var row = 1; row < 4; row++)
                for (var col = 0; col < grid.Cols; col++)
                    Assert.That(grid.IsOccupied(row, col), Is.True);

            for (var col = 0; col < grid.Cols; col++)
                Assert.That(grid.IsOccupied(0, col), Is.False);
        }

        private static PatternPlacementContext NewContext(GridModel grid, int colorCount) => new PatternPlacementContext
        {
            Grid = grid,
            Settings = new PatternGenerationSettings { HexBlobRadius = 2, HexBlobGapCells = 1, StripeWidth = 3, RegionGapRows = 1 },
            ColorCount = colorCount,
            Rng = new System.Random(1),
        };
    }
}
