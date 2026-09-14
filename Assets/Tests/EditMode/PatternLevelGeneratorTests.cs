using Game.Grid;
using NUnit.Framework;

namespace Game.Tests
{
    public class PatternLevelGeneratorTests
    {
        [Test]
        public void Generate_HexBlobPlan_NeverLeavesFloatingCells()
        {
            var grid = new GridModel(rows: 12, cols: 8);
            var plan = new LevelPatternPlan { LevelNumber = 1, Regions = new[] { PatternType.HexBlob } };
            var difficulty = new DifficultyConfig { ColorCount = 4, HeadroomRows = 3 };

            PatternLevelGenerator.Generate(grid, plan, difficulty, Settings());

            Assert.That(grid.IsEmpty, Is.False, "hex blobs should survive generation, not be wiped by connectivity cleanup");
            CollectionAssert.IsEmpty(MatchResolver.FindFloatingCells(grid));
        }

        [Test]
        public void Generate_MixedPlan_SplitsIntoOneBandPerRegionWithGapBetween()
        {
            var grid = new GridModel(rows: 18, cols: 8);
            var plan = new LevelPatternPlan { LevelNumber = 4, Regions = new[] { PatternType.HexBlob, PatternType.VerticalStripe } };
            var difficulty = new DifficultyConfig { ColorCount = 4, HeadroomRows = 0 };

            PatternLevelGenerator.Generate(grid, plan, difficulty, Settings());

            var occupiedInGapRow = 0;
            for (var col = 0; col < grid.Cols; col++)
                if (grid.IsOccupied(8, col)) occupiedInGapRow++;
            Assert.That(occupiedInGapRow, Is.LessThanOrEqualTo(1), "gap row between bands should stay empty except for at most one connectivity bridge cell");

            for (var col = 0; col < grid.Cols; col++)
                Assert.That(grid.IsOccupied(16, col), Is.True, "stripe band's last row should be solid");
        }

        [Test]
        public void Generate_HeadroomRows_LeavesBottomRowsEmpty()
        {
            var grid = new GridModel(rows: 12, cols: 8);
            var plan = new LevelPatternPlan { LevelNumber = 3, Regions = new[] { PatternType.HorizontalStripe } };
            var difficulty = new DifficultyConfig { ColorCount = 3, HeadroomRows = 4 };

            PatternLevelGenerator.Generate(grid, plan, difficulty, Settings());

            for (var row = 0; row < 8; row++)
                for (var col = 0; col < grid.Cols; col++)
                    Assert.That(grid.IsOccupied(row, col), Is.True, $"cell ({row},{col}) should be occupied");

            for (var row = 8; row < 12; row++)
                for (var col = 0; col < grid.Cols; col++)
                    Assert.That(grid.IsOccupied(row, col), Is.False, $"cell ({row},{col}) should be empty");
        }

        [Test]
        public void Generate_SameLevelNumberTwice_ProducesIdenticalGrids()
        {
            var plan = new LevelPatternPlan { LevelNumber = 5, Regions = new[] { PatternType.HexBlob, PatternType.VerticalStripe, PatternType.HorizontalStripe } };
            var difficulty = new DifficultyConfig { ColorCount = 5, HeadroomRows = 2 };
            var gridA = new GridModel(rows: 18, cols: 8);
            var gridB = new GridModel(rows: 18, cols: 8);

            PatternLevelGenerator.Generate(gridA, plan, difficulty, Settings());
            PatternLevelGenerator.Generate(gridB, plan, difficulty, Settings());

            for (var row = 0; row < gridA.Rows; row++)
                for (var col = 0; col < gridA.Cols; col++)
                {
                    Assert.That(gridB.IsOccupied(row, col), Is.EqualTo(gridA.IsOccupied(row, col)));
                    if (gridA.IsOccupied(row, col))
                        Assert.That(gridB.GetColor(row, col), Is.EqualTo(gridA.GetColor(row, col)));
                }
        }

        [Test]
        public void Generate_HeadroomExceedsPlayfieldRows_ClampsToEmptyGridWithoutThrowing()
        {
            var grid = new GridModel(rows: 6, cols: 6);
            var plan = new LevelPatternPlan { LevelNumber = 2, Regions = new[] { PatternType.VerticalStripe } };
            var difficulty = new DifficultyConfig { ColorCount = 3, HeadroomRows = 40 };

            Assert.DoesNotThrow(() => PatternLevelGenerator.Generate(grid, plan, difficulty, Settings()));
            Assert.That(grid.IsEmpty, Is.True);
        }

        [Test]
        public void Generate_EmptyRegionsArray_DoesNotThrowAndLeavesGridEmpty()
        {
            var grid = new GridModel(rows: 10, cols: 6);
            var plan = new LevelPatternPlan { LevelNumber = 1, Regions = new PatternType[0] };
            var difficulty = new DifficultyConfig { ColorCount = 4, HeadroomRows = 0 };

            Assert.DoesNotThrow(() => PatternLevelGenerator.Generate(grid, plan, difficulty, Settings()));
            Assert.That(grid.IsEmpty, Is.True);
        }

        [Test]
        public void Generate_MixedPlan_HexBlobBandIsNotEmpty()
        {
            var grid = new GridModel(rows: 18, cols: 8);
            var plan = new LevelPatternPlan { LevelNumber = 4, Regions = new[] { PatternType.HexBlob, PatternType.VerticalStripe } };
            var difficulty = new DifficultyConfig { ColorCount = 4, HeadroomRows = 0 };

            PatternLevelGenerator.Generate(grid, plan, difficulty, Settings());

            var hexBandOccupied = false;
            for (var row = 0; row < 8; row++)
                for (var col = 0; col < grid.Cols; col++)
                    if (grid.IsOccupied(row, col)) hexBandOccupied = true;
            Assert.That(hexBandOccupied, Is.True, "hex-blob band should contain at least one blob");
        }

        [Test]
        public void Generate_HexBlobBandShorterThanConfiguredRadius_StillPlacesSomething()
        {
            var grid = new GridModel(rows: 10, cols: 8);
            var plan = new LevelPatternPlan { LevelNumber = 5, Regions = new[] { PatternType.HexBlob, PatternType.VerticalStripe, PatternType.HorizontalStripe } };
            var difficulty = new DifficultyConfig { ColorCount = 4, HeadroomRows = 0 };

            PatternLevelGenerator.Generate(grid, plan, difficulty, Settings());

            var hexBandOccupied = false;
            for (var row = 0; row < 2; row++)
                for (var col = 0; col < grid.Cols; col++)
                    if (grid.IsOccupied(row, col)) hexBandOccupied = true;
            Assert.That(hexBandOccupied, Is.True, "hex-blob band should place at least a clamped-radius blob even when too short for the configured radius");
        }

        private static PatternGenerationSettings Settings() => new PatternGenerationSettings
        {
            HexBlobRadius = 2,
            HexBlobGapCells = 1,
            StripeWidth = 3,
            RegionGapRows = 1,
        };
    }
}
