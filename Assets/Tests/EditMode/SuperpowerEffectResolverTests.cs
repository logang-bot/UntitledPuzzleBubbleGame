using System.Collections.Generic;
using Game.Grid;
using Game.Superpowers;
using NUnit.Framework;

namespace Game.Tests
{
    public class SuperpowerEffectResolverTests
    {
        [Test]
        public void ResolveBomb_RadiusOne_IncludesOccupiedNeighborExcludesFarCellAndCenter()
        {
            var grid = new GridModel(rows: 8, cols: 8);
            var center = (Row: 3, Col: 3);
            var neighbor = grid.GetNeighbors(center.Row, center.Col)[0];
            grid.PlaceBubble(neighbor.Row, neighbor.Col, BubbleColor.Red);
            grid.PlaceBubble(0, 0, BubbleColor.Green);

            var result = SuperpowerEffectResolver.ResolveBomb(grid, center, 1);

            CollectionAssert.Contains(result, neighbor);
            CollectionAssert.DoesNotContain(result, (0, 0));
            CollectionAssert.DoesNotContain(result, center);
        }

        [Test]
        public void ResolveRowClear_OccupiedRow_ReturnsAllOccupiedCellsInRow()
        {
            var grid = new GridModel(rows: 5, cols: 5);
            grid.PlaceBubble(3, 0, BubbleColor.Red);
            grid.PlaceBubble(3, 2, BubbleColor.Blue);
            grid.PlaceBubble(1, 0, BubbleColor.Green);

            var result = SuperpowerEffectResolver.ResolveRowClear(grid, 3);

            var expected = new List<(int Row, int Col)> { (3, 0), (3, 2) };
            CollectionAssert.AreEquivalent(expected, result);
        }

        [Test]
        public void ResolveRainbow_SingleAdjacentGroup_ReturnsThatGroup()
        {
            var grid = new GridModel(rows: 6, cols: 6);
            var landing = (Row: 2, Col: 2);
            var neighbor = grid.GetNeighbors(landing.Row, landing.Col)[0];
            grid.PlaceBubble(neighbor.Row, neighbor.Col, BubbleColor.Red);

            var result = SuperpowerEffectResolver.ResolveRainbow(grid, landing);

            CollectionAssert.AreEquivalent(new List<(int Row, int Col)> { neighbor }, result);
        }

        [Test]
        public void ResolveRainbow_NoOccupiedNeighbors_ReturnsEmptySet()
        {
            var grid = new GridModel(rows: 6, cols: 6);

            var result = SuperpowerEffectResolver.ResolveRainbow(grid, (2, 2));

            CollectionAssert.IsEmpty(result);
        }
    }
}
