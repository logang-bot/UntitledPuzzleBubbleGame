namespace Game.Grid
{
    /// <summary>Solo levels change theme every 5 levels: living room, garden, rooftop, park, then repeat.</summary>
    public static class ThemeForLevel
    {
        public const int LevelsPerTheme = 5;
        private const int ThemeCount = 4;

        public static BoardTheme For(int levelNumber)
        {
            var block = (System.Math.Max(levelNumber, 1) - 1) / LevelsPerTheme;
            return (BoardTheme)(block % ThemeCount);
        }

        public static string Key(BoardTheme theme) => theme.ToString().ToLowerInvariant();
    }
}
