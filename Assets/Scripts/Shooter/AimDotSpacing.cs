namespace Game.Shooter
{
    /// <summary>Where the aim dots sit along the path, in world units: the first gap past the pointer, the dot spacing and the scroll offset.</summary>
    public readonly struct AimDotSpacing
    {
        public readonly float Start;
        public readonly float Gap;
        public readonly float Scroll;

        public AimDotSpacing(float start, float gap, float scroll)
        {
            Start = start;
            Gap = gap;
            Scroll = scroll;
        }
    }
}
