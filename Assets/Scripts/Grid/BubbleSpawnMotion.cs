using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    /// Pure scale-in easing for the level-load build-in animation
    /// (ease-out-cubic). See
    /// docs/features/level-content/specs/2026-09-11-curated-levels-and-transitions-design.md.
    /// </summary>
    public static class BubbleSpawnMotion
    {
        public static float ScaleForProgress(float t)
        {
            var m = 1f - Mathf.Clamp01(t);
            return 1f - m * m * m;
        }
    }
}
