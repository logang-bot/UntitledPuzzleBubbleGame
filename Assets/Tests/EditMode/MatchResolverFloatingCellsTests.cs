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
        public void FindFloatingCells_RowZeroEmpty_TreatsTopmostOccupiedRowAsTheCeiling()
        {
            // Row 0 legitimately stays empty after a ceiling-descent push (see
            // shot-timer-and-ceiling-descent.md) - nothing refills it anymore.
            // The topmost row that IS occupied has nothing physically between
            // it and the wall, so it must anchor connectivity just like row 0
            // normally would, or a pop anywhere would wrongly drop everything.
            var grid = new GridModel(rows: 5, cols: 5);
            grid.PlaceBubble(1, 2, BubbleColor.Red);
            grid.PlaceBubble(2, 2, BubbleColor.Red);

            var floating = MatchResolver.FindFloatingCells(grid);

            CollectionAssert.IsEmpty(floating);
        }

        [Test]
        public void FindFloatingCells_RowZeroEmptyWithIslandBelowTopmostRow_ReturnsOnlyTheIsland()
        {
            var grid = new GridModel(rows: 6, cols: 5);
            grid.PlaceBubble(1, 0, BubbleColor.Red); // topmost occupied row - the effective ceiling
            grid.PlaceBubble(1, 1, BubbleColor.Red);
            grid.PlaceBubble(4, 3, BubbleColor.Blue); // disconnected island, unrelated to the row above
            grid.PlaceBubble(4, 4, BubbleColor.Blue);

            var floating = MatchResolver.FindFloatingCells(grid);

            var expected = new List<(int Row, int Col)> { (4, 3), (4, 4) };
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
