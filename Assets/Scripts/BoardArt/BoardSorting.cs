namespace Game.BoardArt
{
    /// <summary>Sprite sorting orders of the in-game world layer, back to front.</summary>
    public static class BoardSorting
    {
        public const int Background = -100;
        public const int BackgroundAmbience = -90;
        public const int Ceiling = -30;
        public const int AimDots = -20;
        public const int DangerGlow = -15;
        public const int Bubble = 0;
        public const int BubbleGlyph = 1;
        public const int BubbleOverlay = 2;
        public const int Edges = 20;
        public const int Front = 30;
        public const int Effects = 40;
    }
}
