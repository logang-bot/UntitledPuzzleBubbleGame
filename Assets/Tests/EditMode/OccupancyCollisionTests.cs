using Game.Grid;
using Game.Shooter;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class OccupancyCollisionTests
    {
        private const float CellWidth = 1f;
        private const float Tolerance = 0.0001f;

        [Test]
        public void Truncate_NoOccupiedCells_ReturnsOriginalPointsAndNullStruckCell()
        {
            var grid = new GridModel(rows: 5, cols: 5, cellWidth: CellWidth);
            var predictor = new TrajectoryPredictor(new BoardBounds(-10f, 10f, 5f));
            var rawPoints = predictor.Simulate(origin: Vector2.zero, angleDegrees: 0f, maxBounces: 10);

            var result = OccupancyCollision.Truncate(rawPoints, (grid, Vector2.zero), CellWidth);

            Assert.That(result.StruckCell, Is.Null);
            CollectionAssert.AreEqual(rawPoints, result.Points);
        }

        [Test]
        public void Truncate_StraightShotIntoOccupiedCell_StopsExactlyCellWidthAway()
        {
            var grid = new GridModel(rows: 5, cols: 5, cellWidth: CellWidth);
            grid.PlaceBubble(2, 2, BubbleColor.Red); // world position (2, -2 * 0.8660254), row 0 is the ceiling
            var predictor = new TrajectoryPredictor(new BoardBounds(-10f, 10f, 10f));
            // Fire from below the cell (smaller y) toward the ceiling (+Y), so the shot travels up into it.
            var rawPoints = predictor.Simulate(origin: new Vector2(2f, -5f), angleDegrees: 0f, maxBounces: 10);

            var result = OccupancyCollision.Truncate(rawPoints, (grid, Vector2.zero), CellWidth);

            Assert.That(result.StruckCell, Is.EqualTo(((int Row, int Col)?)(2, 2)));
            var struckWorldPos = grid.GetWorldPosition(2, 2);
            Assert.That(Vector2.Distance(result.Points[^1], struckWorldPos), Is.EqualTo(CellWidth).Within(Tolerance));
        }

        [Test]
        public void Truncate_NonZeroBoardOrigin_ComparesWorldPositionsNotGridLocalPositions()
        {
            var grid = new GridModel(rows: 5, cols: 5, cellWidth: CellWidth);
            grid.PlaceBubble(2, 2, BubbleColor.Red); // grid-local position (2, 2 * 0.8660254)
            var boardOrigin = new Vector2(5f, 3f);
            var predictor = new TrajectoryPredictor(new BoardBounds(-10f, 10f, 10f));
            // Fire from the cell's actual WORLD x (grid-local x=2 plus the board's world offset).
            var rawPoints = predictor.Simulate(origin: new Vector2(2f + boardOrigin.x, 0f), angleDegrees: 0f, maxBounces: 10);

            var result = OccupancyCollision.Truncate(rawPoints, (grid, boardOrigin), CellWidth);

            Assert.That(result.StruckCell, Is.EqualTo(((int Row, int Col)?)(2, 2)));
            var struckWorldPos = grid.GetWorldPosition(2, 2) + boardOrigin;
            Assert.That(Vector2.Distance(result.Points[^1], struckWorldPos), Is.EqualTo(CellWidth).Within(Tolerance));
        }

        [Test]
        public void Truncate_CollisionOnSecondSegmentAfterWallBounce_TruncatesOnThatSegment()
        {
            var grid = new GridModel(rows: 5, cols: 5, cellWidth: CellWidth);
            grid.PlaceBubble(3, 0, BubbleColor.Blue); // sits near the post-bounce segment, clear of the first
            var predictor = new TrajectoryPredictor(new BoardBounds(-1f, 1f, 100f));
            // maxBounces: 1 caps the raw path at exactly [origin, wallBounce, (would-be next wall hit)],
            // same capping behaviour as TrajectoryPredictorTerminationTests. Firing from y=-4.598076
            // (instead of 0) centers row 3's world position (-2.598076) on the second segment, in the
            // negative-y range where row 3 (below the row-0 ceiling) now lives, clear of the first segment.
            var rawPoints = predictor.Simulate(origin: new Vector2(0f, -4.598076f), angleDegrees: 45f, maxBounces: 1);

            var result = OccupancyCollision.Truncate(rawPoints, (grid, Vector2.zero), CellWidth);

            Assert.That(result.StruckCell, Is.EqualTo(((int Row, int Col)?)(3, 0)));
            var struckWorldPos = grid.GetWorldPosition(3, 0);
            Assert.That(Vector2.Distance(result.Points[^1], struckWorldPos), Is.EqualTo(CellWidth).Within(Tolerance));
            Assert.That(result.Points.Count, Is.EqualTo(3));
            Assert.That(result.Points[1].x, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(result.Points[1].y, Is.EqualTo(-3.598076f).Within(Tolerance));
        }

        [Test]
        public void Truncate_TwoOccupiedCellsAlongPath_StopsAtTheNearerOne()
        {
            var grid = new GridModel(rows: 6, cols: 5, cellWidth: CellWidth);
            grid.PlaceBubble(4, 2, BubbleColor.Yellow); // nearer to a shot fired from below, both on even rows so both sit at x=2
            grid.PlaceBubble(2, 2, BubbleColor.Green); // farther (closer to the row-0 ceiling)
            var predictor = new TrajectoryPredictor(new BoardBounds(-10f, 10f, 10f));
            // Fire from below both rows (toward the ceiling, +Y), so row 4 (more negative y) is reached first.
            var rawPoints = predictor.Simulate(origin: new Vector2(2f, -10f), angleDegrees: 0f, maxBounces: 10);

            var result = OccupancyCollision.Truncate(rawPoints, (grid, Vector2.zero), CellWidth);

            Assert.That(result.StruckCell, Is.EqualTo(((int Row, int Col)?)(4, 2)));
            var fartherWorldPos = grid.GetWorldPosition(2, 2);
            Assert.That(result.Points[^1].y, Is.LessThan(fartherWorldPos.y));
        }
    }
}
