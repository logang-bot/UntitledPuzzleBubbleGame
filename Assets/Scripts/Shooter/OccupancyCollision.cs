using System.Collections.Generic;
using Game.Grid;
using UnityEngine;

namespace Game.Shooter
{
    /// <summary>
    /// Truncates a raw (wall/ceiling-only) TrajectoryPredictor path at the first
    /// occupied cell it touches, so the preview line and the fired bubble both
    /// stop where a real shot would (see docs/features/core-gameplay/shooter-and-trajectory.md).
    /// TrajectoryPredictor itself stays occupancy-unaware to keep its existing tests intact.
    /// `board.Origin` converts GridModel's board-local cell positions to the same
    /// world space the trajectory points already use (GameBoard's transform is
    /// offset from world origin — see docs/features/core-gameplay/firing-and-snapping.md).
    /// </summary>
    public static class OccupancyCollision
    {
        public static (List<Vector2> Points, (int Row, int Col)? StruckCell) Truncate(
            List<Vector2> rawPoints, (GridModel Grid, Vector2 Origin) board, float cellWidth)
        {
            for (var i = 0; i < rawPoints.Count - 1; i++)
            {
                var hit = FirstContactOnSegment((rawPoints[i], rawPoints[i + 1]), board, cellWidth);
                if (hit == null) continue;
                var (contactPoint, struckCell) = hit.Value;
                return (TruncatedPath(rawPoints, i, contactPoint), struckCell);
            }
            return (rawPoints, null);
        }

        private static List<Vector2> TruncatedPath(List<Vector2> rawPoints, int segmentIndex, Vector2 contactPoint)
        {
            var points = rawPoints.GetRange(0, segmentIndex + 1);
            points.Add(contactPoint);
            return points;
        }

        private static (Vector2 ContactPoint, (int Row, int Col) StruckCell)? FirstContactOnSegment(
            (Vector2 Start, Vector2 End) segment, (GridModel Grid, Vector2 Origin) board, float cellWidth)
        {
            (Vector2 Point, (int Row, int Col) Cell, float T)? nearest = null;
            foreach (var cell in board.Grid.OccupiedCells())
            {
                var cellWorldPos = board.Grid.GetWorldPosition(cell.Row, cell.Col) + board.Origin;
                var contact = SegmentCircleContact(segment, cellWorldPos, cellWidth);
                if (contact == null) continue;
                if (nearest == null || contact.Value.T < nearest.Value.T)
                    nearest = (contact.Value.Point, cell, contact.Value.T);
            }
            return nearest == null ? null : (nearest.Value.Point, nearest.Value.Cell);
        }

        /// <summary>
        /// Smallest t in [0,1] along the segment where a point on it is exactly
        /// `radius` away from `circleCenter` (standard ray/circle intersection,
        /// solved as a quadratic in t) — the moment two touching bubbles' centers
        /// would be `radius` apart, not the segment's closest approach to the circle.
        /// </summary>
        private static (Vector2 Point, float T)? SegmentCircleContact(
            (Vector2 Start, Vector2 End) segment, Vector2 circleCenter, float radius)
        {
            var direction = segment.End - segment.Start;
            var toStart = segment.Start - circleCenter;
            var a = Vector2.Dot(direction, direction);
            var b = 2f * Vector2.Dot(toStart, direction);
            var c = Vector2.Dot(toStart, toStart) - radius * radius;
            var discriminant = b * b - 4f * a * c;
            if (discriminant < 0f) return null;
            var t = (-b - Mathf.Sqrt(discriminant)) / (2f * a);
            if (t < 0f || t > 1f) return null;
            return (segment.Start + direction * t, t);
        }
    }
}
