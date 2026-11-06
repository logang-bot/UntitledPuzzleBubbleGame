using System;
using System.Collections.Generic;
using Game.Grid;
using UnityEngine;

namespace Game.Battle
{
    /// <summary>Feeds one battle side's board and shooter events into its MatchStats.</summary>
    public sealed class MatchStatsTracker : IDisposable
    {
        public MatchStats Stats { get; } = new();

        private readonly BattlePlayerSide _side;

        public MatchStatsTracker(BattlePlayerSide side)
        {
            _side = side;
            side.GameBoard.OnBubblesPopped += HandlePopped;
            side.GameBoard.OnClusterDropped += HandleDropped;
            side.ShooterController.OnFireRequested += HandleFired;
        }

        public void Dispose()
        {
            _side.GameBoard.OnBubblesPopped -= HandlePopped;
            _side.GameBoard.OnClusterDropped -= HandleDropped;
            _side.ShooterController.OnFireRequested -= HandleFired;
        }

        private void HandlePopped(IReadOnlyCollection<(int Row, int Col)> cells, BubbleColor color) => Stats.RecordPopped(cells.Count);

        private void HandleDropped(IReadOnlyCollection<(int Row, int Col)> cells) => Stats.RecordDropped(cells.Count);

        private void HandleFired(Vector2 origin, float angleDegrees) => Stats.RecordShot();
    }
}
