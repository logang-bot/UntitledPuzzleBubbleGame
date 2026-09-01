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
        public static BoardBounds Compute(Vector2 cameraPosition, float boardWidth, float orthographicSize)
        {
            var leftWallX = cameraPosition.x - boardWidth * 0.5f;
            var rightWallX = cameraPosition.x + boardWidth * 0.5f;
            var ceilingY = cameraPosition.y + orthographicSize;
            return new BoardBounds(leftWallX, rightWallX, ceilingY);
        }
    }
}
