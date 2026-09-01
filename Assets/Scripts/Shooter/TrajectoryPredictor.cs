using System.Collections.Generic;
using UnityEngine;

namespace Game.Shooter
{
    /// <summary>
    /// Kinematic (non-physics) trajectory simulation, so the preview line and
    /// a later fired bubble's path can never disagree (see
    /// docs/features/core-gameplay/shooter-and-trajectory.md).
    /// </summary>
    public class TrajectoryPredictor
    {
        private readonly BoardBounds _bounds;

        public TrajectoryPredictor(BoardBounds bounds)
        {
            _bounds = bounds;
        }

        public List<Vector2> Simulate(Vector2 origin, float angleDegrees, int maxBounces)
        {
            var points = new List<Vector2> { origin };
            var position = origin;
            var direction = DirectionFromAngle(angleDegrees);
            for (var bounces = 0; bounces <= maxBounces; bounces++)
            {
                var (hitPoint, hitWall) = NextSegment(position, direction);
                points.Add(hitPoint);
                if (!hitWall) break;
                position = hitPoint;
                direction = Reflect(direction);
            }
            return points;
        }

        private (Vector2 HitPoint, bool HitWall) NextSegment(Vector2 position, Vector2 direction)
        {
            var wallDistance = DistanceToWall(position, direction);
            var ceilingDistance = DistanceToCeiling(position, direction);
            var travelled = Mathf.Min(wallDistance, ceilingDistance);
            return (position + direction * travelled, wallDistance < ceilingDistance);
        }

        private float DistanceToWall(Vector2 position, Vector2 direction)
        {
            if (Mathf.Approximately(direction.x, 0f)) return float.PositiveInfinity;
            var wallX = direction.x > 0f ? _bounds.RightWallX : _bounds.LeftWallX;
            return (wallX - position.x) / direction.x;
        }

        private float DistanceToCeiling(Vector2 position, Vector2 direction)
        {
            return (_bounds.CeilingY - position.y) / direction.y;
        }

        private static Vector2 Reflect(Vector2 direction)
        {
            return new Vector2(-direction.x, direction.y);
        }

        private static Vector2 DirectionFromAngle(float angleDegrees)
        {
            var radians = angleDegrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));
        }
    }
}
