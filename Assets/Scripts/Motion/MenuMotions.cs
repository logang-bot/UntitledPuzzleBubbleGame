namespace Game.Motion
{
    /// <summary>
    /// The UI and sprite entries of the cartoon guide's motion spec
    /// (Assets/Art/Cartoon/Spec/motion.json). System entries (cosmos,
    /// floaters, confetti, fireworks, lanterns) live in their own spec classes.
    /// </summary>
    public static class MenuMotions
    {
        public static readonly MotionClip LogoBob = new MotionClip
        {
            Y = Sine(4f, 3.2f),
            Rotation = Sine(1.5f, 6.4f)
        };

        public static readonly MotionClip ButtonRelease = Pop(new KeyTrack(new MotionKey(0f, 0.96f), new MotionKey(0.14f, 1f, EaseKind.OutBack)), 0.14f);

        public static readonly MotionClip BannerDrop = new MotionClip
        {
            Y = new KeyTrack(new MotionKey(0f, -140f), new MotionKey(0.55f, 0f, EaseKind.OutBack)),
            ScaleX = Grow(0.6f, 0.55f),
            ScaleY = Grow(0.6f, 0.55f),
            Duration = 0.55f
        };

        public static readonly MotionClip PanelPop = new MotionClip
        {
            ScaleX = Grow(0.8f, 0.4f),
            ScaleY = Grow(0.8f, 0.4f),
            Alpha = new KeyTrack(new MotionKey(0f, 0f), new MotionKey(0.2f, 1f, EaseKind.OutQuad)),
            Duration = 0.4f,
            Delay = 0.25f
        };

        public static readonly MotionClip SwatchPick = Pop(new KeyTrack(new MotionKey(0f, 1f), new MotionKey(0.1f, 1.15f, EaseKind.OutQuad), new MotionKey(0.3f, 1f, EaseKind.OutBack)), 0.3f);

        public static readonly MotionClip GalaxyGlow = new MotionClip
        {
            Alpha = new SineTrack(0.2f, 4f, (0f, 0.7f)),
            ScaleX = new SineTrack(0.05f, 4f, (0f, 1f)),
            ScaleY = new SineTrack(0.05f, 4f, (0f, 1f))
        };

        public static readonly MotionClip GalaxySpin = new MotionClip { Rotation = new SpinTrack(3f) };

        public static readonly MotionClip BubbleWobble = new MotionClip
        {
            ScaleX = new SineTrack(0.04f, 1.2f, (0f, 1f)),
            ScaleY = new SineTrack(-0.04f, 1.2f, (0f, 1f))
        };

        public static MotionClip StarTwinkle(float period, float phase) => new MotionClip
        {
            Alpha = new SineTrack(0.4f, period, (phase, 0.6f)),
            ScaleX = new SineTrack(0.15f, period, (phase, 0.95f)),
            ScaleY = new SineTrack(0.15f, period, (phase, 0.95f))
        };

        private static SineTrack Sine(float amplitude, float period) => new SineTrack(amplitude, period, (0f, 0f));

        private static KeyTrack Grow(float from, float duration) => new KeyTrack(new MotionKey(0f, from), new MotionKey(duration, 1f, EaseKind.OutBack));

        private static MotionClip Pop(KeyTrack scale, float duration) => new MotionClip { ScaleX = scale, ScaleY = scale, Duration = duration };
    }
}
