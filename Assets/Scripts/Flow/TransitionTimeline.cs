using Game.Motion;
using UnityEngine;

namespace Game.Flow
{
    /// <summary>
    /// Timing of the paw-print iris (motion.json paw_close / paw_open): the
    /// hole shrinks from 24× (the pad covers the whole screen) to nothing in
    /// 0.45 s easing in, holds shut 0.1 s while the scene swaps, then grows
    /// back in 0.45 s easing out.
    /// </summary>
    public static class TransitionTimeline
    {
        public const float CloseSeconds = 0.45f;
        public const float HoldSeconds = 0.1f;
        public const float OpenSeconds = 0.45f;
        public const float OpenScale = 24f;

        public static float ClosingScale(float elapsed)
        {
            return OpenScale * (1f - Ease.Apply(EaseKind.InCubic, Mathf.Clamp01(elapsed / CloseSeconds)));
        }

        public static float OpeningScale(float elapsed)
        {
            return OpenScale * Ease.Apply(EaseKind.OutCubic, Mathf.Clamp01(elapsed / OpenSeconds));
        }
    }
}
