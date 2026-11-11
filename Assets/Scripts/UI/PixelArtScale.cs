using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Sizes the pixel pets so each art pixel covers a whole number of phone
    /// pixels on any screen. The cartoon guide gives their scale in device
    /// pixels on its 3× reference (1080 px wide, 3 device px per pt).
    /// </summary>
    public static class PixelArtScale
    {
        public const float ReferenceDevicePixelsPerPt = 3f;

        /// <summary>
        /// pt per art pixel: the design's device px per art px is rescaled to
        /// this canvas (canvasScale device px per pt), rounded to a whole
        /// number of device pixels (at least 1), then converted back to pt.
        /// </summary>
        public static float UnitsPerArtPixel(int designDevicePixels, float canvasScale)
        {
            var devicePixels = Mathf.Max(1f, Mathf.Round(designDevicePixels * canvasScale / ReferenceDevicePixelsPerPt));
            return devicePixels / canvasScale;
        }

        public static Vector3 SnapToScreenPixel(Vector3 screenPosition)
        {
            return new Vector3(Mathf.Round(screenPosition.x), Mathf.Round(screenPosition.y), screenPosition.z);
        }
    }
}
