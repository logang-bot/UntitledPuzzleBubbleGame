using Game.UI;
using UnityEngine;

namespace Game.Battle
{
    /// <summary>
    /// The two 270×292 halves of the 2-player result. Player 2 sits on top,
    /// rotated 180° so it reads correctly from the far end of the phone.
    /// </summary>
    public static class BattleHalfFrame
    {
        public const int HalfHeight = 292;
        public const int BottomHalfY = 293;

        public static RectTransform Create(RectTransform stage, bool isTop)
        {
            var rect = new RectInt(0, isTop ? 0 : BottomHalfY, 270, HalfHeight);
            var half = RuntimeUi.CreateRect(stage, isTop ? "TopHalf" : "BottomHalf", UiPlacement.TopLeft(rect));
            if (!isTop) return half;
            half.pivot = new Vector2(0.5f, 0.5f);
            half.anchoredPosition += new Vector2(rect.width / 2f, -rect.height / 2f);
            half.localEulerAngles = new Vector3(0f, 0f, 180f);
            return half;
        }
    }
}
