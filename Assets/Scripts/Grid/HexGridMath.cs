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
    }
}
