using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Configures a canvas for the 270×585 native menu art: one canvas unit
    /// is one art pixel, and Expand keeps the whole native frame visible on
    /// every aspect ratio (the extra space becomes background). With sprites
    /// at 1 pixel per unit, referencePixelsPerUnit 1 keeps 9-slice borders at
    /// their native size. See docs/features/art/overview.md, Art Phase 6b.
    /// </summary>
    public static class PixelCanvas
    {
        public static readonly Vector2 NativeSize = new Vector2(270f, 585f);

        public static void Configure(Canvas canvas)
        {
            canvas.pixelPerfect = true;
            if (!canvas.TryGetComponent<CanvasScaler>(out var scaler)) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = NativeSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            scaler.referencePixelsPerUnit = 1f;
        }
    }
}
