namespace Game.Deck
{
    /// <summary>
    /// The pixel strip a launcher pet shows: its id, either a fixed frame or
    /// a time into the strip (looping unless Once), and whether it is
    /// mirrored. Front-view strips are never mirrored.
    /// </summary>
    public readonly struct PetPose
    {
        public const int NoFixedFrame = -1;

        public readonly string Id;
        public readonly float Time;
        public readonly int FixedFrame;
        public readonly bool Once;
        public readonly bool Flip;

        public PetPose(string id, (float Time, int FixedFrame, bool Once) playback, bool flip)
        {
            Id = id;
            Time = playback.Time;
            FixedFrame = playback.FixedFrame;
            Once = playback.Once;
            Flip = flip;
        }
    }
}
