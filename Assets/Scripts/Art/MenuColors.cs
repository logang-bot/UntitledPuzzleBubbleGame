using UnityEngine;

namespace Game.Art
{
    /// <summary>The cartoon guide's color tokens (tokens.json) that the screens use at runtime.</summary>
    public static class MenuColors
    {
        public static readonly Color Ink = Hex("#1a1424");
        public static readonly Color Abyss = Hex("#0b0820");
        public static readonly Color Night = Hex("#1b1442");
        public static readonly Color Cream = Hex("#fff3dc");
        public static readonly Color ButtonText = Hex("#fff8e8");
        public static readonly Color Mist = Hex("#c5b9e6");
        public static readonly Color SegmentOff = Hex("#b9addb");
        public static readonly Color DisabledText = Hex("#d4cee0");
        public static readonly Color DisabledOutline = Hex("#4b4560");
        public static readonly Color LabelShadow = Hex("#140d33");
        public static readonly Color FieldPlaceholder = Hex("#a597b8");
        public static readonly Color PanelRim = Hex("#6a58b8");
        public static readonly Color Gold = Hex("#ffc93c");
        public static readonly Color VictoryDim = new Color(11 / 255f, 8 / 255f, 32 / 255f, 0.72f);
        public static readonly Color StatsDim = new Color(11 / 255f, 8 / 255f, 32 / 255f, 0.78f);
        public static readonly Color SettingsBackdrop = new Color(16 / 255f, 10 / 255f, 44 / 255f, 0.62f);

        public static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var color) ? color : Color.magenta;
    }
}
