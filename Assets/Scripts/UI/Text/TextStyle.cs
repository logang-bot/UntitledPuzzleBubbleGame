using Game.Art;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// One text style token from the cartoon guide (tokens.json): font, size
    /// in pt, solid or top-to-bottom gradient fill, outline, drop shadow
    /// (drawn with the outline, like the guide) and letter spacing. Built
    /// fluently from TextStyles; each step returns a modified copy.
    /// </summary>
    public struct TextStyle
    {
        public string Id;
        public FontRole Font;
        public float Size;
        public Color FillTop;
        public Color FillBottom;
        public float Outline;
        public Color OutlineColor;
        public Vector2 ShadowOffset;
        public Color ShadowColor;
        public float Spacing;
        public TextAlignmentOptions Alignment;

        public bool HasGradient => FillTop != FillBottom;
        public bool HasShadow => ShadowOffset != Vector2.zero;
        public string MaterialKey => $"{Id}|{Outline}|{OutlineColor}|{ShadowOffset}|{ShadowColor}";

        public static TextStyle Of(string id, FontRole font, float size) => new TextStyle
        {
            Id = id,
            Font = font,
            Size = size,
            FillTop = Color.white,
            FillBottom = Color.white,
            Alignment = TextAlignmentOptions.Center
        };

        public TextStyle WithFill(Color color)
        {
            var copy = this;
            copy.FillTop = color;
            copy.FillBottom = color;
            return copy;
        }

        public TextStyle WithGradient(Color top, Color bottom)
        {
            var copy = this;
            copy.FillTop = top;
            copy.FillBottom = bottom;
            return copy;
        }

        public TextStyle WithOutline(float width, Color color)
        {
            var copy = this;
            copy.Outline = width;
            copy.OutlineColor = color;
            return copy;
        }

        public TextStyle WithShadow(float drop, Color color)
        {
            var copy = this;
            copy.ShadowOffset = new Vector2(0f, drop);
            copy.ShadowColor = color;
            return copy;
        }

        public TextStyle WithSpacing(float spacing)
        {
            var copy = this;
            copy.Spacing = spacing;
            return copy;
        }

        public TextStyle Aligned(TextAlignmentOptions alignment)
        {
            var copy = this;
            copy.Alignment = alignment;
            return copy;
        }

        /// <summary>TMP characterSpacing is in hundredths of an em, the guide's spacing in pt.</summary>
        public float CharacterSpacingEm100() => Spacing / Size * 100f;
    }
}
