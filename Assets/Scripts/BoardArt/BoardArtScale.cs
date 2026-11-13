using Game.Grid;

namespace Game.BoardArt
{
    /// <summary>
    /// World scales for the cartoon sheets (3 px per unit, so one sprite unit
    /// is one guide pt): bubble-family sprites are drawn 42 pt wide and shrink
    /// to exactly one cell; everything else keeps its pt size.
    /// </summary>
    public static class BoardArtScale
    {
        public const float BubbleSpritePt = 42f;

        public static float Bubble(float cellWidth) => cellWidth / BubbleSpritePt;

        public static float Pt(float cellWidth) => PlayfieldFrame.WorldPerPtFor(cellWidth);
    }
}
