namespace Game.Menu
{
    /// <summary>
    /// Sprite pools for the drifting cosmos slots: the big slot cycles the
    /// three hero galaxies in order, the small slots pick any of the rest.
    /// </summary>
    public static class CosmosSprites
    {
        public const string Comet = "cos_comet";
        public const string ShootingStar = "cos_shooting_star";

        public static readonly string[] Big = { "cos_milkyway", "cos_spiral_blue", "cos_barred" };

        public static readonly string[] Small =
        {
            "cos_spiral_violet", "cos_ring", "cos_edge_on", "cos_elliptical", "cos_planet_ringed",
            "cos_planet_earth", "cos_planet_ice", "cos_planet_gas", "cos_moon", "cos_planet_red", Comet
        };

        public static string BigForCycle(int cycle) => Big[(cycle % Big.Length + Big.Length) % Big.Length];
    }
}
