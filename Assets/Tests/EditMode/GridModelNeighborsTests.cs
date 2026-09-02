using System.Collections.Generic;
using Game.Grid;
using NUnit.Framework;
using UnityEngine;

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

        [Test]
        public void GetNeighbors_AfterSeveralPushes_TouchingNeighborsStayExactlyOneCellWidthApart()
        {
            var grid = new GridModel(rows: 10, cols: 5, cellWidth: 1f);
            grid.PlaceBubble(5, 2, BubbleColor.Red);

            for (var pushes = 0; pushes <= 4; pushes++)
            {
                var row = 5 + pushes;
                foreach (var neighbor in grid.GetNeighbors(row, 2))
                {
                    var distance = Vector2.Distance(grid.GetWorldPosition(row, 2), grid.GetWorldPosition(neighbor.Row, neighbor.Col));
                    Assert.That(distance, Is.EqualTo(1f).Within(0.0001f),
                        $"after {pushes} push(es), neighbor ({neighbor.Row},{neighbor.Col}) isn't touching ({row},2)");
                }
                grid.PushRowsDown(out _);
            }
        }
    }
}
