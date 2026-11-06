namespace Game.Art
{
    /// <summary>
    /// Maps elapsed seconds to a sprite frame index. Static sprites (fps 0 or
    /// a single frame) always sit on frame 0.
    /// </summary>
    public static class FrameClock
    {
        public static int LoopingFrame(float elapsed, float fps, int frameCount)
        {
            if (IsStatic(fps, frameCount)) return 0;
            return (int)(elapsed * fps) % frameCount;
        }

        public static int OnceFrame(float elapsed, float fps, int frameCount)
        {
            if (IsStatic(fps, frameCount)) return 0;
            var frame = (int)(elapsed * fps);
            return frame < frameCount ? frame : frameCount - 1;
        }

        public static bool IsFinished(float elapsed, float fps, int frameCount)
        {
            return IsStatic(fps, frameCount) || elapsed * fps >= frameCount;
        }

        private static bool IsStatic(float fps, int frameCount) => fps <= 0f || frameCount <= 1;
    }
}
