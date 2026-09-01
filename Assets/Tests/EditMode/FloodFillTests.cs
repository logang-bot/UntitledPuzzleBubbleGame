using System.Collections.Generic;
using Game.Grid;
using NUnit.Framework;

namespace Game.Tests
{
    public class FloodFillTests
    {
        [Test]
        public void Run_SingleSeedNoMatchingNeighbors_ReturnsOnlySeed()
        {
            var grid = new GridModel(rows: 5, cols: 5);
            grid.PlaceBubble(2, 2, BubbleColor.Red);

            var result = FloodFill.Run(grid, new[] { (2, 2) }, cell => grid.IsOccupied(cell.Row, cell.Col));

            CollectionAssert.AreEquivalent(new[] { (2, 2) }, result);
        }

        [Test]
        public void Run_SeedFailsPredicate_ReturnsEmptySet()
        {
            var grid = new GridModel(rows: 5, cols: 5);

            var result = FloodFill.Run(grid, new[] { (2, 2) }, cell => grid.IsOccupied(cell.Row, cell.Col));

            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void Run_ConnectedChainAllSatisfyPredicate_ReturnsWholeChain()
        {
            var grid = new GridModel(rows: 5, cols: 5);
            grid.PlaceBubble(2, 2, BubbleColor.Red);
            grid.PlaceBubble(2, 3, BubbleColor.Red);
            grid.PlaceBubble(1, 2, BubbleColor.Red);

            var result = FloodFill.Run(grid, new[] { (2, 2) }, cell => grid.IsOccupied(cell.Row, cell.Col));

            var expected = new List<(int Row, int Col)> { (2, 2), (2, 3), (1, 2) };
            CollectionAssert.AreEquivalent(expected, result);
        }

        [Test]
        public void Run_NeighborFailsPredicate_StopsTraversalThroughIt()
        {
            var grid = new GridModel(rows: 5, cols: 5);
            grid.PlaceBubble(2, 2, BubbleColor.Red);
            grid.PlaceBubble(0, 2, BubbleColor.Red); // only reachable through the unoccupied (1,2)

            var result = FloodFill.Run(grid, new[] { (2, 2) }, cell => grid.IsOccupied(cell.Row, cell.Col));

            CollectionAssert.AreEquivalent(new[] { (2, 2) }, result);
        }

        [Test]
        public void Run_MultipleSeeds_MergeIntoOneReachableSet()
        {
            var grid = new GridModel(rows: 5, cols: 5);
            grid.PlaceBubble(0, 0, BubbleColor.Red);
            grid.PlaceBubble(4, 4, BubbleColor.Blue);

            var result = FloodFill.Run(grid, new[] { (0, 0), (4, 4) }, cell => grid.IsOccupied(cell.Row, cell.Col));

            CollectionAssert.AreEquivalent(new[] { (0, 0), (4, 4) }, result);
        }

        [Test]
        public void Run_EmptySeeds_ReturnsEmptySet()
        {
            var grid = new GridModel(rows: 5, cols: 5);
            grid.PlaceBubble(2, 2, BubbleColor.Red);

            var result = FloodFill.Run(grid, new (int Row, int Col)[0], cell => grid.IsOccupied(cell.Row, cell.Col));

            CollectionAssert.IsEmpty(result);
        }
    }
}
