namespace Game.Battle
{
    /// <summary>
    /// Accumulates fractional attack value and releases only whole rows,
    /// since GridModel.PushRowsDown only moves whole rows — the remainder
    /// carries forward rather than being lost. See
    /// docs/features/battle-mode/specs/2026-09-15-simple-attack-battle-mode-design.md.
    /// </summary>
    public class PendingRowsMeter
    {
        private float _accumulated;

        public void Add(float amount) => _accumulated += amount;

        public void Reset() => _accumulated = 0f;

        public int ConsumeWholeRows()
        {
            var wholeRows = (int)_accumulated;
            _accumulated -= wholeRows;
            return wholeRows;
        }
    }
}
