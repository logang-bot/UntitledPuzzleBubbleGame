using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    /// Generates a plain white circle sprite at runtime, so debug rendering
    /// doesn't depend on any imported placeholder art.
    /// </summary>
    public static class CircleSpriteFactory
    {
        private const int TextureSize = 64;

        public static Sprite CreateWhiteCircle()
        {
            var texture = new Texture2D(TextureSize, TextureSize);
            PaintCircle(texture);
            texture.Apply();
            return ToSprite(texture);
        }

        private static void PaintCircle(Texture2D texture)
        {
            var radius = TextureSize * 0.5f;
            var center = new Vector2(radius, radius);
            for (var y = 0; y < TextureSize; y++)
                for (var x = 0; x < TextureSize; x++)
                    texture.SetPixel(x, y, PixelColor(x, y, center, radius));
        }

        private static Color PixelColor(int x, int y, Vector2 center, float radius)
        {
            var inside = Vector2.Distance(new Vector2(x, y), center) <= radius;
            return inside ? Color.white : new Color(0f, 0f, 0f, 0f);
        }

        private static Sprite ToSprite(Texture2D texture)
        {
            var rect = new Rect(0, 0, TextureSize, TextureSize);
            return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), TextureSize);
        }
    }
}
