using Game.Settings;
using UnityEngine;

namespace Game.Motion
{
    /// <summary>
    /// The board entries of the cartoon game guide's motion spec
    /// (Assets/Art/Game/Spec/motion.json). Offsets are in pt, y down.
    /// </summary>
    public static class GameMotions
    {
        public const float BuildInSeconds = 0.36f;
        public const float PushSlideSeconds = 0.35f;
        public const float SettleSlideSeconds = 0.07f;

        public static readonly MotionClip SettleBounce = new MotionClip
        {
            Y = new KeyTrack(new MotionKey(0f, 0f), new MotionKey(0.07f, 4f, EaseKind.OutQuad), new MotionKey(0.19f, -1.4f, EaseKind.InOutSine), new MotionKey(0.32f, 0f, EaseKind.InOutSine)),
            Duration = 0.32f
        };

        public static readonly MotionClip SettleSquash = new MotionClip
        {
            ScaleX = new KeyTrack(new MotionKey(0f, 1f), new MotionKey(0.06f, 1.2f, EaseKind.OutQuad), new MotionKey(0.16f, 0.94f, EaseKind.InOutSine), new MotionKey(0.3f, 1f, EaseKind.OutBack)),
            ScaleY = new KeyTrack(new MotionKey(0f, 1f), new MotionKey(0.06f, 0.8f, EaseKind.OutQuad), new MotionKey(0.16f, 1.06f, EaseKind.InOutSine), new MotionKey(0.3f, 1f, EaseKind.OutBack)),
            Duration = 0.3f
        };

        public static readonly MotionClip PointerTap = Pop(new KeyTrack(new MotionKey(0f, 0.92f), new MotionKey(0.16f, 1f, EaseKind.OutBack)), 0.16f);

        public static readonly MotionClip NextPop = Pop(new KeyTrack(new MotionKey(0f, 0f), new MotionKey(0.22f, 1f, EaseKind.OutBack)), 0.22f);

        public static readonly MotionClip CountdownPop = new MotionClip
        {
            ScaleX = CountdownGrow(),
            ScaleY = CountdownGrow(),
            Alpha = new KeyTrack(new MotionKey(0f, 0f), new MotionKey(0.08f, 1f, EaseKind.OutQuad)),
            Duration = 0.3f
        };

        public static readonly MotionClip RainbowSpin = new MotionClip { Rotation = new SpinTrack(140f) };

        public static readonly MotionClip BombSpark = new MotionClip
        {
            ScaleX = new SineTrack(0.25f, 0.12f, (0f, 1f)),
            ScaleY = new SineTrack(0.25f, 0.12f, (0f, 1f)),
            Rotation = new SpinTrack(400f)
        };

        public static readonly SineTrack CountdownWobble = new(6f, 0.25f, (0f, 0f));

        public static MotionClip Settle(LandingAnimationStyle style) => style == LandingAnimationStyle.SquashPop ? SettleSquash : SettleBounce;

        public static MotionClip BuildIn(float delaySeconds) => new MotionClip
        {
            ScaleX = Grow(),
            ScaleY = Grow(),
            Alpha = new KeyTrack(new MotionKey(0f, 0f), new MotionKey(0.12f, 1f, EaseKind.OutQuad)),
            Duration = BuildInSeconds,
            Delay = delaySeconds
        };

        public static MotionClip NeighborNudge(Vector2 awayPt) => new MotionClip
        {
            X = Nudge(awayPt.x),
            Y = Nudge(awayPt.y),
            Duration = 0.24f
        };

        private static MotionClip Pop(KeyTrack scale, float duration) => new MotionClip { ScaleX = scale, ScaleY = scale, Duration = duration };

        private static KeyTrack CountdownGrow() => new KeyTrack(new MotionKey(0f, 0.4f), new MotionKey(0.18f, 1.12f, EaseKind.OutBack), new MotionKey(0.3f, 1f, EaseKind.InOutSine));

        private static KeyTrack Grow() => new KeyTrack(new MotionKey(0f, 0f), new MotionKey(BuildInSeconds, 1f, EaseKind.OutBack));

        private static KeyTrack Nudge(float distance) => new KeyTrack(new MotionKey(0f, 0f), new MotionKey(0.06f, distance, EaseKind.OutQuad), new MotionKey(0.24f, 0f, EaseKind.OutBack));
    }
}
