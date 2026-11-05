using UnityEngine;

namespace Game.UI
{
    public struct UiPlacement
    {
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
        public Vector2 SizeDelta;

        public static UiPlacement At(Vector2 anchor, Vector2 size) => new UiPlacement
        {
            AnchorMin = anchor,
            AnchorMax = anchor,
            SizeDelta = size
        };

        public static UiPlacement Stretch(Vector2 anchorMin, Vector2 anchorMax) => new UiPlacement
        {
            AnchorMin = anchorMin,
            AnchorMax = anchorMax,
            SizeDelta = Vector2.zero
        };

        public static UiPlacement Fill() => Stretch(Vector2.zero, Vector2.one);

        public void ApplyTo(RectTransform rect)
        {
            rect.anchorMin = AnchorMin;
            rect.anchorMax = AnchorMax;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = SizeDelta;
        }
    }
}
