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

        // Only the row actually touching the wall (RowsPushed - the wall's
        // advanced position, see GridModel.RowsPushed) anchors connectivity.
        // Seeding from "whatever row is topmost" instead was a bug: popping a
        // chunk that spanned the whole wall row turned the next row down into
        // a stand-in ceiling, leaving everything hanging from it floating.
        // See docs/features/core-gameplay/matching-and-popping.md.
        public static HashSet<(int Row, int Col)> FindFloatingCells(GridModel grid)
        {
            var occupied = grid.OccupiedCells().ToList();
            if (occupied.Count == 0) return new HashSet<(int Row, int Col)>();
            var ceilingCells = occupied.Where(cell => cell.Row == grid.RowsPushed);
            var reachable = FloodFill.Run(grid, ceilingCells, cell => grid.IsOccupied(cell.Row, cell.Col));
            return occupied.Where(cell => !reachable.Contains(cell)).ToHashSet();
        }

        private static bool SameColor(GridModel grid, (int Row, int Col) cell, BubbleColor color)
        {
            return grid.IsOccupied(cell.Row, cell.Col) && grid.GetColor(cell.Row, cell.Col) == color;
        }
    }
}
