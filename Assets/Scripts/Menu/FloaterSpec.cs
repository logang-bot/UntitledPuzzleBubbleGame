using UnityEngine;

namespace Game.Menu
{
    /// <summary>
    /// The main menu's floating pets (motion.json "floaters"), tuned for
    /// smoothness: each size drifts at its own constant speed (small and slow
    /// far away, large and fast up close), pixel art at 4 device px per art px
    /// on the 3× reference, horizontal crossings inside y 250–500, a 2–8 s
    /// rest between crossings, and a ±3 pt bob over 3 s.
    /// </summary>
    public static class FloaterSpec
    {
        public const int SlotCount = 12;
        public const int DesignDevicePixels = 4;
        public const float BobAmplitude = 3f;
        public const float BobPeriod = 3f;
        public const float BubbleSpriteRadius = 48f;
        public static readonly Vector2 Band = new Vector2(250f, 500f);
        public static readonly Vector2 GapSeconds = new Vector2(2f, 8f);

        private const float EdgeMargin = 6f;

        public static float Speed(FloaterSize size) => size switch
        {
            FloaterSize.S => 9f,
            FloaterSize.M => 15f,
            _ => 23f
        };

        public static float BubbleRadius(FloaterSize size) => size switch
        {
            FloaterSize.S => 24f,
            FloaterSize.M => 32f,
            _ => 40f
        };

        /// <summary>Half the square floater frame (34 / 46 / 56 art px) in pt, plus a margin, so crossings start fully off screen.</summary>
        public static Vector2 HalfExtent(FloaterSize size)
        {
            var framePixels = size switch { FloaterSize.S => 34f, FloaterSize.M => 46f, _ => 56f };
            return Vector2.one * (framePixels * DesignDevicePixels / 3f / 2f + EdgeMargin);
        }
    }
}
