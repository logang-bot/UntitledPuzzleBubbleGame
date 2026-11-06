namespace Game.Flow
{
    /// <summary>
    /// Timing of the paw-print iris: frames 0→9 at 20 fps close it (0.5 s),
    /// the swap happens on the fully dark frame 9, then 9→0 opens it again.
    /// </summary>
    public static class TransitionTimeline
    {
        public const int FrameCount = 10;
        public const float Fps = 20f;
        public const float HalfDuration = FrameCount / Fps;

        public static int ClosingFrame(float elapsed) => ClampFrame((int)(elapsed * Fps));

        public static int OpeningFrame(float elapsed) => ClampFrame(FrameCount - 1 - (int)(elapsed * Fps));

        public static bool IsHalfDone(float elapsed) => elapsed >= HalfDuration;

        private static int ClampFrame(int frame) => frame < 0 ? 0 : frame >= FrameCount ? FrameCount - 1 : frame;
    }
}
