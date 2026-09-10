using System.Collections.Generic;
using Game.Grid;

namespace Game.Superpowers
{
    public static class HexRadius
    {
        public static HashSet<(int Row, int Col)> CellsWithinRadius(GridModel grid, (int Row, int Col) center, int radius)
        {
            var visited = new HashSet<(int Row, int Col)> { center };
            var frontier = new List<(int Row, int Col)> { center };
            for (var step = 0; step < radius; step++)
                frontier = ExpandFrontier(grid, frontier, visited);
            return visited;
        }

        private static List<(int Row, int Col)> ExpandFrontier(GridModel grid, List<(int Row, int Col)> frontier, HashSet<(int Row, int Col)> visited)
        {
            var next = new List<(int Row, int Col)>();
            foreach (var cell in frontier)
                foreach (var neighbor in grid.GetNeighbors(cell.Row, cell.Col))
                    if (visited.Add(neighbor))
                        next.Add(neighbor);
            return next;
        }
    }
}
