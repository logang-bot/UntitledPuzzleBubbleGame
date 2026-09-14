using Game.Grid;
using NUnit.Framework;

namespace Game.Tests
{
    public class LevelContentGeneratorTests
    {
        [Test]
        public void Generate_WithPattern_MatchesPatternLevelGeneratorDirectly()
        {
            var plan = new LevelPatternPlan { LevelNumber = 1, Regions = new[] { PatternType.HexBlob } };
            var settings = new PatternGenerationSettings { HexBlobRadius = 2, HexBlobGapCells = 1, StripeWidth = 3, RegionGapRows = 1 };
            var difficulty = new DifficultyConfig { ColorCount = 4, HeadroomRows = 2 };
            var gridA = new GridModel(rows: 12, cols: 8);
            var gridB = new GridModel(rows: 12, cols: 8);

            LevelContentGenerator.Generate(gridA, levelNumber: 1, difficulty, (plan, settings));
            PatternLevelGenerator.Generate(gridB, plan, difficulty, settings);

            for (var row = 0; row < gridA.Rows; row++)
                for (var col = 0; col < gridA.Cols; col++)
                {
                    Assert.That(gridB.IsOccupied(row, col), Is.EqualTo(gridA.IsOccupied(row, col)));
                    if (gridA.IsOccupied(row, col))
                        Assert.That(gridB.GetColor(row, col), Is.EqualTo(gridA.GetColor(row, col)));
                }
        }

        [Test]
        public void Generate_WithoutPattern_MatchesLevelGeneratorDirectly()
        {
            var difficulty = new DifficultyConfig { ColorCount = 4, Density = 0.6f, HeadroomRows = 1 };
            var gridA = new GridModel(rows: 8, cols: 6);
            var gridB = new GridModel(rows: 8, cols: 6);

            LevelContentGenerator.Generate(gridA, levelNumber: 7, difficulty, null);
            LevelGenerator.Generate(gridB, levelNumber: 7, difficulty);

            for (var row = 0; row < gridA.Rows; row++)
                for (var col = 0; col < gridA.Cols; col++)
                {
                    Assert.That(gridB.IsOccupied(row, col), Is.EqualTo(gridA.IsOccupied(row, col)));
                    if (gridA.IsOccupied(row, col))
                        Assert.That(gridB.GetColor(row, col), Is.EqualTo(gridA.GetColor(row, col)));
                }
        }
    }
}
