using System.Collections.Generic;
using System.Linq;

namespace Game.Grid
{
    /// <summary>
    /// Given a newly-placed bubble's cell, flood-fills same-color neighbors to
    /// decide what pops, and (separately) finds bubbles left disconnected from
    /// the ceiling. See docs/features/core-gameplay/matching-and-popping.md.
    /// </summary>
    public static class MatchResolver
    {
        private const int MinMatchSize = 3;

        public static HashSet<(int Row, int Col)> FindMatchGroup(GridModel grid, (int Row, int Col) placedCell)
        {
            var color = grid.GetColor(placedCell.Row, placedCell.Col);
            var group = FloodFill.Run(grid, new[] { placedCell }, cell => SameColor(grid, cell, color));
            return group.Count >= MinMatchSize ? group : new HashSet<(int Row, int Col)>();
        }

        public static HashSet<(int Row, int Col)> FindFloatingCells(GridModel grid)
        {
            var ceilingCells = grid.OccupiedCells().Where(cell => cell.Row == 0);
            var reachable = FloodFill.Run(grid, ceilingCells, cell => grid.IsOccupied(cell.Row, cell.Col));
            return grid.OccupiedCells().Where(cell => !reachable.Contains(cell)).ToHashSet();
        }

        private static bool SameColor(GridModel grid, (int Row, int Col) cell, BubbleColor color)
        {
            return grid.IsOccupied(cell.Row, cell.Col) && grid.GetColor(cell.Row, cell.Col) == color;
        }
    }
}
