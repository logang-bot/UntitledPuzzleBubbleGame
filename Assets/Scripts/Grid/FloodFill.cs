using System;
using System.Collections.Generic;

namespace Game.Grid
{
    /// <summary>
    /// Generic BFS over GridModel's hex adjacency, starting from any number of
    /// seed cells and expanding only through cells satisfying `include`. Used
    /// by MatchResolver for both same-color matching and ceiling-reachability
    /// checks — see docs/features/core-gameplay/matching-and-popping.md.
    /// </summary>
    public static class FloodFill
    {
        public static HashSet<(int Row, int Col)> Run(
            GridModel grid, IEnumerable<(int Row, int Col)> seeds, Func<(int Row, int Col), bool> include)
        {
            var visited = new HashSet<(int Row, int Col)>();
            var queue = new Queue<(int Row, int Col)>();

            void Visit((int Row, int Col) cell)
            {
                if (include(cell) && visited.Add(cell))
                    queue.Enqueue(cell);
            }

            foreach (var seed in seeds) Visit(seed);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var neighbor in grid.GetNeighbors(current.Row, current.Col)) Visit(neighbor);
            }
            return visited;
        }
    }
}
