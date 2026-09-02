using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    /// Per-level difficulty curve, resolved via ForLevel. Every ramp here is a
    /// deliberately rough placeholder linear curve — see
    /// docs/features/core-gameplay/level-generation.md's open question on
    /// tuning; needs playtesting, not further hand-tuning up front.
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultyCurveConfig", menuName = "Game/Difficulty Curve Config")]
    public sealed class DifficultyCurveConfig : ScriptableObject
    {
        [SerializeField, Min(2)] private int startColorCount = 3;
        [SerializeField, Min(2)] private int maxColorCount = 6;
        [SerializeField, Min(1)] private int levelsPerColorIncrease = 5;

        [SerializeField, Range(0f, 1f)] private float startDensity = 0.55f;
        [SerializeField, Range(0f, 1f)] private float maxDensity = 0.85f;
        [SerializeField] private float densityIncreasePerLevel = 0.02f;

        [SerializeField, Min(0)] private int startHeadroomRows = 6;
        [SerializeField, Min(0)] private int minHeadroomRows = 3;
        [SerializeField, Min(1)] private int levelsPerHeadroomDecrease = 3;

        [SerializeField, Min(0f)] private float startCeilingIntervalSeconds = 20f;
        [SerializeField, Min(0f)] private float minCeilingIntervalSeconds = 8f;
        [SerializeField] private float ceilingIntervalDecreasePerLevel = 0.75f;

        public DifficultyConfig ForLevel(int levelNumber) => new DifficultyConfig
        {
            ColorCount = ColorCountForLevel(levelNumber),
            Density = DensityForLevel(levelNumber),
            HeadroomRows = HeadroomRowsForLevel(levelNumber),
            CeilingDropIntervalSeconds = CeilingIntervalForLevel(levelNumber),
        };

        private int ColorCountForLevel(int levelNumber) =>
            Mathf.Clamp(startColorCount + (levelNumber - 1) / levelsPerColorIncrease, startColorCount, maxColorCount);

        private float DensityForLevel(int levelNumber) =>
            Mathf.Clamp(startDensity + densityIncreasePerLevel * (levelNumber - 1), startDensity, maxDensity);

        private int HeadroomRowsForLevel(int levelNumber) =>
            Mathf.Max(startHeadroomRows - (levelNumber - 1) / levelsPerHeadroomDecrease, minHeadroomRows);

        private float CeilingIntervalForLevel(int levelNumber) =>
            Mathf.Max(startCeilingIntervalSeconds - ceilingIntervalDecreasePerLevel * (levelNumber - 1), minCeilingIntervalSeconds);
    }
}
