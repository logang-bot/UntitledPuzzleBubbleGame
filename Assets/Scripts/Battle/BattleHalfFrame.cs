using Game.UI;
using UnityEngine;

namespace Game.Battle
{
    /// <summary>
    /// The two 360×390 pt halves of the 2-player result. Player 2 sits on top,
    /// rotated 180° so it reads correctly from the far end of the phone.
    /// </summary>
    public static class BattleHalfFrame
    {
        public const int Width = 360;
        public const int HalfHeight = 390;

        public static RectTransform Create(RectTransform stage, bool isTop)
        {
            var rect = new RectInt(0, isTop ? 0 : HalfHeight, Width, HalfHeight);
            var half = RuntimeUi.CreateRect(stage, isTop ? "TopHalf" : "BottomHalf", UiPlacement.TopLeft(rect));
            if (!isTop) return half;
            UiPlacement.MovePivot(half, new Vector2(0.5f, 0.5f));
            half.localEulerAngles = new Vector3(0f, 0f, 180f);
            return half;
        }
    }
}
