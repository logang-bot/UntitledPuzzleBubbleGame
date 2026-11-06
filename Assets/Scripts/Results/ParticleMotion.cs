using UnityEngine;

namespace Game.Results
{
    /// <summary>Pure motion helpers for the looping overlay particles.</summary>
    public static class ParticleMotion
    {
        /// <summary>Wraps <paramref name="value"/> into [min, min + span), also for negatives.</summary>
        public static float Wrap(float value, float min, float span)
        {
            return min + Mathf.Repeat(value - min, span);
        }

        public static float Sway(float time, float amplitude, float phase)
        {
            return amplitude * Mathf.Sin(time * 1.5f + phase);
        }

        /// <summary>
        /// A burst that plays its frames once at <paramref name="fps"/> and then
        /// stays hidden until the period restarts. Returns -1 while hidden.
        /// </summary>
        public static int BurstFrame(float time, float period, (int Count, float Fps) frames)
        {
            var frame = (int)(Mathf.Repeat(time, period) * frames.Fps);
            return frame < frames.Count ? frame : -1;
        }
    }
}
