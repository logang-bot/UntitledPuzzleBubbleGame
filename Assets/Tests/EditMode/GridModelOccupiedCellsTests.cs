using System.Collections.Generic;
using Game.Grid;
using NUnit.Framework;

namespace Game.Tests
{
    public class GridModelOccupiedCellsTests
    {
        [Test]
        public void OccupiedCells_EmptyGrid_ReturnsEmpty()
        {
            var grid = new GridModel(rows: 3, cols: 3);

            var cells = grid.OccupiedCells();

            CollectionAssert.IsEmpty(cells);
        }

        [Test]
        public void OccupiedCells_SomeCellsPlaced_ReturnsOnlyThose()
        {
            var grid = new GridModel(rows: 3, cols: 3);
            grid.PlaceBubble(0, 0, BubbleColor.Red);
            grid.PlaceBubble(2, 1, BubbleColor.Blue);

            var cells = grid.OccupiedCells();

            var expected = new List<(int Row, int Col)> { (0, 0), (2, 1) };
            CollectionAssert.AreEquivalent(expected, cells);
        }
    }
}
