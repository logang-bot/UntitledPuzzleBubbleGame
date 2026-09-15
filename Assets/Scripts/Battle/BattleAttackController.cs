using System.Collections.Generic;
using Game.Grid;
using UnityEngine;

namespace Game.Battle
{
    /// <summary>
    /// Turns my own board's pops/drops into rows pushed onto the opponent's
    /// board — the one new cross-board wiring this feature needs; every
    /// other event stays single-board, same as solo mode. See
    /// docs/features/battle-mode/specs/2026-09-15-simple-attack-battle-mode-design.md.
    /// </summary>
    public class BattleAttackController : MonoBehaviour
    {
        [SerializeField] private GameBoard ownBoard;
        [SerializeField] private GameBoard opponentBoard;
        [SerializeField] private BattleAttackConfig attackConfig;

        private readonly PendingRowsMeter _meter = new();

        private void Start()
        {
            ownBoard.OnBubblesPopped += HandlePopped;
            ownBoard.OnClusterDropped += HandleDropped;
        }

        private void OnDestroy()
        {
            ownBoard.OnBubblesPopped -= HandlePopped;
            ownBoard.OnClusterDropped -= HandleDropped;
        }

        public void ResetForRematch() => _meter.Reset();

        private void HandlePopped(IReadOnlyCollection<(int Row, int Col)> cells, BubbleColor color)
        {
            if (!enabled) return;
            BankAttack(attackConfig.AttackForPop(cells.Count));
        }

        private void HandleDropped(IReadOnlyCollection<(int Row, int Col)> cells)
        {
            if (!enabled) return;
            BankAttack(attackConfig.AttackForDrop(cells.Count));
        }

        private void BankAttack(float amount)
        {
            _meter.Add(amount);
            var rowsToSend = _meter.ConsumeWholeRows();
            for (var i = 0; i < rowsToSend && opponentBoard.Grid.RowsPushed < opponentBoard.Grid.Rows; i++)
                opponentBoard.PushRowDown();
        }
    }
}
