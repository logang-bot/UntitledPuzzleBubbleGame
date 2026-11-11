using UnityEngine;

namespace Game.Results
{
    /// <summary>Pure motion helpers for the looping result-screen particles.</summary>
    public static class ParticleMotion
    {
        /// <summary>Wraps <paramref name="value"/> into [min, min + span), also for negatives.</summary>
        public static float Wrap(float value, float min, float span)
        {
            return min + Mathf.Repeat(value - min, span);
        }

        /// <summary>sin(2π(t / period + phase)), a wave in [−1, 1].</summary>
        public static float Wave(float time, float period, float phase)
        {
            return Mathf.Sin(2f * Mathf.PI * (time / period + phase));
        }

        /// <summary>The frame of a burst <paramref name="age"/> seconds old that plays its frames once, or −1 once it is over.</summary>
        public static int BurstFrame(float age, (int Count, float Fps) frames)
        {
            if (age < 0f) return -1;
            var frame = (int)(age * frames.Fps);
            return frame < frames.Count ? frame : -1;
        }

        public static float Between(System.Random random, float min, float max) => min + (float)random.NextDouble() * (max - min);
    }
}
