using System;
using System.Collections.Generic;
using Game.Grid;
using UnityEngine;

namespace Game.Battle
{
    /// <summary>
    /// Reuses solo mode's exact win/loss signals (GameBoard.OnRowPushedDown,
    /// GridModel.IsEmpty — see GameStateManager), scoped to one side and
    /// without the shot/ceiling timer machinery those checks came bundled
    /// with in solo mode. See
    /// docs/features/battle-mode/specs/2026-09-15-simple-attack-battle-mode-design.md.
    /// </summary>
    public class BattleSideOutcome : MonoBehaviour
    {
        [SerializeField] private GameBoard gameBoard;

        public event Action<BattleEndReason> OnSideEnded;

        private bool _hasEnded;

        private void Start()
        {
            gameBoard.OnRowPushedDown += HandleRowPushedDown;
            gameBoard.OnBubblesPopped += HandlePopped;
            gameBoard.OnClusterDropped += HandleDropped;
        }

        private void OnDestroy()
        {
            gameBoard.OnRowPushedDown -= HandleRowPushedDown;
            gameBoard.OnBubblesPopped -= HandlePopped;
            gameBoard.OnClusterDropped -= HandleDropped;
        }

        public void ResetForRematch() => _hasEnded = false;

        private void HandleRowPushedDown(bool wasLastRowOccupied)
        {
            if (wasLastRowOccupied) RaiseEnded(BattleEndReason.WallReachedLine);
        }

        private void HandlePopped(IReadOnlyCollection<(int Row, int Col)> cells, BubbleColor color) => CheckCleared();
        private void HandleDropped(IReadOnlyCollection<(int Row, int Col)> cells) => CheckCleared();

        private void CheckCleared()
        {
            if (gameBoard.Grid.IsEmpty) RaiseEnded(BattleEndReason.Cleared);
        }

        private void RaiseEnded(BattleEndReason reason)
        {
            if (_hasEnded) return;
            _hasEnded = true;
            OnSideEnded?.Invoke(reason);
        }
    }
}
