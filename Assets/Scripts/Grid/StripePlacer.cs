namespace Game.Grid
{
    /// <summary>
    /// Fills a row band solid with repeating same-width color stripes along
    /// the given axis. See
    /// docs/features/level-content/specs/2026-09-11-curated-levels-and-transitions-design.md.
    /// </summary>
    public static class StripePlacer
    {
        public static void PlaceVertical(PatternPlacementContext context, (int Start, int EndExclusive) rowBand)
        {
            for (var row = rowBand.Start; row < rowBand.EndExclusive; row++)
                for (var col = 0; col < context.Grid.Cols; col++)
                    context.Grid.PlaceBubble(row, col, ColorForIndex(context, col));
        }

        public static void PlaceHorizontal(PatternPlacementContext context, (int Start, int EndExclusive) rowBand)
        {
            for (var row = rowBand.Start; row < rowBand.EndExclusive; row++)
                for (var col = 0; col < context.Grid.Cols; col++)
                    context.Grid.PlaceBubble(row, col, ColorForIndex(context, row - rowBand.Start));
        }

        private static BubbleColor ColorForIndex(PatternPlacementContext context, int index)
        {
            var stripeIndex = index / context.Settings.StripeWidth;
            return BubbleColorPalette.AllColors[stripeIndex % context.ColorCount];
        }
    }
}
