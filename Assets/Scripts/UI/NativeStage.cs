using UnityEngine;

namespace Game.UI
{
    public static class NativeStage
    {
        public static ScreenLayers Create(RectTransform canvasRect)
        {
            var background = RuntimeUi.CreateRect(canvasRect, "Background", UiPlacement.Fill());
            var safeArea = RuntimeUi.CreateRect(canvasRect, "SafeArea", UiPlacement.Fill());
            safeArea.gameObject.AddComponent<SafeAreaFitter>();
            var stage = RuntimeUi.CreateRect(safeArea, "Stage", UiPlacement.At(new Vector2(0.5f, 0.5f), PixelCanvas.NativeSize));
            return new ScreenLayers { Background = background, Stage = stage };
        }

        /// <summary>A 270×585 rect matching the stage, for full-bleed art that must line up with it.</summary>
        public static RectTransform CreateStageAligned(RectTransform background, string name)
        {
            return RuntimeUi.CreateRect(background, name, UiPlacement.At(new Vector2(0.5f, 0.5f), PixelCanvas.NativeSize));
        }
    }
}
