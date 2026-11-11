using UnityEngine;

namespace Game.Motion
{
    public enum EaseKind { Linear, InQuad, OutQuad, InOutSine, InCubic, OutCubic, OutBack }

    /// <summary>The seven easing curves of the cartoon guide's motion spec, for t in [0, 1].</summary>
    public static class Ease
    {
        private const float BackOvershoot = 1.70158f;

        public static float Apply(EaseKind kind, float t) => kind switch
        {
            EaseKind.InQuad => t * t,
            EaseKind.OutQuad => 1f - (1f - t) * (1f - t),
            EaseKind.InOutSine => -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f,
            EaseKind.InCubic => t * t * t,
            EaseKind.OutCubic => 1f - Mathf.Pow(1f - t, 3f),
            EaseKind.OutBack => OutBack(t),
            _ => t
        };

        /// <summary>Penner's back-out: 1 + (c+1)(t−1)³ + c(t−1)², overshooting by c = 1.70158.</summary>
        private static float OutBack(float t)
        {
            var u = t - 1f;
            return 1f + (BackOvershoot + 1f) * u * u * u + BackOvershoot * u * u;
        }
    }
}
