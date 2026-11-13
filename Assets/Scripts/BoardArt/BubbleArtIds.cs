using Game.Grid;

namespace Game.BoardArt
{
    /// <summary>Sprite ids of the cartoon bubbles (gm_bubbles): body, glyph and pop per color.</summary>
    public static class BubbleArtIds
    {
        public const string Scared = "bubble_scared";

        public static string Body(BubbleColor color) => "bubble_" + Key(color);

        public static string Pop(BubbleColor color) => "pop_" + Key(color);

        public static string Glyph(BubbleColor color) => color switch
        {
            BubbleColor.Red => "glyph_heart",
            BubbleColor.Orange => "glyph_fish",
            BubbleColor.Yellow => "glyph_star",
            BubbleColor.Green => "glyph_paw",
            BubbleColor.Blue => "glyph_bone",
            _ => "glyph_yarn"
        };

        private static string Key(BubbleColor color) => color.ToString().ToLowerInvariant();
    }
}
