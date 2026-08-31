using System.Collections.Generic;
using Game.Grid;
using NUnit.Framework;

namespace Game.Tests
{
    public class GridModelNeighborsTests
    {
        [Test]
        public void GetNeighbors_EvenRowInteriorCell_ReturnsSixNeighbors()
        {
            var grid = new GridModel(rows: 5, cols: 5);

            var neighbors = grid.GetNeighbors(row: 2, col: 2);

            var expected = new List<(int Row, int Col)>
            {
                (2, 1), (2, 3),
                (1, 1), (1, 2),
                (3, 1), (3, 2),
            };
            CollectionAssert.AreEquivalent(expected, neighbors);
        }

        [Test]
        public void GetNeighbors_OddRowInteriorCell_ReturnsSixNeighbors()
        {
            var grid = new GridModel(rows: 5, cols: 5);

            var neighbors = grid.GetNeighbors(row: 1, col: 2);

            var expected = new List<(int Row, int Col)>
            {
                (1, 1), (1, 3),
                (0, 2), (0, 3),
                (2, 2), (2, 3),
            };
            CollectionAssert.AreEquivalent(expected, neighbors);
        }

        [Test]
        public void GetNeighbors_CornerCell_ClipsOutOfBoundsNeighbors()
        {
            var grid = new GridModel(rows: 5, cols: 5);

            var neighbors = grid.GetNeighbors(row: 0, col: 0);

            var expected = new List<(int Row, int Col)>
            {
                (0, 1),
                (1, 0),
            };
            CollectionAssert.AreEquivalent(expected, neighbors);
        }
    }
}
