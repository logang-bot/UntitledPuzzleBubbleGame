using Game.Grid;
using NUnit.Framework;

namespace Game.Tests
{
    public class LevelGeneratorTests
    {
        [Test]
        public void Generate_FullDensity_FillsEveryCellInInitialRows()
        {
            var grid = new GridModel(rows: 4, cols: 5);
            var config = new DifficultyConfig { ColorCount = 6, Density = 1f, HeadroomRows = 0 };

            LevelGenerator.Generate(grid, levelNumber: 1, config);

            for (var row = 0; row < grid.Rows; row++)
                for (var col = 0; col < grid.Cols; col++)
                    Assert.That(grid.IsOccupied(row, col), Is.True, $"cell ({row},{col}) should be occupied");
        }

        [Test]
        public void Generate_ZeroDensity_LeavesGridEmpty()
        {
            var grid = new GridModel(rows: 4, cols: 5);
            var config = new DifficultyConfig { ColorCount = 6, Density = 0f, HeadroomRows = 0 };

            LevelGenerator.Generate(grid, levelNumber: 1, config);

            Assert.That(grid.IsEmpty, Is.True);
        }

        [Test]
        public void Generate_HeadroomRows_LeavesBottomRowsEmpty()
        {
            var grid = new GridModel(rows: 10, cols: 5);
            var config = new DifficultyConfig { ColorCount = 6, Density = 1f, HeadroomRows = 4 };

            LevelGenerator.Generate(grid, levelNumber: 1, config);

            for (var row = 0; row < 6; row++)
                for (var col = 0; col < grid.Cols; col++)
                    Assert.That(grid.IsOccupied(row, col), Is.True, $"cell ({row},{col}) should be occupied");

            for (var row = 6; row < 10; row++)
                for (var col = 0; col < grid.Cols; col++)
                    Assert.That(grid.IsOccupied(row, col), Is.False, $"cell ({row},{col}) should be empty");
        }

        [Test]
        public void Generate_HeadroomExceedsPlayfieldRows_ClampsToEmptyGridWithoutThrowing()
        {
            var grid = new GridModel(rows: 4, cols: 5);
            var config = new DifficultyConfig { ColorCount = 6, Density = 1f, HeadroomRows = 40 };

            Assert.DoesNotThrow(() => LevelGenerator.Generate(grid, levelNumber: 1, config));
            Assert.That(grid.IsEmpty, Is.True);
        }

        [Test]
        public void Generate_ColorCountRestriction_NeverPlacesColorsBeyondCount()
        {
            var grid = new GridModel(rows: 4, cols: 5);
            var config = new DifficultyConfig { ColorCount = 2, Density = 1f, HeadroomRows = 0 };
            var allowedColors = new[] { BubbleColorPalette.AllColors[0], BubbleColorPalette.AllColors[1] };

            LevelGenerator.Generate(grid, levelNumber: 1, config);

            foreach (var cell in grid.OccupiedCells())
                Assert.That(allowedColors, Does.Contain(grid.GetColor(cell.Row, cell.Col)));
        }

        [Test]
        public void Generate_ColorCountRestriction_UsesMoreThanOneColorWhenAllowed()
        {
            var grid = new GridModel(rows: 4, cols: 5);
            var config = new DifficultyConfig { ColorCount = 2, Density = 1f, HeadroomRows = 0 };

            LevelGenerator.Generate(grid, levelNumber: 1, config);

            var distinctColors = new System.Collections.Generic.HashSet<BubbleColor>();
            foreach (var cell in grid.OccupiedCells())
                distinctColors.Add(grid.GetColor(cell.Row, cell.Col));
            Assert.That(distinctColors.Count, Is.GreaterThan(1));
        }

        [Test]
        public void Generate_SameLevelNumberTwice_ProducesIdenticalGrids()
        {
            var config = new DifficultyConfig { ColorCount = 4, Density = 0.6f, HeadroomRows = 1 };
            var gridA = new GridModel(rows: 8, cols: 6);
            var gridB = new GridModel(rows: 8, cols: 6);

            LevelGenerator.Generate(gridA, levelNumber: 7, config);
            LevelGenerator.Generate(gridB, levelNumber: 7, config);

            for (var row = 0; row < gridA.Rows; row++)
                for (var col = 0; col < gridA.Cols; col++)
                {
                    Assert.That(gridB.IsOccupied(row, col), Is.EqualTo(gridA.IsOccupied(row, col)));
                    if (gridA.IsOccupied(row, col))
                        Assert.That(gridB.GetColor(row, col), Is.EqualTo(gridA.GetColor(row, col)));
                }
        }

        [Test]
        public void Generate_DifferentLevelNumbers_CanProduceDifferentGrids()
        {
            var config = new DifficultyConfig { ColorCount = 4, Density = 0.6f, HeadroomRows = 1 };
            var gridA = new GridModel(rows: 8, cols: 6);
            var gridB = new GridModel(rows: 8, cols: 6);

            LevelGenerator.Generate(gridA, levelNumber: 7, config);
            LevelGenerator.Generate(gridB, levelNumber: 8, config);

            var identical = true;
            for (var row = 0; row < gridA.Rows && identical; row++)
                for (var col = 0; col < gridA.Cols; col++)
                {
                    if (gridA.IsOccupied(row, col) == gridB.IsOccupied(row, col)) continue;
                    identical = false;
                    break;
                }

            Assert.That(identical, Is.False);
        }

        [Test]
        public void Generate_WorstCaseLowColorCountHighDensity_NeverLeavesAMatchOfThreeOrMore()
        {
            var config = new DifficultyConfig { ColorCount = 2, Density = 1f, HeadroomRows = 0 };

            for (var levelNumber = 1; levelNumber <= 100; levelNumber++)
            {
                var grid = new GridModel(rows: 6, cols: 8);
                LevelGenerator.Generate(grid, levelNumber, config);

                foreach (var cell in grid.OccupiedCells())
                    Assert.That(MatchResolver.FindMatchGroup(grid, cell), Is.Empty,
                        $"level {levelNumber}, cell ({cell.Row},{cell.Col}) formed a match of 3+");
            }
        }
    }
}
