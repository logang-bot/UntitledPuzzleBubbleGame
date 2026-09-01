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
            grid.PlaceBubble(2, 2, BubbleColor.Red); // world position (2, 2 * 0.8660254)
            var predictor = new TrajectoryPredictor(new BoardBounds(-10f, 10f, 10f));
            var rawPoints = predictor.Simulate(origin: new Vector2(2f, 0f), angleDegrees: 0f, maxBounces: 10);

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
            // same capping behaviour as TrajectoryPredictorTerminationTests, so the second segment's
            // endpoint is known precisely (-1, 3) without needing to simulate the full zigzag to the ceiling.
            var rawPoints = predictor.Simulate(origin: Vector2.zero, angleDegrees: 45f, maxBounces: 1);

            var result = OccupancyCollision.Truncate(rawPoints, (grid, Vector2.zero), CellWidth);

            Assert.That(result.StruckCell, Is.EqualTo(((int Row, int Col)?)(3, 0)));
            var struckWorldPos = grid.GetWorldPosition(3, 0);
            Assert.That(Vector2.Distance(result.Points[^1], struckWorldPos), Is.EqualTo(CellWidth).Within(Tolerance));
            Assert.That(result.Points.Count, Is.EqualTo(3));
            Assert.That(result.Points[1].x, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(result.Points[1].y, Is.EqualTo(1f).Within(Tolerance));
        }

        [Test]
        public void Truncate_TwoOccupiedCellsAlongPath_StopsAtTheNearerOne()
        {
            var grid = new GridModel(rows: 6, cols: 5, cellWidth: CellWidth);
            grid.PlaceBubble(2, 2, BubbleColor.Green); // nearer, both on even rows so both sit at x=2
            grid.PlaceBubble(4, 2, BubbleColor.Yellow); // farther
            var predictor = new TrajectoryPredictor(new BoardBounds(-10f, 10f, 10f));
            var rawPoints = predictor.Simulate(origin: new Vector2(2f, 0f), angleDegrees: 0f, maxBounces: 10);

            var result = OccupancyCollision.Truncate(rawPoints, (grid, Vector2.zero), CellWidth);

            Assert.That(result.StruckCell, Is.EqualTo(((int Row, int Col)?)(2, 2)));
            var fartherWorldPos = grid.GetWorldPosition(4, 2);
            Assert.That(result.Points[^1].y, Is.LessThan(fartherWorldPos.y));
        }
    }
}
