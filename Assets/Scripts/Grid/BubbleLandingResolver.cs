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
        public static (int Row, int Col)? ResolveLandingCell(
            (GridModel Grid, Vector2 Origin) board, Vector2 contactPoint, (int Row, int Col)? struckCell)
        {
            var candidates = struckCell == null
                ? UnoccupiedRowZeroCells(board.Grid)
                : UnoccupiedNeighbors(board.Grid, struckCell.Value);
            return NearestTo(candidates, board, contactPoint);
        }

        private static IEnumerable<(int Row, int Col)> UnoccupiedNeighbors(GridModel grid, (int Row, int Col) cell)
        {
            return grid.GetNeighbors(cell.Row, cell.Col).Where(c => !grid.IsOccupied(c.Row, c.Col));
        }

        private static IEnumerable<(int Row, int Col)> UnoccupiedRowZeroCells(GridModel grid)
        {
            return Enumerable.Range(0, grid.Cols)
                .Select(col => (Row: 0, Col: col))
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
