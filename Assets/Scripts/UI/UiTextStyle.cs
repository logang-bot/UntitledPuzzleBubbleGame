using UnityEngine;

namespace Game.UI
{
    public struct UiTextStyle
    {
        public Font Font;
        public int Size;
        public Color Color;
        public TextAnchor Alignment;

        public UiTextStyle WithColor(Color color)
        {
            var copy = this;
            copy.Color = color;
            return copy;
        }

        public UiTextStyle WithAlignment(TextAnchor alignment)
        {
            var copy = this;
            copy.Alignment = alignment;
            return copy;
        }

        public void ApplyTo(UnityEngine.UI.Text text)
        {
            text.font = Font;
            text.fontSize = Size;
            text.color = Color;
            text.alignment = Alignment;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
        }
    }
}
