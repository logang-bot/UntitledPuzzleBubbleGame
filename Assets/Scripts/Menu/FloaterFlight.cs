namespace Game.Menu
{
    /// <summary>One crossing of a floater slot: what flies, along which path, and for how long.</summary>
    public struct FloaterFlight
    {
        public int Number;
        public FloaterPick Pick;
        public CrossingPath Path;
        public float Duration;

        public static FloaterFlight Roll(System.Random random, int number)
        {
            var pick = FloaterPick.Roll(random);
            var route = new CrossingRoute(CrossingPath.RandomEdge(random), FloaterSpec.Band);
            var path = CrossingPath.Pick(random, FloaterSpec.HalfExtent(pick.Size), route);
            return new FloaterFlight { Number = number, Pick = pick, Path = path, Duration = path.Length / FloaterSpec.Speed(pick.Size) };
        }
    }
}
