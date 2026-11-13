using System.Collections.Generic;
using UnityEngine;

namespace Game.Shooter
{
    /// <summary>
    /// Lays the aim dots (aim_dots) along a traced polyline: from Start past
    /// the first point, every Gap, shifted forward by Scroll (wrapped into
    /// one gap), up to the path's end. Each dot fades from alpha 1 at the
    /// start to 0.35 at the end.
    /// </summary>
    public static class AimDotPlacer
    {
        public const float EndAlpha = 0.35f;

        public static void Place(IReadOnlyList<Vector2> path, AimDotSpacing spacing, List<(Vector2 Position, float Alpha)> dots)
        {
            dots.Clear();
            var length = PathLength(path);
            var span = Mathf.Max(length - spacing.Start, 0.0001f);
            for (var along = spacing.Start + Mathf.Repeat(spacing.Scroll, spacing.Gap); along <= length; along += spacing.Gap)
                dots.Add((PointAt(path, along), Mathf.Lerp(1f, EndAlpha, (along - spacing.Start) / span)));
        }

        public static float PathLength(IReadOnlyList<Vector2> path)
        {
            var length = 0f;
            for (var i = 1; i < path.Count; i++) length += Vector2.Distance(path[i - 1], path[i]);
            return length;
        }

        public static Vector2 PointAt(IReadOnlyList<Vector2> path, float distance)
        {
            for (var i = 1; i < path.Count; i++)
            {
                var segment = Vector2.Distance(path[i - 1], path[i]);
                if (distance <= segment) return Vector2.Lerp(path[i - 1], path[i], segment > 0f ? distance / segment : 0f);
                distance -= segment;
            }
            return path[path.Count - 1];
        }
    }
}
