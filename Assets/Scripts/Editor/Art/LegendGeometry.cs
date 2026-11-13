using UnityEngine;

namespace Game.EditorTools.Art
{
    /// <summary>
    /// Pixel geometry of one legend frame: its rect in Unity's bottom-up
    /// texture space, rounded to whole pixels, and its normalized pivot.
    /// A legend that names its pivot ("center", "bottom-center") without
    /// pivotX/pivotY gets the named point.
    /// </summary>
    public static class LegendGeometry
    {
        public static Rect FrameRect(SheetLegendSprite sprite, int frame, float sheetHeight)
        {
            var x = Mathf.Round(sprite.x + frame * sprite.stepX);
            var top = Mathf.Round(sprite.y);
            var height = Mathf.Round(sprite.frameH);
            return new Rect(x, Mathf.Round(sheetHeight) - top - height, Mathf.Round(sprite.frameW), height);
        }

        public static Vector2 NormalizedPivot(SheetLegendSprite sprite)
        {
            var pixels = PivotPixels(sprite);
            return new Vector2(pixels.x / sprite.frameW, 1f - pixels.y / sprite.frameH);
        }

        public static int MaxTextureSize(SheetLegend legend)
        {
            var longest = Mathf.CeilToInt(Mathf.Max(legend.width, legend.height));
            return Mathf.Max(4096, Mathf.NextPowerOfTwo(longest));
        }

        private static Vector2 PivotPixels(SheetLegendSprite sprite)
        {
            if (sprite.pivotX != 0f || sprite.pivotY != 0f) return new Vector2(sprite.pivotX, sprite.pivotY);
            return NamedPivot(sprite.pivot) * new Vector2(sprite.frameW, sprite.frameH);
        }

        private static Vector2 NamedPivot(string pivot)
        {
            return pivot switch
            {
                "center" => new Vector2(0.5f, 0.5f),
                "bottom-center" => new Vector2(0.5f, 1f),
                _ => Vector2.zero
            };
        }
    }
}
