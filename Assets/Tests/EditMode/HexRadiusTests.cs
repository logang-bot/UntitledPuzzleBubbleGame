using System.Collections.Generic;
using Game.Grid;
using Game.Superpowers;
using NUnit.Framework;

namespace Game.Tests
{
    public class HexRadiusTests
    {
        [Test]
        public void CellsWithinRadius_RadiusZero_ReturnsOnlyCenter()
        {
            var grid = new GridModel(rows: 5, cols: 5);

            var result = HexRadius.CellsWithinRadius(grid, (2, 2), 0);

            CollectionAssert.AreEquivalent(new List<(int Row, int Col)> { (2, 2) }, result);
        }

        [Test]
        public void CellsWithinRadius_RadiusOne_IncludesAllImmediateNeighbors()
        {
            var grid = new GridModel(rows: 5, cols: 5);
            var expectedNeighbors = grid.GetNeighbors(2, 2);

            var result = HexRadius.CellsWithinRadius(grid, (2, 2), 1);

            foreach (var neighbor in expectedNeighbors)
                CollectionAssert.Contains(result, neighbor);
            CollectionAssert.Contains(result, (2, 2));
        }
    }
}
