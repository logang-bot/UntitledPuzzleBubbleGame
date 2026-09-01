using Game.Grid;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class BubbleLandingResolverTests
    {
        private const int Cols = 5;
        private static readonly Vector2 NoOffset = Vector2.zero;

        [Test]
        public void ResolveLandingCell_StruckCellGiven_PicksNearestUnoccupiedNeighborToContactPoint()
        {
            var grid = new GridModel(rows: 5, cols: Cols, cellWidth: 1f);
            grid.PlaceBubble(2, 2, BubbleColor.Red); // struck cell, occupied
            // Neighbors of (2,2) (even row): (2,1), (2,3), (1,1), (1,2), (3,1), (3,2) - all unoccupied.
            var contactPoint = grid.GetWorldPosition(1, 2); // closest to neighbor (1,2)

            var landing = BubbleLandingResolver.ResolveLandingCell((grid, NoOffset), contactPoint, (2, 2));

            Assert.That(landing, Is.EqualTo(((int Row, int Col)?)(1, 2)));
        }

        [Test]
        public void ResolveLandingCell_PreferredNeighborOccupied_FallsBackToNextNearest()
        {
            var grid = new GridModel(rows: 5, cols: Cols, cellWidth: 1f);
            grid.PlaceBubble(2, 2, BubbleColor.Red); // struck cell
            grid.PlaceBubble(1, 2, BubbleColor.Green); // otherwise-nearest neighbor, but occupied
            var contactPoint = grid.GetWorldPosition(1, 2);

            var landing = BubbleLandingResolver.ResolveLandingCell((grid, NoOffset), contactPoint, (2, 2));

            Assert.That(landing, Is.Not.EqualTo(((int Row, int Col)?)(1, 2)));
            Assert.That(landing, Is.Not.Null);
            Assert.IsFalse(grid.IsOccupied(landing.Value.Row, landing.Value.Col));
        }

        [Test]
        public void ResolveLandingCell_NoStruckCell_PicksNearestUnoccupiedRowZeroCellByX()
        {
            var grid = new GridModel(rows: 5, cols: Cols, cellWidth: 1f);
            var contactPoint = grid.GetWorldPosition(0, 3);

            var landing = BubbleLandingResolver.ResolveLandingCell((grid, NoOffset), contactPoint, null);

            Assert.That(landing, Is.EqualTo(((int Row, int Col)?)(0, 3)));
        }

        [Test]
        public void ResolveLandingCell_AllNeighborsOccupied_ReturnsNull()
        {
            var grid = new GridModel(rows: 5, cols: Cols, cellWidth: 1f);
            grid.PlaceBubble(2, 2, BubbleColor.Red); // struck cell
            foreach (var neighbor in grid.GetNeighbors(2, 2))
                grid.PlaceBubble(neighbor.Row, neighbor.Col, BubbleColor.Blue);
            var contactPoint = grid.GetWorldPosition(2, 2);

            var landing = BubbleLandingResolver.ResolveLandingCell((grid, NoOffset), contactPoint, (2, 2));

            Assert.That(landing, Is.Null);
        }

        [Test]
        public void ResolveLandingCell_NonZeroBoardOrigin_ComparesWorldPositionsNotGridLocalPositions()
        {
            var grid = new GridModel(rows: 5, cols: Cols, cellWidth: 1f);
            grid.PlaceBubble(2, 2, BubbleColor.Red); // struck cell
            var boardOrigin = new Vector2(10f, -4f);
            // Contact point closest to neighbor (1,2) in WORLD space (grid-local position plus the offset).
            var contactPoint = grid.GetWorldPosition(1, 2) + boardOrigin;

            var landing = BubbleLandingResolver.ResolveLandingCell((grid, boardOrigin), contactPoint, (2, 2));

            Assert.That(landing, Is.EqualTo(((int Row, int Col)?)(1, 2)));
        }
    }
}
