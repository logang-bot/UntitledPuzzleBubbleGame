using System.Collections.Generic;
using Game.Grid;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class BubbleLandingResolverTests
    {
        private const int Cols = 5;
        private const float CellWidth = 1f;
        private static readonly Vector2 NoOffset = Vector2.zero;

        [Test]
        public void ResolveLandingCell_StruckCellGiven_PicksNearestUnoccupiedNeighborToContactPoint()
        {
            var grid = new GridModel(rows: 5, cols: Cols, cellWidth: 1f);
            grid.PlaceBubble(2, 2, BubbleColor.Red); // struck cell, occupied
            // Neighbors of (2,2) (even row): (2,1), (2,3), (1,1), (1,2), (3,1), (3,2) - all unoccupied.
            var contactPoint = grid.GetWorldPosition(1, 2); // closest to neighbor (1,2)

            var landing = BubbleLandingResolver.ResolveLandingCell((grid, NoOffset), contactPoint, (2, 2), CellWidth);

            Assert.That(landing, Is.EqualTo(((int Row, int Col)?)(1, 2)));
        }

        [Test]
        public void ResolveLandingCell_PreferredNeighborOccupied_FallsBackToNextNearest()
        {
            var grid = new GridModel(rows: 5, cols: Cols, cellWidth: 1f);
            grid.PlaceBubble(2, 2, BubbleColor.Red); // struck cell
            grid.PlaceBubble(1, 2, BubbleColor.Green); // otherwise-nearest neighbor, but occupied
            var contactPoint = grid.GetWorldPosition(1, 2);

            var landing = BubbleLandingResolver.ResolveLandingCell((grid, NoOffset), contactPoint, (2, 2), CellWidth);

            Assert.That(landing, Is.Not.EqualTo(((int Row, int Col)?)(1, 2)));
            Assert.That(landing, Is.Not.Null);
            Assert.IsFalse(grid.IsOccupied(landing.Value.Row, landing.Value.Col));
        }

        [Test]
        public void ResolveLandingCell_NoStruckCell_PicksNearestUnoccupiedRowZeroCellByX()
        {
            var grid = new GridModel(rows: 5, cols: Cols, cellWidth: 1f);
            var contactPoint = grid.GetWorldPosition(0, 3);

            var landing = BubbleLandingResolver.ResolveLandingCell((grid, NoOffset), contactPoint, null, CellWidth);

            Assert.That(landing, Is.EqualTo(((int Row, int Col)?)(0, 3)));
        }

        [Test]
        public void ResolveLandingCell_NoStruckCellAfterPushes_PicksCellInTheEffectiveCeilingRowNotRowZero()
        {
            // Rows behind the advanced wall (0 and 1 here) are permanently
            // vacated - row 0 is no longer where an unobstructed shot should land.
            var grid = new GridModel(rows: 6, cols: Cols, cellWidth: 1f);
            grid.PushRowsDown(out _);
            grid.PushRowsDown(out _); // RowsPushed = 2
            var contactPoint = grid.GetWorldPosition(2, 3);

            var landing = BubbleLandingResolver.ResolveLandingCell((grid, NoOffset), contactPoint, null, CellWidth);

            Assert.That(landing, Is.EqualTo(((int Row, int Col)?)(2, 3)));
        }

        [Test]
        public void ResolveLandingCell_NearestNeighborIsBehindTheAdvancedWall_SkipsItForOneInFrontOfIt()
        {
            var grid = new GridModel(rows: 6, cols: Cols, cellWidth: 1f);
            grid.PushRowsDown(out _); // RowsPushed = 1 - row 0 is now behind the wall
            grid.PlaceBubble(1, 2, BubbleColor.Red); // struck cell, touching the advanced wall
            // Block the same-row neighbors so the (otherwise tied-distance) search
            // would reach row 0's neighbors first if they weren't excluded.
            grid.PlaceBubble(1, 1, BubbleColor.Blue);
            grid.PlaceBubble(1, 3, BubbleColor.Blue);
            var contactPoint = grid.GetWorldPosition(1, 2);

            var landing = BubbleLandingResolver.ResolveLandingCell((grid, NoOffset), contactPoint, (1, 2), CellWidth);

            Assert.That(landing, Is.Not.Null);
            Assert.That(landing.Value.Row, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void ResolveLandingCell_NoRoomWithinSearchRadius_ReturnsNull()
        {
            var grid = new GridModel(rows: 5, cols: Cols, cellWidth: 1f);
            grid.PlaceBubble(2, 2, BubbleColor.Red); // struck cell
            // The search now also looks at neighbors of struckCell's own occupied
            // neighbors (anything within the search radius), so proving "no room"
            // requires occupying the full 2-ring neighborhood, not just the 1-ring.
            var toOccupy = new HashSet<(int Row, int Col)> { (2, 2) };
            foreach (var neighbor in grid.GetNeighbors(2, 2))
            {
                toOccupy.Add(neighbor);
                foreach (var secondRing in grid.GetNeighbors(neighbor.Row, neighbor.Col))
                    toOccupy.Add(secondRing);
            }
            foreach (var cell in toOccupy)
                grid.PlaceBubble(cell.Row, cell.Col, BubbleColor.Blue);
            var contactPoint = grid.GetWorldPosition(2, 2);

            var landing = BubbleLandingResolver.ResolveLandingCell((grid, NoOffset), contactPoint, (2, 2), CellWidth);

            Assert.That(landing, Is.Null);
        }

        [Test]
        public void ResolveLandingCell_PocketOnlyAdjacentToOtherNearbyOccupiedCell_IsFound()
        {
            var grid = new GridModel(rows: 5, cols: Cols, cellWidth: 1f);
            grid.PlaceBubble(2, 2, BubbleColor.Red); // struck cell (earliest contact)
            grid.PlaceBubble(2, 3, BubbleColor.Blue); // second bubble the ball is also touching near contact
            // (1,3) is a neighbor of (2,3) but not of (2,2) - only reachable once
            // neighbors of other nearby occupied cells are considered too.
            var contactPoint = grid.GetWorldPosition(1, 3);

            var landing = BubbleLandingResolver.ResolveLandingCell((grid, NoOffset), contactPoint, (2, 2), CellWidth);

            Assert.That(landing, Is.EqualTo(((int Row, int Col)?)(1, 3)));
        }

        [Test]
        public void ResolveLandingCell_OtherOccupiedCellBeyondSearchRadius_IsIgnored()
        {
            var grid = new GridModel(rows: 5, cols: 6, cellWidth: 1f);
            // Seal off every cell within the search radius of the struck corner
            // cell (itself plus its neighbors plus their neighbors, since those
            // are within the radius too), so the only way a candidate can appear
            // is via the deliberately far-away bubble below - proving it's
            // correctly excluded, not "wins because nothing closer exists."
            var sealedCells = new (int Row, int Col)[] { (0, 0), (0, 1), (1, 0), (0, 2), (1, 1), (2, 0), (2, 1) };
            foreach (var cell in sealedCells)
                grid.PlaceBubble(cell.Row, cell.Col, BubbleColor.Red);
            grid.PlaceBubble(0, 4, BubbleColor.Blue); // far outside the search radius
            var contactPoint = grid.GetWorldPosition(0, 0);

            var landing = BubbleLandingResolver.ResolveLandingCell((grid, NoOffset), contactPoint, (0, 0), CellWidth);

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

            var landing = BubbleLandingResolver.ResolveLandingCell((grid, boardOrigin), contactPoint, (2, 2), CellWidth);

            Assert.That(landing, Is.EqualTo(((int Row, int Col)?)(1, 2)));
        }
    }
}
