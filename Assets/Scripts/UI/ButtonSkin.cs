namespace Game.UI
{
    /// <summary>
    /// Which cartoon sprites and caption style a ButtonKind uses. Labelled
    /// buttons share the disabled face; icon buttons have the glyph baked in
    /// and no disabled state.
    /// </summary>
    public struct ButtonSkin
    {
        public const string DisabledFace = "btn_disabled_normal";

        public string NormalId;
        public string PressedId;
        public bool HasCaption;
        public TextStyle Caption;

        public static ButtonSkin For(ButtonKind kind) => kind switch
        {
            ButtonKind.Primary => Labelled("btn_primary", TextStyles.ButtonPrimary),
            ButtonKind.Secondary => Labelled("btn_secondary", TextStyles.ButtonSecondary),
            ButtonKind.Disabled => Labelled("btn_disabled", TextStyles.ButtonDisabled),
            ButtonKind.Back => Icon("icon_back"),
            ButtonKind.Home => Icon("icon_home"),
            ButtonKind.Rematch => Icon("icon_rematch"),
            ButtonKind.ArrowLeft => Icon("arrow_left"),
            _ => Icon("arrow_right")
        };

        private static ButtonSkin Labelled(string spriteId, TextStyle caption) => new ButtonSkin
        {
            NormalId = spriteId + "_normal",
            PressedId = spriteId + "_pressed",
            HasCaption = true,
            Caption = caption
        };

        private static ButtonSkin Icon(string spriteId) => new ButtonSkin
        {
            NormalId = spriteId + "_normal",
            PressedId = spriteId + "_pressed"
        };
    }
}
