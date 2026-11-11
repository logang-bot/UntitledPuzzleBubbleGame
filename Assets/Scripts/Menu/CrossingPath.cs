using UnityEngine;

namespace Game.Menu
{
    /// <summary>Which edge a crossing enters from, and the y band horizontal crossings stay in.</summary>
    public struct CrossingRoute
    {
        public static readonly Vector2 FullHeight = new Vector2(40f, 740f);

        public int Edge;
        public Vector2 Band;

        public CrossingRoute(int edge, Vector2 band)
        {
            Edge = edge;
            Band = band;
        }
    }

    /// <summary>
    /// A straight line across the 360×780 stage, from just off one edge to
    /// just off the opposite edge. Points are native pt, y down. Horizontal
    /// crossings start and end inside the route's y band; vertical ones
    /// cross anywhere between x 30 and 330.
    /// </summary>
    public struct CrossingPath
    {
        public const int FromLeft = 0;
        public const int FromRight = 1;
        public const int FromTop = 2;
        public const int FromBottom = 3;

        public Vector2 Start;
        public Vector2 End;

        public bool MovesRight => End.x > Start.x;
        public float Length => Vector2.Distance(Start, End);

        private const float Width = 360f;
        private const float Height = 780f;
        private const float VerticalMinX = 30f;
        private const float VerticalSpanX = 300f;

        public Vector2 At(float progress) => Vector2.LerpUnclamped(Start, End, progress);

        public static CrossingPath Pick(System.Random random, Vector2 halfSize, CrossingRoute route)
        {
            var a = (float)random.NextDouble();
            var b = (float)random.NextDouble();
            var bandY = new Vector2(Mathf.Lerp(route.Band.x, route.Band.y, a), Mathf.Lerp(route.Band.x, route.Band.y, b));
            var columnX = new Vector2(VerticalMinX + a * VerticalSpanX, VerticalMinX + b * VerticalSpanX);
            return route.Edge switch
            {
                FromLeft => Between(new Vector2(-halfSize.x, bandY.x), new Vector2(Width + halfSize.x, bandY.y)),
                FromRight => Between(new Vector2(Width + halfSize.x, bandY.x), new Vector2(-halfSize.x, bandY.y)),
                FromTop => Between(new Vector2(columnX.x, -halfSize.y), new Vector2(columnX.y, Height + halfSize.y)),
                _ => Between(new Vector2(columnX.x, Height + halfSize.y), new Vector2(columnX.y, -halfSize.y))
            };
        }

        public static int RandomEdge(System.Random random) => random.Next(4);

        public static int RandomHorizontalEdge(System.Random random) => random.Next(2);

        /// <summary>The same crossing flattened to its start height, for sprites that only travel sideways.</summary>
        public CrossingPath Leveled() => Between(Start, new Vector2(End.x, Start.y));

        private static CrossingPath Between(Vector2 start, Vector2 end) => new CrossingPath { Start = start, End = end };
    }
}
