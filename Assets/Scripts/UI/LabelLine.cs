namespace Game.UI
{
    /// <summary>A horizontally centered line of text whose top sits at native y.</summary>
    public struct LabelLine
    {
        public int Y;
        public UiTextStyle Style;

        public LabelLine(int y, UiTextStyle style)
        {
            Y = y;
            Style = style;
        }
    }
}
