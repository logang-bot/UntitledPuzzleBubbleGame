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
        public void FindFloatingCells_AfterPush_TreatsTheAdvancedWallRowAsTheCeiling()
        {
            // Row 0 legitimately stays empty after a ceiling-descent push (see
            // shot-timer-and-ceiling-descent.md) - the wall now touches row
            // RowsPushed, so that row anchors connectivity instead.
            var grid = new GridModel(rows: 5, cols: 5);
            grid.PlaceBubble(0, 2, BubbleColor.Red);
            grid.PlaceBubble(1, 2, BubbleColor.Red);
            grid.PushRowsDown(out _); // bubbles now at rows 1-2, RowsPushed = 1

            var floating = MatchResolver.FindFloatingCells(grid);

            CollectionAssert.IsEmpty(floating);
        }

        [Test]
        public void FindFloatingCells_AfterPushWithIslandBelowWallRow_ReturnsOnlyTheIsland()
        {
            var grid = new GridModel(rows: 6, cols: 5);
            grid.PlaceBubble(0, 0, BubbleColor.Red);
            grid.PlaceBubble(0, 1, BubbleColor.Red);
            grid.PlaceBubble(3, 3, BubbleColor.Blue); // disconnected island, unrelated to the wall row
            grid.PlaceBubble(3, 4, BubbleColor.Blue);
            grid.PushRowsDown(out _); // wall row is now row 1, island at row 4

            var floating = MatchResolver.FindFloatingCells(grid);

            var expected = new List<(int Row, int Col)> { (4, 3), (4, 4) };
            CollectionAssert.AreEquivalent(expected, floating);
        }

        [Test]
        public void FindFloatingCells_WallRowFullyCleared_DropsClusterHangingBelow()
        {
            // Playtest repro: a chunk spanning the whole wall row pops, and the
            // bubbles stacked beneath it must fall rather than the next row
            // down becoming a stand-in ceiling.
            var grid = new GridModel(rows: 5, cols: 5);
            grid.PlaceBubble(0, 2, BubbleColor.Red);
            grid.PlaceBubble(1, 2, BubbleColor.Blue);
            grid.PlaceBubble(2, 2, BubbleColor.Blue);
            grid.ClearCell(0, 2);

            var floating = MatchResolver.FindFloatingCells(grid);

            var expected = new List<(int Row, int Col)> { (1, 2), (2, 2) };
            CollectionAssert.AreEquivalent(expected, floating);
        }

        [Test]
        public void FindFloatingCells_WallRowFullyClearedAfterPush_DropsClusterHangingBelow()
        {
            var grid = new GridModel(rows: 6, cols: 5);
            grid.PlaceBubble(0, 2, BubbleColor.Red);
            grid.PlaceBubble(1, 2, BubbleColor.Blue);
            grid.PlaceBubble(2, 2, BubbleColor.Blue);
            grid.PushRowsDown(out _); // wall row is now row 1
            grid.ClearCell(1, 2);

            var floating = MatchResolver.FindFloatingCells(grid);

            var expected = new List<(int Row, int Col)> { (2, 2), (3, 2) };
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
