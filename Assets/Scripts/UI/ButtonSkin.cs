using Game.Art;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Which buttons.png frames and caption color a ButtonKind uses. Frame
    /// order follows the legend notes (normal, pressed, disabled; the arrow
    /// strip is left normal, left pressed, right normal, right pressed).
    /// </summary>
    public struct ButtonSkin
    {
        public const int NoFrame = -1;

        public string SpriteId;
        public int NormalFrame;
        public int PressedFrame;
        public int DisabledFrame;
        public Color CaptionColor;

        public bool HasDisabledFrame => DisabledFrame != NoFrame;

        public static ButtonSkin For(ButtonKind kind) => kind switch
        {
            ButtonKind.Primary => Labelled("btn_primary", MenuPalette.Ink),
            ButtonKind.Secondary => Labelled("btn_secondary", MenuPalette.Cream),
            ButtonKind.Back => Icon("btn_back", 0),
            ButtonKind.Home => Icon("btn_home", 0),
            ButtonKind.Rematch => Icon("btn_rematch", 0),
            ButtonKind.ArrowLeft => Icon("btn_arrow", 0),
            _ => Icon("btn_arrow", 2)
        };

        private static ButtonSkin Labelled(string spriteId, Color captionColor) => new ButtonSkin
        {
            SpriteId = spriteId,
            NormalFrame = 0,
            PressedFrame = 1,
            DisabledFrame = 2,
            CaptionColor = captionColor
        };

        private static ButtonSkin Icon(string spriteId, int normalFrame) => new ButtonSkin
        {
            SpriteId = spriteId,
            NormalFrame = normalFrame,
            PressedFrame = normalFrame + 1,
            DisabledFrame = NoFrame,
            CaptionColor = MenuPalette.Cream
        };
    }
}
