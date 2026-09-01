namespace Game.Gameplay
{
    /// <summary>
    /// Per-turn countdown for the shooter. Tick reports expiry but never
    /// self-resets — the caller decides when a reset is warranted (e.g. only
    /// once a shot is actually fired), so there is a single reset path.
    /// </summary>
    public class ShotTimer
    {
        public float Duration { get; }
        public float TimeRemaining { get; private set; }

        public ShotTimer(float duration)
        {
            Duration = duration;
            TimeRemaining = duration;
        }

        public bool Tick(float deltaTime)
        {
            TimeRemaining -= deltaTime;
            return TimeRemaining <= 0f;
        }

        public void Reset()
        {
            TimeRemaining = Duration;
        }
    }
}
