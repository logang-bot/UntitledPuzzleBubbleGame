using Game.Art;
using static Game.Art.MenuColors;

namespace Game.UI
{
    /// <summary>The 4 text style tokens the cartoon game guide adds (Assets/Art/Game/Spec/tokens.json).</summary>
    public static class GameTextStyles
    {
        public static readonly TextStyle HudValue = TextStyle.Of("hud_value", FontRole.Display, 18f).WithFill(Cream).WithOutline(2.5f, Ink).WithShadow(2f, Ink).WithSpacing(1f);
        public static readonly TextStyle SignLabel = TextStyle.Of("sign_label", FontRole.UiExtraBold, 10f).WithFill(Ink).WithSpacing(1.2f);
        public static readonly TextStyle CountdownDigit = TextStyle.Of("countdown_digit", FontRole.Display, 44f).WithGradient(Hex("#fff6c8"), Hex("#ffb52e")).WithOutline(3.5f, Ink).WithShadow(3f, Ink);
        public static readonly TextStyle ChargeCount = TextStyle.Of("charge_count", FontRole.Display, 13f).WithFill(Ink);
    }
}
