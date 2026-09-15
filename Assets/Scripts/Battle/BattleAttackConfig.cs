using UnityEngine;

namespace Game.Battle
{
    /// <summary>
    /// Match size / cascade-drop count to attack value, deliberately
    /// decoupled from ScoreCalculator's solo-mode formula so the two can be
    /// tuned independently. Pop scales quadratically (rewards bigger
    /// matches disproportionately, mirroring ScoreCalculator's own pop
    /// formula shape); drops scale linearly. Untuned placeholder constants,
    /// same caveat as DifficultyCurveConfig's keyframes — see
    /// docs/features/battle-mode/specs/2026-09-15-simple-attack-battle-mode-design.md.
    /// </summary>
    [CreateAssetMenu(fileName = "BattleAttackConfig", menuName = "Game/Battle Attack Config")]
    public sealed class BattleAttackConfig : ScriptableObject
    {
        [SerializeField] private float popAttackPerBubble = 0.05f;
        [SerializeField] private float dropAttackPerBubble = 0.15f;

        public float AttackForPop(int bubbleCount) => popAttackPerBubble * bubbleCount * (bubbleCount - 1);
        public float AttackForDrop(int bubbleCount) => dropAttackPerBubble * bubbleCount;
    }
}
