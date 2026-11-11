using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Keeps a pixel-art Image crisp on the smooth cartoon canvas: sizes it so
    /// one art pixel is a whole number of screen pixels (PixelArtScale) and,
    /// after everything has moved it this frame, rounds its pivot onto the
    /// screen pixel grid. Only valid on Screen Space Overlay canvases, where
    /// world units are screen pixels.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class PixelSnap : MonoBehaviour
    {
        private RectTransform _rect;
        private Canvas _canvas;
        private Vector2 _artSize;
        private int _designDevicePixels;
        private float _appliedScale;

        public static PixelSnap Attach(RectTransform rect, Vector2 artSize, int designDevicePixels)
        {
            var snap = rect.gameObject.AddComponent<PixelSnap>();
            snap._rect = rect;
            snap._designDevicePixels = designDevicePixels;
            snap.SetArtSize(artSize);
            return snap;
        }

        public void SetArtSize(Vector2 artSize)
        {
            _artSize = artSize;
            _appliedScale = 0f;
        }

        private void LateUpdate()
        {
            if (_canvas == null) _canvas = GetComponentInParent<Canvas>().rootCanvas;
            if (!Mathf.Approximately(_canvas.scaleFactor, _appliedScale)) Resize(_canvas.scaleFactor);
            _rect.position = PixelArtScale.SnapToScreenPixel(_rect.position);
        }

        private void Resize(float canvasScale)
        {
            _appliedScale = canvasScale;
            _rect.sizeDelta = _artSize * PixelArtScale.UnitsPerArtPixel(_designDevicePixels, canvasScale);
        }
    }
}
