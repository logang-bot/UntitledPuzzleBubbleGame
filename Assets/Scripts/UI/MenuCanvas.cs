using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Configures a canvas for the cartoon menu art's 360×780 pt grid: one
    /// canvas unit is one pt, and Expand keeps the whole grid visible on every
    /// aspect ratio (the extra space becomes background). Reference PPU 1 with
    /// the 3× sheets imported at 3 px per unit keeps sprites and 9-slice
    /// borders at their pt size. Pixel-perfect is off so drift stays smooth;
    /// the pixel pets snap themselves (PixelSnap). See
    /// docs/features/art/overview.md.
    /// </summary>
    public static class MenuCanvas
    {
        public static readonly Vector2 NativeSize = new Vector2(360f, 780f);

        public static void Configure(Canvas canvas)
        {
            canvas.pixelPerfect = false;
            if (!canvas.TryGetComponent<CanvasScaler>(out var scaler)) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = NativeSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            scaler.referencePixelsPerUnit = 1f;
        }
    }
}
