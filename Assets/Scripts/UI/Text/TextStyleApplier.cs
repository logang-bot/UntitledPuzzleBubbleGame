using Game.Art;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Puts a TextStyle on a TMP text: font, size, spacing and alignment, the
    /// fill (vertex gradient) on the plain font material, and the outline and
    /// drop shadow on a TextOutlineLayer behind it when the style has them.
    /// </summary>
    public static class TextStyleApplier
    {
        public static void Apply(TMP_Text text, TextStyle style, MenuArtFonts fonts)
        {
            ApplyLayout(text, style, fonts);
            ApplyFill(text, style);
            if (text.font != null) text.fontSharedMaterial = text.font.material;
            if (style.Outline > 0f || style.HasShadow) TextOutlineLayer.For(text).Show(style, fonts);
            else if (text.TryGetComponent<TextOutlineLayer>(out var layer)) layer.Hide();
        }

        public static void ApplyLayout(TMP_Text text, TextStyle style, MenuArtFonts fonts)
        {
            text.font = fonts.For(style.Font);
            text.fontSize = style.Size;
            text.characterSpacing = style.CharacterSpacingEm100();
            text.alignment = style.Alignment;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
        }

        private static void ApplyFill(TMP_Text text, TextStyle style)
        {
            text.enableVertexGradient = style.HasGradient;
            text.color = style.HasGradient ? Color.white : style.FillTop;
            if (style.HasGradient) text.colorGradient = new VertexGradient(style.FillTop, style.FillTop, style.FillBottom, style.FillBottom);
        }
    }
}
