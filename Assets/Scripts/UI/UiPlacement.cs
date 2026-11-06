using UnityEngine;

namespace Game.UI
{
    public struct UiPlacement
    {
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
        public Vector2 SizeDelta;
        public Vector2 Pivot;
        public Vector2 Position;

        private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
        private static readonly Vector2 TopLeftCorner = new Vector2(0f, 1f);

        public static UiPlacement At(Vector2 anchor, Vector2 size) => new UiPlacement
        {
            AnchorMin = anchor,
            AnchorMax = anchor,
            SizeDelta = size,
            Pivot = Center
        };

        public static UiPlacement Stretch(Vector2 anchorMin, Vector2 anchorMax) => new UiPlacement
        {
            AnchorMin = anchorMin,
            AnchorMax = anchorMax,
            SizeDelta = Vector2.zero,
            Pivot = Center
        };

        public static UiPlacement Fill() => Stretch(Vector2.zero, Vector2.one);

        /// <summary>
        /// Places a rect by its top-left corner in native art pixels, measured
        /// from the parent's top-left (y grows downward, as in the legends).
        /// </summary>
        public static UiPlacement TopLeft(RectInt native) => new UiPlacement
        {
            AnchorMin = TopLeftCorner,
            AnchorMax = TopLeftCorner,
            SizeDelta = native.size,
            Pivot = TopLeftCorner,
            Position = new Vector2(native.x, -native.y)
        };

        /// <summary>A full-width band whose top edge sits <paramref name="y"/> native pixels below the parent's top.</summary>
        public static UiPlacement TopBand(int y, int height) => new UiPlacement
        {
            AnchorMin = TopLeftCorner,
            AnchorMax = Vector2.one,
            SizeDelta = new Vector2(0f, height),
            Pivot = new Vector2(0.5f, 1f),
            Position = new Vector2(0f, -y)
        };

        /// <summary>A rect of <paramref name="size"/> centered on a native point (y down from the parent's top-left).</summary>
        public static UiPlacement CenteredAt(Vector2 nativeCenter, Vector2 size) => new UiPlacement
        {
            AnchorMin = TopLeftCorner,
            AnchorMax = TopLeftCorner,
            SizeDelta = size,
            Pivot = Center,
            Position = NativePosition(nativeCenter)
        };

        /// <summary>Converts a native y-down point to a whole-pixel anchoredPosition under a top-left anchor.</summary>
        public static Vector2 NativePosition(Vector2 nativePoint)
        {
            return new Vector2(Mathf.Round(nativePoint.x), -Mathf.Round(nativePoint.y));
        }

        public void ApplyTo(RectTransform rect)
        {
            rect.anchorMin = AnchorMin;
            rect.anchorMax = AnchorMax;
            rect.pivot = Pivot;
            rect.anchoredPosition = Position;
            rect.sizeDelta = SizeDelta;
        }
    }
}
