using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Creates a standalone pixel-art overlay canvas, so result screens can
    /// use the native 270×585 scaling without touching the HUD canvas.
    /// </summary>
    public static class OverlayCanvasFactory
    {
        public static RectTransform Create(string name, int sortingOrder)
        {
            var canvasObject = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            PixelCanvas.Configure(canvas);
            return (RectTransform)canvasObject.transform;
        }
    }
}
