using Game.Grid;
using NUnit.Framework;

namespace Game.Tests
{
    public class GridModelOccupancyTests
    {
        [Test]
        public void NewGrid_CellIsNotOccupied()
        {
            var grid = new GridModel(rows: 5, cols: 5);

            Assert.IsFalse(grid.IsOccupied(0, 0));
        }

        [Test]
        public void PlaceBubble_MakesCellOccupied()
        {
            var grid = new GridModel(rows: 5, cols: 5);

            grid.PlaceBubble(0, 0, BubbleColor.Red);

            Assert.IsTrue(grid.IsOccupied(0, 0));
        }

        [Test]
        public void PlaceBubble_SetsColor()
        {
            var grid = new GridModel(rows: 5, cols: 5);

            grid.PlaceBubble(0, 0, BubbleColor.Red);

            Assert.AreEqual(BubbleColor.Red, grid.GetColor(0, 0));
        }

        [Test]
        public void ClearCell_MakesCellUnoccupied()
        {
            var grid = new GridModel(rows: 5, cols: 5);
            grid.PlaceBubble(0, 0, BubbleColor.Red);

            grid.ClearCell(0, 0);

            Assert.IsFalse(grid.IsOccupied(0, 0));
        }
    }
}
