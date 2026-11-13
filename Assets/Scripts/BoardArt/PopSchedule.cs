using System;
using System.Collections.Generic;

namespace Game.BoardArt
{
    /// <summary>
    /// When each bubble of a popped cluster bursts: 0.12 s after landing, then
    /// 0.035 s per hex step outward from the cluster's first cell (breadth
    /// first through the cluster). Cells the walk never reaches burst first.
    /// </summary>
    public static class PopSchedule
    {
        public const float StartDelaySeconds = 0.12f;
        public const float StepSeconds = 0.035f;

        public static Dictionary<(int Row, int Col), float> Delays(IReadOnlyCollection<(int Row, int Col)> cells, Func<(int Row, int Col), IEnumerable<(int Row, int Col)>> neighbors)
        {
            var steps = StepsFromFirst(cells, neighbors);
            var delays = new Dictionary<(int Row, int Col), float>();
            foreach (var cell in cells)
                delays[cell] = StartDelaySeconds + (steps.TryGetValue(cell, out var step) ? step : 0) * StepSeconds;
            return delays;
        }

        private static Dictionary<(int Row, int Col), int> StepsFromFirst(IReadOnlyCollection<(int Row, int Col)> cells, Func<(int Row, int Col), IEnumerable<(int Row, int Col)>> neighbors)
        {
            var walk = new ClusterWalk(cells, neighbors);
            foreach (var first in cells) { walk.Start(first); break; }
            while (walk.HasNext) walk.VisitNext();
            return walk.Steps;
        }

        private class ClusterWalk
        {
            public readonly Dictionary<(int Row, int Col), int> Steps = new();
            public bool HasNext => _queue.Count > 0;

            private readonly HashSet<(int Row, int Col)> _cluster;
            private readonly Func<(int Row, int Col), IEnumerable<(int Row, int Col)>> _neighbors;
            private readonly Queue<(int Row, int Col)> _queue = new();

            public ClusterWalk(IEnumerable<(int Row, int Col)> cells, Func<(int Row, int Col), IEnumerable<(int Row, int Col)>> neighbors)
            {
                _cluster = new HashSet<(int Row, int Col)>(cells);
                _neighbors = neighbors;
            }

            public void Start((int Row, int Col) first)
            {
                Steps[first] = 0;
                _queue.Enqueue(first);
            }

            public void VisitNext()
            {
                var cell = _queue.Dequeue();
                foreach (var next in _neighbors(cell))
                    if (_cluster.Contains(next) && !Steps.ContainsKey(next)) Reach(next, Steps[cell] + 1);
            }

            private void Reach((int Row, int Col) cell, int step)
            {
                Steps[cell] = step;
                _queue.Enqueue(cell);
            }
        }
    }
}
