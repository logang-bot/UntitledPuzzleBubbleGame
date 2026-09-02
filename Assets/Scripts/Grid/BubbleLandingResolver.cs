using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    /// Resolves which empty cell a fired bubble snaps into once its (already
    /// occupancy-truncated) path ends, per docs/features/core-gameplay/shooter-and-trajectory.md:
    /// the nearest empty pocket to the contact point, not the exact contact point itself.
    /// `board.Origin` converts GridModel's board-local cell positions to the same
    /// world space `contactPoint` already uses (GameBoard's transform is offset
    /// from world origin — see docs/features/core-gameplay/firing-and-snapping.md).
    /// </summary>
    public static class BubbleLandingResolver
    {
        // A hex lattice only has occupied-cell-to-occupied-cell distances of
        // 1.0x cellWidth (first ring) or ~1.73x (second ring, sqrt(3)) from any
        // given point on it, so 1.3x sits strictly between: wide enough to
        // catch a second bubble the flying bubble is also touching near the
        // contact point, narrow enough to exclude anything a full ring further out.
        private const float NearbyContactDistanceFactor = 1.3f;

        public static (int Row, int Col)? ResolveLandingCell(
            (GridModel Grid, Vector2 Origin) board, Vector2 contactPoint, (int Row, int Col)? struckCell, float cellWidth)
        {
            var candidates = struckCell == null
                ? UnoccupiedEffectiveCeilingRowCells(board.Grid)
                : UnoccupiedNeighborsOfNearbyCells(board, contactPoint, cellWidth);
            return NearestTo(candidates, board, contactPoint);
        }

        // Considers every occupied cell touching the contact point, not just the
        // single earliest-hit struckCell - a shot's true pocket is often bounded
        // by a different nearby bubble than the one it happened to reach first.
        private static IEnumerable<(int Row, int Col)> UnoccupiedNeighborsOfNearbyCells(
            (GridModel Grid, Vector2 Origin) board, Vector2 contactPoint, float cellWidth)
        {
            return NearbyOccupiedCells(board, contactPoint, cellWidth)
                .SelectMany(cell => UnoccupiedNeighbors(board.Grid, cell))
                .Distinct();
        }

        private static IEnumerable<(int Row, int Col)> NearbyOccupiedCells(
            (GridModel Grid, Vector2 Origin) board, Vector2 contactPoint, float cellWidth)
        {
            var maxDistance = cellWidth * NearbyContactDistanceFactor;
            return board.Grid.OccupiedCells()
                .Where(cell => Vector2.Distance(board.Grid.GetWorldPosition(cell.Row, cell.Col) + board.Origin, contactPoint) <= maxDistance);
        }

        // Excludes anything behind the advanced wall (row < RowsPushed): those
        // rows are permanently vacated (see GridModel.RowsPushed) and could
        // otherwise still be picked as "nearest" by raw distance alone, even
        // though nothing can physically occupy them anymore.
        private static IEnumerable<(int Row, int Col)> UnoccupiedNeighbors(GridModel grid, (int Row, int Col) cell)
        {
            return grid.GetNeighbors(cell.Row, cell.Col)
                .Where(c => !grid.IsOccupied(c.Row, c.Col) && c.Row >= grid.RowsPushed);
        }

        private static IEnumerable<(int Row, int Col)> UnoccupiedEffectiveCeilingRowCells(GridModel grid)
        {
            var row = grid.RowsPushed;
            return Enumerable.Range(0, grid.Cols)
                .Select(col => (Row: row, Col: col))
                .Where(c => !grid.IsOccupied(c.Row, c.Col));
        }

        private static (int Row, int Col)? NearestTo(
            IEnumerable<(int Row, int Col)> candidates, (GridModel Grid, Vector2 Origin) board, Vector2 contactPoint)
        {
            var ordered = candidates
                .OrderBy(c => Vector2.Distance(board.Grid.GetWorldPosition(c.Row, c.Col) + board.Origin, contactPoint))
                .ToList();
            return ordered.Count == 0 ? null : ordered[0];
        }
    }
}
