using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// A 1 px dotted rule (one dot every 3 px) drawn as a tiled 3×1 sprite,
    /// for the dividers between stat rows.
    /// </summary>
    public static class DottedLine
    {
        private const int Pitch = 3;
        private static Sprite _dotSprite;

        public static Image Create(RectTransform parent, RectInt rect, Color color)
        {
            var line = RuntimeUi.CreateImage(parent, "DottedLine", UiPlacement.TopLeft(rect));
            line.sprite = DotSprite();
            line.type = Image.Type.Tiled;
            line.color = color;
            line.raycastTarget = false;
            return line;
        }

        private static Sprite DotSprite()
        {
            if (_dotSprite != null) return _dotSprite;
            var texture = new Texture2D(Pitch, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
            texture.SetPixels(new[] { Color.white, Color.clear, Color.clear });
            texture.Apply();
            _dotSprite = Sprite.Create(texture, new Rect(0, 0, Pitch, 1), Vector2.zero, 1f);
            return _dotSprite;
        }
    }
}
