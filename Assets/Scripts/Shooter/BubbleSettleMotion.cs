using Game.Settings;
using UnityEngine;

namespace Game.Shooter
{
    public static class BubbleSettleMotion
    {
        private const float SquashAmplitude = 0.25f;

        public static float Ease(LandingAnimationStyle style, float t)
        {
            return style == LandingAnimationStyle.OvershootBounce ? EaseOutBack(t) : EaseOutCubic(t);
        }

        public static Vector2 SquashScale(LandingAnimationStyle style, float t)
        {
            return style == LandingAnimationStyle.SquashPop ? Squash(t) : Vector2.one;
        }

        /// <summary>
        /// Standard cubic ease-out-back curve: overshoots 1 before settling back
        /// to it, so the bubble springs slightly past its final position instead
        /// of stopping dead. Constants are the canonical easing-functions values.
        /// </summary>
        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            var m = t - 1f;
            return 1f + c3 * m * m * m + c1 * m * m;
        }

        private static float EaseOutCubic(float t)
        {
            var m = 1f - t;
            return 1f - m * m * m;
        }

        /// <summary>
        /// Sine-based squash: widens x / flattens y, peaking at t=0.5 and
        /// returning to (1,1) at t=1 - composes with any position tween over
        /// the same [0,1] duration with no separate phase-tracking.
        /// </summary>
        private static Vector2 Squash(float t)
        {
            var offset = SquashAmplitude * Mathf.Sin(t * Mathf.PI);
            return new Vector2(1f + offset, 1f - offset);
        }
    }
}
