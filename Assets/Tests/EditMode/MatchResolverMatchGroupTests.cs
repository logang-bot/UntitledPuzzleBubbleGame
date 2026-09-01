using System.Collections.Generic;
using Game.Grid;
using NUnit.Framework;

namespace Game.Tests
{
    public class MatchResolverMatchGroupTests
    {
        [Test]
        public void FindMatchGroup_GroupOfThreeSameColor_ReturnsAllThree()
        {
            var grid = new GridModel(rows: 5, cols: 5);
            grid.PlaceBubble(2, 2, BubbleColor.Red);
            grid.PlaceBubble(2, 3, BubbleColor.Red);
            grid.PlaceBubble(1, 2, BubbleColor.Red);

            var group = MatchResolver.FindMatchGroup(grid, (2, 2));

            var expected = new List<(int Row, int Col)> { (2, 2), (2, 3), (1, 2) };
            CollectionAssert.AreEquivalent(expected, group);
        }

        [Test]
        public void FindMatchGroup_GroupOfTwoSameColor_ReturnsEmpty()
        {
            var grid = new GridModel(rows: 5, cols: 5);
            grid.PlaceBubble(2, 2, BubbleColor.Red);
            grid.PlaceBubble(2, 3, BubbleColor.Red);

            var group = MatchResolver.FindMatchGroup(grid, (2, 2));

            CollectionAssert.IsEmpty(group);
        }

        [Test]
        public void FindMatchGroup_DifferentColorNeighborBreaksChain_ExcludesIt()
        {
            var grid = new GridModel(rows: 5, cols: 5);
            grid.PlaceBubble(2, 2, BubbleColor.Red);
            grid.PlaceBubble(2, 3, BubbleColor.Red);
            grid.PlaceBubble(1, 2, BubbleColor.Blue); // blocks the only path to (0,2)
            grid.PlaceBubble(0, 2, BubbleColor.Red); // would extend the group to 4 if reachable

            var group = MatchResolver.FindMatchGroup(grid, (2, 2));

            CollectionAssert.IsEmpty(group);
        }

        [Test]
        public void FindMatchGroup_LargerConnectedGroup_ReturnsAllOfThem()
        {
            var grid = new GridModel(rows: 5, cols: 5);
            grid.PlaceBubble(2, 2, BubbleColor.Green);
            grid.PlaceBubble(2, 3, BubbleColor.Green);
            grid.PlaceBubble(1, 2, BubbleColor.Green);
            grid.PlaceBubble(1, 3, BubbleColor.Green);

            var group = MatchResolver.FindMatchGroup(grid, (2, 2));

            var expected = new List<(int Row, int Col)> { (2, 2), (2, 3), (1, 2), (1, 3) };
            CollectionAssert.AreEquivalent(expected, group);
        }

        [Test]
        public void FindMatchGroup_NoSameColorNeighbors_ReturnsEmpty()
        {
            var grid = new GridModel(rows: 5, cols: 5);
            grid.PlaceBubble(2, 2, BubbleColor.Red);
            grid.PlaceBubble(2, 3, BubbleColor.Blue);
            grid.PlaceBubble(1, 2, BubbleColor.Yellow);

            var group = MatchResolver.FindMatchGroup(grid, (2, 2));

            CollectionAssert.IsEmpty(group);
        }
    }
}
