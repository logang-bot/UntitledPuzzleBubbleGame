using UnityEngine;

namespace Game.Menu
{
    /// <summary>
    /// The drifting cosmos of the main menu (motion.json "cosmos"): a hero
    /// slot alternating the two big spirals through the middle band, three
    /// small slots of planets, the teal spiral and the comet, and the
    /// shooting star's timing. Galaxies spin and glow; the comet flies sideways.
    /// </summary>
    public static class CosmosSprites
    {
        public const string Comet = "comet";
        public const string ShootingStar = "shooting_star";
        public const string GalaxyGlow = "galaxy_glow";
        public const float GlowScalePerPt = 1.3f / 120f;
        public const float GlowFlattening = 0.7f;
        public const float ShootingStarEvery = 6f;
        public const float ShootingStarLife = 0.7f;
        public const float ShootingStarSpeed = 420f;
        public static readonly Vector2 ShootingStarAngles = new Vector2(20f, 35f);

        private const string Ringed = "planet_ringed";
        private const string Teal = "planet_teal";
        private const string Rose = "planet_rose";
        private const string Moon = "planet_moon";
        private const string TealSpiral = "cos_galaxy_teal";

        public static readonly CosmosSlotSpec[] Slots =
        {
            new CosmosSlotSpec
            {
                Slot = new SpawnSlot { Id = 0, Period = 150f, Offset = 0.42f, VisibleShare = 0.94f },
                Pool = new[] { "cos_galaxy_hero", "cos_galaxy_rose" },
                Band = new Vector2(200f, 420f),
                CyclesInOrder = true
            },
            Small(new SpawnSlot { Id = 1, Period = 90f, Offset = 0.12f, VisibleShare = 0.8f }, new[] { Ringed, Teal, Rose, Moon, TealSpiral, Comet }),
            Small(new SpawnSlot { Id = 2, Period = 110f, Offset = 0.6f, VisibleShare = 0.8f }, new[] { Ringed, Teal, Rose, Moon, TealSpiral }),
            Small(new SpawnSlot { Id = 3, Period = 75f, Offset = 0.86f, VisibleShare = 0.8f }, new[] { Moon, Rose, Comet })
        };

        public static bool IsGalaxy(string spriteId) => spriteId.StartsWith("cos_");

        private static CosmosSlotSpec Small(SpawnSlot slot, string[] pool) => new CosmosSlotSpec
        {
            Slot = slot,
            Pool = pool,
            Band = CrossingRoute.FullHeight
        };
    }
}
