namespace Game.Grid
{
    /// <summary>
    /// Shared hex-packing constants (see docs/features/core-gameplay/hex-grid.md).
    /// </summary>
    public static class HexGridMath
    {
        public const float RowHeightFactor = 0.8660254f;

        public static float RowHeight(float cellWidth)
        {
            return cellWidth * RowHeightFactor;
        }

        /// <summary>
        /// Odd rows shift every bubble half a cell right (GridModel.GetWorldPosition),
        /// so fitting only cols*cellWidth clips the odd row's rightmost bubble against
        /// the right wall. This adds the missing half-cell margin.
        /// </summary>
        public static float BoardWidthWithOffsetMargin(int cols, float cellWidth)
        {
            return (cols + 0.5f) * cellWidth;
        }

        /// <summary>
        /// Local-to-world x offset that centers the combined even/odd-row footprint
        /// (see BoardWidthWithOffsetMargin) under the camera, rather than just the
        /// unshifted even-row columns.
        /// </summary>
        public static float BoardOriginXOffset(int cols, float cellWidth)
        {
            return (cols - 0.5f) * cellWidth * 0.5f;
        }
    }
}
