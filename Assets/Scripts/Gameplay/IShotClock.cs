namespace Game.Gameplay
{
    /// <summary>A turn's countdown the deck shows as timer pips and the last-seconds hurry burst.</summary>
    public interface IShotClock
    {
        float ShotTimeRemaining { get; }
    }
}
