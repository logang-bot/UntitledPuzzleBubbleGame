using UnityEngine;

namespace Game.Shooter
{
    /// <summary>
    /// Derives world-space board bounds from camera framing. Pure math, no
    /// Camera/Screen access inside, so it stays unit-testable (mirrors
    /// Game.Grid.PlayfieldSizer's convention).
    /// </summary>
    public static class BoardBoundsCalculator
    {
        // ceilingHeight is the band reserved at the screen's top edge for the
        // ceiling visual (see CeilingRenderer) - a shot's ceiling boundary is
        // row 0's top edge, which sits that much below the screen's actual top.
        public static BoardBounds Compute(Vector2 cameraPosition, float boardWidth, float orthographicSize, float ceilingHeight)
        {
            var leftWallX = cameraPosition.x - boardWidth * 0.5f;
            var rightWallX = cameraPosition.x + boardWidth * 0.5f;
            var ceilingY = cameraPosition.y + orthographicSize - ceilingHeight;
            return new BoardBounds(leftWallX, rightWallX, ceilingY);
        }
    }
}
