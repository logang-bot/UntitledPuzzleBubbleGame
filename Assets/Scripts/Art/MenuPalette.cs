using UnityEngine;

namespace Game.Art
{
    /// <summary>
    /// The menu palette colors the code paints with directly (captions,
    /// fills, dims). Values come from Assets/Art/Screens/palette.gpl.
    /// </summary>
    public static class MenuPalette
    {
        public static readonly Color Ink = new Color32(26, 20, 36, 255);
        public static readonly Color Shadow = new Color32(58, 46, 74, 255);
        public static readonly Color Stone = new Color32(142, 134, 160, 255);
        public static readonly Color Mist = new Color32(198, 191, 208, 255);
        public static readonly Color White = new Color32(252, 252, 248, 255);
        public static readonly Color Cream = new Color32(246, 226, 188, 255);
        public static readonly Color Abyss = new Color32(18, 14, 34, 255);
        public static readonly Color Night0 = new Color32(28, 26, 52, 255);
        public static readonly Color Night3 = new Color32(70, 76, 112, 255);
        public static readonly Color GoldDark = new Color32(168, 124, 24, 255);
        public static readonly Color Gold = new Color32(246, 204, 48, 255);

        public static Color AbyssDim(float amount) => new Color(Abyss.r, Abyss.g, Abyss.b, amount);
    }
}
