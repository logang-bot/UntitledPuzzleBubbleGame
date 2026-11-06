using System;
using UnityEngine;

namespace Game.Menu
{
    /// <summary>
    /// A straight line across the 270×585 stage, from just off one edge to
    /// just off the opposite edge. Points are native pixels, y down.
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

        private const float Width = 270f;
        private const float Height = 585f;

        public Vector2 At(float progress) => Vector2.LerpUnclamped(Start, End, progress);

        public static CrossingPath Pick(System.Random random, Vector2 halfSize, int edge)
        {
            var a = (float)random.NextDouble();
            var b = (float)random.NextDouble();
            return edge switch
            {
                FromLeft => Between(new Vector2(-halfSize.x, 40f + a * 505f), new Vector2(Width + halfSize.x, 40f + b * 505f)),
                FromRight => Between(new Vector2(Width + halfSize.x, 40f + a * 505f), new Vector2(-halfSize.x, 40f + b * 505f)),
                FromTop => Between(new Vector2(20f + a * 230f, -halfSize.y), new Vector2(20f + b * 230f, Height + halfSize.y)),
                _ => Between(new Vector2(20f + a * 230f, Height + halfSize.y), new Vector2(20f + b * 230f, -halfSize.y))
            };
        }

        public static int RandomEdge(System.Random random) => random.Next(4);

        /// <summary>
        /// Remaps a horizontal path into the big-galaxy band: the start lands
        /// in y 250–320 and the end drifts at most ±20 px from it.
        /// </summary>
        public CrossingPath SquashedIntoBigBand()
        {
            var startY = 250f + (Start.y - 40f) / 505f * 70f;
            var endY = startY + (End.y - 290f) / 505f * 40f;
            return Between(new Vector2(Start.x, startY), new Vector2(End.x, endY));
        }

        private static CrossingPath Between(Vector2 start, Vector2 end) => new CrossingPath { Start = start, End = end };
    }
}
