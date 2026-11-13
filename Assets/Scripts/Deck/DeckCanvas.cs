using Game.Grid;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Deck
{
    /// <summary>
    /// The deck's overlay canvas: the guide's 360 pt grid matched to the
    /// screen width, so one pt is exactly as wide as on the width-fitted
    /// board. Returns a stage in layout pt, anchored to the bottom of the
    /// screen (solo, lifted over a tall gesture bar) or of a Battle half (the
    /// top half turned 180°).
    /// </summary>
    public static class DeckCanvas
    {
        private const int SortingOrder = 5;

        public static RectTransform SoloStage()
        {
            var canvas = CreateCanvas("DeckCanvas");
            var raise = PlayfieldFrame.RaisePt(Screen.safeArea.yMin, Screen.width);
            return Stage(canvas, DeckLayout.SoloStageHeight, raise);
        }

        public static RectTransform BattleStage(bool top)
        {
            var canvas = CreateCanvas(top ? "TopDeckCanvas" : "BottomDeckCanvas");
            var half = RuntimeUi.CreateRect(canvas, "Half", UiPlacement.Stretch(new Vector2(0f, top ? 0.5f : 0f), new Vector2(1f, top ? 1f : 0.5f)));
            if (top) half.localRotation = Quaternion.Euler(0f, 0f, 180f);
            return Stage(half, DeckLayout.BattleStageHeight, 0f);
        }

        private static RectTransform CreateCanvas(string name)
        {
            var canvas = OverlayCanvasFactory.Create(name, SortingOrder);
            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f;
            return canvas;
        }

        private static RectTransform Stage(RectTransform parent, float height, float raisePt)
        {
            var bottomCenter = new Vector2(0.5f, 0f);
            var placement = new UiPlacement { AnchorMin = bottomCenter, AnchorMax = bottomCenter, Pivot = bottomCenter, SizeDelta = new Vector2(DeckGeometry.StageWidth, height), Position = new Vector2(0f, raisePt) };
            return RuntimeUi.CreateRect(parent, "DeckStage", placement);
        }
    }
}
