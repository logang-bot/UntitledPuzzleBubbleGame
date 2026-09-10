using System.Collections.Generic;
using Game.Grid;

namespace Game.Superpowers
{
    public static class SuperpowerEffectResolver
    {
        public static HashSet<(int Row, int Col)> ResolveBomb(GridModel grid, (int Row, int Col) landingCell, int radius)
        {
            var inRange = HexRadius.CellsWithinRadius(grid, landingCell, radius);
            var occupied = new HashSet<(int Row, int Col)>();
            foreach (var cell in inRange)
                if (cell != landingCell && grid.IsOccupied(cell.Row, cell.Col))
                    occupied.Add(cell);
            return occupied;
        }

        public static HashSet<(int Row, int Col)> ResolveRowClear(GridModel grid, int row)
        {
            var occupied = new HashSet<(int Row, int Col)>();
            for (var col = 0; col < grid.Cols; col++)
                if (grid.IsOccupied(row, col))
                    occupied.Add((row, col));
            return occupied;
        }

        public static HashSet<(int Row, int Col)> ResolveRainbow(GridModel grid, (int Row, int Col) landingCell)
        {
            var best = new HashSet<(int Row, int Col)>();
            foreach (var neighbor in OccupiedNeighbors(grid, landingCell))
            {
                var color = grid.GetColor(neighbor.Row, neighbor.Col);
                var group = FloodFill.Run(grid, new[] { neighbor }, c => grid.IsOccupied(c.Row, c.Col) && grid.GetColor(c.Row, c.Col) == color);
                if (group.Count > best.Count) best = group;
            }
            return best;
        }

        private static IEnumerable<(int Row, int Col)> OccupiedNeighbors(GridModel grid, (int Row, int Col) cell)
        {
            foreach (var neighbor in grid.GetNeighbors(cell.Row, cell.Col))
                if (grid.IsOccupied(neighbor.Row, neighbor.Col))
                    yield return neighbor;
        }
    }
}
