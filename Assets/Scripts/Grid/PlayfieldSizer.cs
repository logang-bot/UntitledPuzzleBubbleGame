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

        /// <summary>
        /// Whole rows that fit in a band of the given height: row r spans
        /// r * rowHeight down to r * rowHeight + cellWidth, so the first row
        /// takes a full cell and each further row adds one row height.
        /// </summary>
        public static int RowsFitting(float bandHeight, float cellWidth)
        {
            var extraRows = Mathf.FloorToInt((bandHeight - cellWidth) / HexGridMath.RowHeight(cellWidth));
            return Mathf.Max(0, extraRows + 1);
        }
    }
}
