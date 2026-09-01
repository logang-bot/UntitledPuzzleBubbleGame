using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    /// Device-independent screen-fit math (see
    /// docs/features/core-gameplay/screen-fit-and-difficulty-scaling.md).
    /// Pure math only, no Camera/Screen access, so it stays unit-testable.
    /// </summary>
    public static class PlayfieldSizer
    {
        public static float OrthographicSizeForWidth(float boardWidth, float screenWidth, float screenHeight)
        {
            return boardWidth * screenHeight / (2f * screenWidth);
        }

        public static int RowsForWorldHeight(float worldHeight, float cellWidth)
        {
            return Mathf.FloorToInt(worldHeight / HexGridMath.RowHeight(cellWidth));
        }
    }
}
