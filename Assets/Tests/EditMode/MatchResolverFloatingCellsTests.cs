using System.Collections.Generic;
using Game.Grid;
using NUnit.Framework;

namespace Game.Tests
{
    public class MatchResolverFloatingCellsTests
    {
        [Test]
        public void FindFloatingCells_AllCellsReachableFromCeiling_ReturnsEmpty()
        {
            var grid = new GridModel(rows: 5, cols: 5);
            grid.PlaceBubble(0, 2, BubbleColor.Red);
            grid.PlaceBubble(1, 2, BubbleColor.Blue);
            grid.PlaceBubble(2, 2, BubbleColor.Green);

            var floating = MatchResolver.FindFloatingCells(grid);

            CollectionAssert.IsEmpty(floating);
        }

        [Test]
        public void FindFloatingCells_IslandDisconnectedFromRowZero_ReturnsIsland()
        {
            var grid = new GridModel(rows: 5, cols: 5);
            grid.PlaceBubble(0, 0, BubbleColor.Red); // ceiling-connected, elsewhere on the board
            grid.PlaceBubble(3, 3, BubbleColor.Blue);
            grid.PlaceBubble(3, 4, BubbleColor.Blue); // island, no path back to row 0

            var floating = MatchResolver.FindFloatingCells(grid);

            var expected = new List<(int Row, int Col)> { (3, 3), (3, 4) };
            CollectionAssert.AreEquivalent(expected, floating);
        }

        [Test]
        public void FindFloatingCells_EmptyGrid_ReturnsEmpty()
        {
            var grid = new GridModel(rows: 5, cols: 5);

            var floating = MatchResolver.FindFloatingCells(grid);

            CollectionAssert.IsEmpty(floating);
        }

        [Test]
        public void FindFloatingCells_RowZeroFullyCleared_ReturnsAllRemainingOccupiedCells()
        {
            var grid = new GridModel(rows: 5, cols: 5);
            grid.PlaceBubble(1, 2, BubbleColor.Red);
            grid.PlaceBubble(2, 2, BubbleColor.Red);

            var floating = MatchResolver.FindFloatingCells(grid);

            var expected = new List<(int Row, int Col)> { (1, 2), (2, 2) };
            CollectionAssert.AreEquivalent(expected, floating);
        }

        [Test]
        public void FindFloatingCells_MultipleRowZeroSeeds_MergeReachabilityCorrectly()
        {
            var grid = new GridModel(rows: 5, cols: 5);
            grid.PlaceBubble(0, 0, BubbleColor.Red);
            grid.PlaceBubble(1, 0, BubbleColor.Red);
            grid.PlaceBubble(0, 4, BubbleColor.Blue);
            grid.PlaceBubble(1, 4, BubbleColor.Blue);

            var floating = MatchResolver.FindFloatingCells(grid);

            CollectionAssert.IsEmpty(floating);
        }
    }
}
