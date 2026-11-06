using Game.Art;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// The text sizes the menu mockups use, in native pixels: titles and
    /// button captions in Pixelify Sans, small labels in Silkscreen.
    /// </summary>
    public class MenuTextStyles
    {
        public const int TitleSize = 16;
        public const int CaptionSize = 13;
        public const int SmallSize = 8;

        public UiTextStyle Title { get; }
        public UiTextStyle Caption { get; }
        public UiTextStyle Small { get; }

        public MenuTextStyles(MenuArtLibrary art)
        {
            Title = Style(art.CaptionFont, TitleSize, MenuPalette.Cream);
            Caption = Style(art.CaptionFont, CaptionSize, MenuPalette.Cream);
            Small = Style(art.SmallFont, SmallSize, MenuPalette.Mist);
        }

        private static UiTextStyle Style(Font font, int size, Color color) => new UiTextStyle
        {
            Font = font,
            Size = size,
            Color = color,
            Alignment = TextAnchor.MiddleCenter
        };
    }
}
