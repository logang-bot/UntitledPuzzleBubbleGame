using System;
using System.Collections.Generic;
using Game.Grid;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>
    /// Accumulates score from board pop/drop events via ScoreCalculator.
    /// Persists across a level-complete -> next-level transition; only
    /// reset explicitly by GameStateManager on retry, so a game-over screen
    /// can still show the failed run's final score before it clears. See
    /// docs/features/core-gameplay/hud-and-level-flow.md.
    /// </summary>
    public class ScoreTracker : MonoBehaviour
    {
        [SerializeField] private GameBoard gameBoard;

        public event Action<int> OnScoreChanged;
        public int Score { get; private set; }

        private void Start()
        {
            gameBoard.OnBubblesPopped += HandlePopped;
            gameBoard.OnClusterDropped += HandleDropped;
        }

        private void OnDestroy()
        {
            gameBoard.OnBubblesPopped -= HandlePopped;
            gameBoard.OnClusterDropped -= HandleDropped;
        }

        public void ResetScore()
        {
            Score = 0;
            OnScoreChanged?.Invoke(Score);
        }

        private void HandlePopped(IReadOnlyCollection<(int Row, int Col)> cells, BubbleColor color)
        {
            AddScore(ScoreCalculator.PointsForPop(cells.Count));
        }

        private void HandleDropped(IReadOnlyCollection<(int Row, int Col)> cells)
        {
            AddScore(ScoreCalculator.PointsForDrop(cells.Count));
        }

        private void AddScore(int delta)
        {
            Score += delta;
            OnScoreChanged?.Invoke(Score);
        }
    }
}
