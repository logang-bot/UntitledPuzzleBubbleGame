using System.Linq;
using Game.Grid;
using NUnit.Framework;

namespace Game.Tests
{
    public class GridModelPushRowsDownTests
    {
        [Test]
        public void PushRowsDown_OccupiedRow_ShiftsContentsDownOneRow()
        {
            var grid = new GridModel(rows: 3, cols: 3);
            grid.PlaceBubble(0, 1, BubbleColor.Red);

            grid.PushRowsDown(out _);

            Assert.IsFalse(grid.IsOccupied(0, 1));
            Assert.IsTrue(grid.IsOccupied(1, 1));
            Assert.AreEqual(BubbleColor.Red, grid.GetColor(1, 1));
        }

        [Test]
        public void PushRowsDown_MultipleRows_PreservesRelativeOrderAndColors()
        {
            var grid = new GridModel(rows: 4, cols: 1);
            grid.PlaceBubble(0, 0, BubbleColor.Red);
            grid.PlaceBubble(1, 0, BubbleColor.Blue);

            grid.PushRowsDown(out _);

            Assert.AreEqual(BubbleColor.Red, grid.GetColor(1, 0));
            Assert.AreEqual(BubbleColor.Blue, grid.GetColor(2, 0));
        }

        [Test]
        public void PushRowsDown_EmptyGrid_LeavesGridEmpty()
        {
            var grid = new GridModel(rows: 3, cols: 3);

            grid.PushRowsDown(out var wasLastRowOccupied);

            CollectionAssert.IsEmpty(grid.OccupiedCells());
            Assert.IsFalse(wasLastRowOccupied);
        }

        [Test]
        public void PushRowsDown_LastRowOccupied_ReportsWasLastRowOccupiedTrue()
        {
            var grid = new GridModel(rows: 3, cols: 3);
            grid.PlaceBubble(2, 0, BubbleColor.Red);

            grid.PushRowsDown(out var wasLastRowOccupied);

            Assert.IsTrue(wasLastRowOccupied);
        }

        [Test]
        public void PushRowsDown_LastRowEmpty_ReportsWasLastRowOccupiedFalse()
        {
            var grid = new GridModel(rows: 3, cols: 3);
            grid.PlaceBubble(0, 0, BubbleColor.Red);

            grid.PushRowsDown(out var wasLastRowOccupied);

            Assert.IsFalse(wasLastRowOccupied);
        }

        [Test]
        public void PushRowsDown_LastRowContentsAreDiscarded()
        {
            var grid = new GridModel(rows: 3, cols: 3);
            grid.PlaceBubble(2, 0, BubbleColor.Red);
            grid.PlaceBubble(2, 1, BubbleColor.Blue);

            grid.PushRowsDown(out _);

            Assert.AreEqual(0, grid.OccupiedCells().Count(cell => cell.Row == 2));
        }

        [Test]
        public void PushRowsDown_Row0AfterPush_IsAlwaysEmpty()
        {
            var grid = new GridModel(rows: 3, cols: 3);
            grid.PlaceBubble(0, 0, BubbleColor.Red);

            grid.PushRowsDown(out _);

            Assert.AreEqual(0, grid.OccupiedCells().Count(cell => cell.Row == 0));
        }
    }
}
