using TMPro;

namespace Game.Art
{
    /// <summary>The menu's TMP fonts: Lilita One for display text, Baloo 2 in three weights for UI text.</summary>
    [System.Serializable]
    public struct MenuArtFonts
    {
        public TMP_FontAsset Display;
        public TMP_FontAsset UiSemiBold;
        public TMP_FontAsset UiBold;
        public TMP_FontAsset UiExtraBold;

        public TMP_FontAsset For(FontRole role) => role switch
        {
            FontRole.Display => Display,
            FontRole.UiSemiBold => UiSemiBold,
            FontRole.UiBold => UiBold,
            _ => UiExtraBold
        };
    }
}
