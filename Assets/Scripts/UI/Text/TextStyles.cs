using Game.Art;
using static Game.Art.MenuColors;

namespace Game.UI
{
    /// <summary>The 14 text style tokens of the cartoon menu guide (Assets/Art/Cartoon/Spec/tokens.json).</summary>
    public static class TextStyles
    {
        public static readonly TextStyle Title = Display("title", 34f).WithGradient(Hex("#fff6c8"), Hex("#ffc23a")).WithOutline(3.5f, Ink).WithShadow(3f, Ink).WithSpacing(1f);
        public static readonly TextStyle TitleSad = Display("title_sad", 34f).WithGradient(Hex("#f2f3ff"), Hex("#a9b4ec")).WithOutline(3.5f, Ink).WithShadow(3f, Ink).WithSpacing(1f);
        public static readonly TextStyle TitlePlate = Display("title_plate", 24f).WithFill(ButtonText).WithOutline(3f, Ink).WithShadow(2f, Ink).WithSpacing(1f);
        public static readonly TextStyle ButtonPrimary = Display("btn_primary", 22f).WithFill(ButtonText).WithOutline(3f, Ink).WithShadow(2.5f, Ink).WithSpacing(1f);
        public static readonly TextStyle ButtonSecondary = Display("btn_secondary", 22f).WithFill(ButtonText).WithOutline(3f, Ink).WithShadow(2.5f, Ink).WithSpacing(1f);
        public static readonly TextStyle ButtonDisabled = Display("btn_disabled", 22f).WithFill(DisabledText).WithOutline(2.5f, DisabledOutline).WithSpacing(1f);
        public static readonly TextStyle Label = TextStyle.Of("label", FontRole.UiExtraBold, 15f).WithFill(Cream).WithShadow(1.5f, LabelShadow).WithSpacing(0.6f);
        public static readonly TextStyle SmallLabel = TextStyle.Of("small_label", FontRole.UiBold, 12f).WithFill(Mist).WithSpacing(1.6f);
        public static readonly TextStyle Number = Display("number", 34f).WithGradient(Hex("#fff6c8"), Hex("#ffb52e")).WithOutline(3.5f, Ink).WithShadow(3f, Ink).WithSpacing(1.5f);
        public static readonly TextStyle StatValue = Display("stat_value", 20f).WithFill(Cream).WithOutline(2.5f, Ink).WithShadow(2f, Ink).WithSpacing(1f);
        public static readonly TextStyle SegmentOn = TextStyle.Of("seg_on", FontRole.UiExtraBold, 14f).WithFill(Ink).WithSpacing(1f);
        public static readonly TextStyle SegmentOff = TextStyle.Of("seg_off", FontRole.UiExtraBold, 14f).WithFill(MenuColors.SegmentOff).WithSpacing(1f);
        public static readonly TextStyle NameText = TextStyle.Of("name_text", FontRole.UiExtraBold, 18f).WithFill(Ink).WithSpacing(0.5f);
        public static readonly TextStyle NamePlaceholder = TextStyle.Of("name_placeholder", FontRole.UiSemiBold, 18f).WithFill(FieldPlaceholder).WithSpacing(0.5f);

        private static TextStyle Display(string id, float size) => TextStyle.Of(id, FontRole.Display, size);
    }
}
