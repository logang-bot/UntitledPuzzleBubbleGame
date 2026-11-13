using Game.Art;
using UnityEngine;

namespace Game.Deck
{
    /// <summary>
    /// The guide's shot timer rules (timer_pips, countdown_pop): one lit pip
    /// per second left out of 12; from 4 s left the lit pips turn hot and
    /// flash white at 4 fps, and the hurry burst shows the digit, growing
    /// more urgent as it counts down.
    /// </summary>
    public static class ShotClockDisplay
    {
        public const int PipCount = 12;
        public const float HurrySeconds = 4f;
        public const float FlashFps = 4f;

        public static int LitPips(float remaining) => Mathf.Clamp(Mathf.CeilToInt(remaining), 0, PipCount);

        public static bool IsHurrying(float remaining) => remaining > 0f && remaining <= HurrySeconds;

        public static int Digit(float remaining) => Mathf.CeilToInt(remaining);

        public static Color PipColor(int index, float remaining, float time)
        {
            if (index >= LitPips(remaining)) return GameColors.PipOff;
            if (!IsHurrying(remaining)) return GameColors.PipOn;
            return Mathf.FloorToInt(time * FlashFps) % 2 == 1 ? GameColors.PipFlash : GameColors.PipHot;
        }

        public static float Urgency(int digit) => digit switch
        {
            3 => 1.08f,
            2 => 1.16f,
            1 => 1.26f,
            _ => 1f
        };
    }
}
