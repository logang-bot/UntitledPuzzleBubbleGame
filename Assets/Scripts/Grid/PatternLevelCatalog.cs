using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    /// Per-level curated pattern recipes (levels 1-5) plus the shared
    /// pattern-generation tunables. See
    /// docs/features/level-content/specs/2026-09-11-curated-levels-and-transitions-design.md.
    /// </summary>
    [CreateAssetMenu(fileName = "PatternLevelCatalog", menuName = "Game/Pattern Level Catalog")]
    public sealed class PatternLevelCatalog : ScriptableObject
    {
        [SerializeField] private List<LevelPatternPlan> plans = new();
        [SerializeField, Min(1)] private int hexBlobRadius = 2;
        [SerializeField, Min(0)] private int hexBlobGapCells = 1;
        [SerializeField, Min(1)] private int stripeWidth = 3;
        [SerializeField, Min(0)] private int regionGapRows = 1;

        public PatternGenerationSettings Settings => new PatternGenerationSettings
        {
            HexBlobRadius = hexBlobRadius,
            HexBlobGapCells = hexBlobGapCells,
            StripeWidth = stripeWidth,
            RegionGapRows = regionGapRows,
        };

        public bool TryGetPlan(int levelNumber, out LevelPatternPlan plan)
        {
            plan = plans.FirstOrDefault(p => p.LevelNumber == levelNumber);
            return plan != null;
        }
    }
}
