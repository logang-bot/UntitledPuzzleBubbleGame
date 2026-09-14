using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    /// Per-level difficulty curve, resolved via ForLevel. Each knob is an
    /// Inspector-editable AnimationCurve keyed by level number — deliberately
    /// rough placeholder keyframes, see
    /// docs/features/core-gameplay/level-generation.md and
    /// docs/features/level-content/specs/2026-09-11-curated-levels-and-transitions-design.md.
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultyCurveConfig", menuName = "Game/Difficulty Curve Config")]
    public sealed class DifficultyCurveConfig : ScriptableObject
    {
        [SerializeField] private AnimationCurve colorCountCurve = AnimationCurve.Linear(1, 3, 30, 6);
        [SerializeField] private AnimationCurve densityCurve = DefaultDensityCurve();
        [SerializeField] private AnimationCurve headroomRowsCurve = AnimationCurve.Linear(1, 9, 10, 3);
        [SerializeField] private AnimationCurve ceilingIntervalCurve = AnimationCurve.Linear(1, 20, 20, 8);

        public DifficultyConfig ForLevel(int levelNumber) => new DifficultyConfig
        {
            ColorCount = Mathf.Clamp(Mathf.RoundToInt(colorCountCurve.Evaluate(levelNumber)), 2, BubbleColorPalette.AllColors.Length),
            Density = densityCurve.Evaluate(levelNumber),
            HeadroomRows = Mathf.Max(0, Mathf.RoundToInt(headroomRowsCurve.Evaluate(levelNumber))),
            CeilingDropIntervalSeconds = Mathf.Max(1f, ceilingIntervalCurve.Evaluate(levelNumber)),
        };

        private static AnimationCurve DefaultDensityCurve()
        {
            var curve = new AnimationCurve();
            curve.AddKey(1, 0.35f);
            curve.AddKey(2, 0.55f);
            curve.AddKey(6, 0.8f);
            curve.AddKey(15, 0.85f);
            return curve;
        }
    }
}
