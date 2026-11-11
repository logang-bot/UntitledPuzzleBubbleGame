using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Where a pixel-art sprite goes: its legend pivot's native point, and the
    /// guide's scale in device pixels per art pixel on the 3× reference.
    /// </summary>
    public struct PixelPlacement
    {
        public Vector2 Pivot;
        public int DesignDevicePixels;

        public PixelPlacement(Vector2 pivot, int designDevicePixels)
        {
            Pivot = pivot;
            DesignDevicePixels = designDevicePixels;
        }
    }
}
