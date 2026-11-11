using Game.Pets;

namespace Game.Menu
{
    /// <summary>
    /// One random space pet for a main-menu slot cycle: species, shape,
    /// colorway, size (S 20 %, M 40 %, L 40 %), pose, a 0/90/180/270° turn
    /// (none for the tumble pose, which already spins), mirror, bubble (20 %)
    /// and a 0–1 bob/animation phase.
    /// </summary>
    public struct FloaterPick
    {
        public const int ShapeCount = 4;
        public const int ColorwayCount = 6;
        public static readonly string[] Poses = { "spread", "swim", "wave", "curl", "star", "tumble" };

        public PetSpecies Species;
        public int ShapeIndex;
        public int ColorwayIndex;
        public FloaterSize Size;
        public int PoseIndex;
        public int QuarterTurns;
        public bool Mirror;
        public bool InBubble;
        public float Phase;

        private const string TumblePose = "tumble";
        private static readonly FloaterSize[] SizeWeights = { FloaterSize.S, FloaterSize.M, FloaterSize.M, FloaterSize.L, FloaterSize.L };

        public static FloaterPick Roll(System.Random random)
        {
            var pick = RollLook(random);
            pick.RollMotion(random);
            return pick;
        }

        public string SpriteId(string shapeArtId)
        {
            var species = Species == PetSpecies.Cat ? "cat" : "dog";
            return $"fl_{species}_{shapeArtId}_{Poses[PoseIndex]}_{Size}";
        }

        private static FloaterPick RollLook(System.Random random) => new FloaterPick
        {
            Species = random.NextDouble() < 0.5 ? PetSpecies.Cat : PetSpecies.Dog,
            ShapeIndex = random.Next(ShapeCount),
            ColorwayIndex = random.Next(ColorwayCount),
            Size = SizeWeights[random.Next(SizeWeights.Length)],
            PoseIndex = random.Next(Poses.Length)
        };

        private void RollMotion(System.Random random)
        {
            QuarterTurns = Poses[PoseIndex] == TumblePose ? 0 : random.Next(4);
            Mirror = random.NextDouble() < 0.5;
            InBubble = random.NextDouble() < 0.2;
            Phase = (float)random.NextDouble();
        }
    }
}
