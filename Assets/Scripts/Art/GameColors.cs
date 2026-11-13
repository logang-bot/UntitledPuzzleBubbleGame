using UnityEngine;
using static Game.Art.MenuColors;

namespace Game.Art
{
    /// <summary>The cartoon game guide's color tokens (Assets/Art/Game/Spec/tokens.json) that the game tints with at runtime.</summary>
    public static class GameColors
    {
        public static readonly Color PipOn = Hex("#ffc93c");
        public static readonly Color PipHot = Hex("#ff6f61");
        public static readonly Color PipOff = Hex("#5e5470");
        public static readonly Color PipFlash = Color.white;
        public static readonly Color FreezeTint = new Color(160 / 255f, 220 / 255f, 1f, 0.16f);
    }
}
