using Game.Grid;
using NUnit.Framework;

namespace Game.Tests
{
    public class HexBlobPlacerTests
    {
        [Test]
        public void Place_OnlyFillsCellsWithinRowBand()
        {
            var grid = new GridModel(rows: 10, cols: 8);
            var context = NewContext(grid, colorCount: 4, seed: 1);

            HexBlobPlacer.Place(context, (2, 7));

            foreach (var cell in grid.OccupiedCells())
                Assert.That(cell.Row, Is.InRange(2, 6));
        }

        [Test]
        public void Place_NeverPutsDifferentlyColoredCellsAdjacent()
        {
            var grid = new GridModel(rows: 10, cols: 8);
            var context = NewContext(grid, colorCount: 6, seed: 2);

            HexBlobPlacer.Place(context, (0, 10));

            foreach (var cell in grid.OccupiedCells())
                foreach (var neighbor in grid.GetNeighbors(cell.Row, cell.Col))
                    if (grid.IsOccupied(neighbor.Row, neighbor.Col))
                        Assert.That(grid.GetColor(neighbor.Row, neighbor.Col), Is.EqualTo(grid.GetColor(cell.Row, cell.Col)));
        }

        [Test]
        public void Place_OnEmptyFullGridBand_ProducesAtLeastOneBlob()
        {
            var grid = new GridModel(rows: 10, cols: 8);
            var context = NewContext(grid, colorCount: 3, seed: 3);

            HexBlobPlacer.Place(context, (0, 10));

            Assert.That(grid.IsEmpty, Is.False);
        }

        [Test]
        public void Place_SameSeedTwice_ProducesIdenticalGrids()
        {
            var gridA = new GridModel(rows: 10, cols: 8);
            var gridB = new GridModel(rows: 10, cols: 8);

            HexBlobPlacer.Place(NewContext(gridA, colorCount: 4, seed: 5), (0, 10));
            HexBlobPlacer.Place(NewContext(gridB, colorCount: 4, seed: 5), (0, 10));

            for (var row = 0; row < gridA.Rows; row++)
                for (var col = 0; col < gridA.Cols; col++)
                {
                    Assert.That(gridB.IsOccupied(row, col), Is.EqualTo(gridA.IsOccupied(row, col)));
                    if (gridA.IsOccupied(row, col))
                        Assert.That(gridB.GetColor(row, col), Is.EqualTo(gridA.GetColor(row, col)));
                }
        }

        [Test]
        public void Place_BandShorterThanConfiguredRadius_ClampsRadiusAndStillPlacesBlobs()
        {
            var grid = new GridModel(rows: 6, cols: 8);
            var context = NewContext(grid, colorCount: 3, seed: 7);

            HexBlobPlacer.Place(context, (0, 2));

            Assert.That(grid.IsEmpty, Is.False, "a 2-row band should still get a radius-clamped blob, not nothing");
            foreach (var cell in grid.OccupiedCells())
                Assert.That(cell.Row, Is.InRange(0, 1));
        }

        private static PatternPlacementContext NewContext(GridModel grid, int colorCount, int seed) => new PatternPlacementContext
        {
            Grid = grid,
            Settings = new PatternGenerationSettings { HexBlobRadius = 2, HexBlobGapCells = 1, StripeWidth = 3, RegionGapRows = 1 },
            ColorCount = colorCount,
            Rng = new System.Random(seed),
        };
    }
}
