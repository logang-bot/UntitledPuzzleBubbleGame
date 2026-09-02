using System.Collections.Generic;
using UnityEngine;

namespace Game.Shooter
{
    /// <summary>
    /// Adjusts an occupancy-truncated preview path purely for display: the
    /// truncated endpoint is the future bubble's center, exactly cellWidth
    /// from the struck bubble's actual center - correct for where a fired
    /// bubble lands, but still a full bubble radius short of the struck
    /// bubble's rendered surface, since a bare LineRenderer tip has no radius
    /// of its own. Moves the endpoint straight toward the struck bubble's own
    /// center (not along the incoming segment's direction, which only
    /// coincides with the center for a head-on shot - an angled/grazing hit
    /// would otherwise land short of, or past, the true surface) so the
    /// preview visibly touches the bubble at every approach angle. Never
    /// changes where a shot truncates or lands - see OccupancyCollision and
    /// BubbleLandingResolver.
    /// </summary>
    public static class PreviewPointsCalculator
    {
        public static List<Vector2> TrimToSurface(
            List<Vector2> truncatedPoints, Vector2? targetCenter, float cellWidth)
        {
            if (targetCenter == null || truncatedPoints.Count < 1) return truncatedPoints;
            var points = new List<Vector2>(truncatedPoints);
            var last = points.Count - 1;
            var toCenter = targetCenter.Value - points[last];
            var distance = toCenter.magnitude;
            var surfaceRadius = cellWidth * 0.5f;
            if (distance <= surfaceRadius) return points;
            points[last] += toCenter.normalized * (distance - surfaceRadius);
            return points;
        }
    }
}
