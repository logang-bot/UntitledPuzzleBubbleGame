using Game.Grid;
using NUnit.Framework;

namespace Game.Tests
{
    public class GridModelIsEmptyTests
    {
        [Test]
        public void IsEmpty_NoCellsPlaced_ReturnsTrue()
        {
            var grid = new GridModel(rows: 3, cols: 3);

            Assert.IsTrue(grid.IsEmpty);
        }

        [Test]
        public void IsEmpty_OneCellPlaced_ReturnsFalse()
        {
            var grid = new GridModel(rows: 3, cols: 3);
            grid.PlaceBubble(0, 0, BubbleColor.Red);

            Assert.IsFalse(grid.IsEmpty);
        }

        [Test]
        public void IsEmpty_AfterClearingOnlyOccupiedCell_ReturnsTrue()
        {
            var grid = new GridModel(rows: 3, cols: 3);
            grid.PlaceBubble(0, 0, BubbleColor.Red);
            grid.ClearCell(0, 0);

            Assert.IsTrue(grid.IsEmpty);
        }
    }
}
