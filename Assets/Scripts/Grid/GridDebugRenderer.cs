using System.Collections.Generic;
using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    /// Event-driven bubble rendering: draws whatever GameBoard already holds at
    /// Start, then spawns one more sprite each time OnBubblePlaced fires, and
    /// removes/animates sprites on OnBubblesPopped/OnClusterDropped. Still the
    /// Milestone-1 plain-circle debug visuals (see
    /// docs/features/core-gameplay/hex-grid.md).
    /// </summary>
    public class GridDebugRenderer : MonoBehaviour
    {
        [SerializeField] private GameBoard gameBoard;

        private readonly Dictionary<(int Row, int Col), GameObject> _bubbles = new();
        private Sprite _sprite;
        private const float PerRowSpawnDelaySeconds = 0.05f;

        private void Start()
        {
            _sprite = CircleSpriteFactory.CreateWhiteCircle();
            foreach (var cell in gameBoard.Grid.OccupiedCells())
                SpawnAnimatedBubble(cell);
            gameBoard.OnBubblePlaced += OnBubblePlaced;
            gameBoard.OnBubblesPopped += OnBubblesPopped;
            gameBoard.OnClusterDropped += OnClusterDropped;
            gameBoard.OnRowPushedDown += OnRowPushedDown;
            gameBoard.OnLevelLoaded += OnLevelLoaded;
        }

        private void OnDestroy()
        {
            gameBoard.OnBubblePlaced -= OnBubblePlaced;
            gameBoard.OnBubblesPopped -= OnBubblesPopped;
            gameBoard.OnClusterDropped -= OnClusterDropped;
            gameBoard.OnRowPushedDown -= OnRowPushedDown;
            gameBoard.OnLevelLoaded -= OnLevelLoaded;
        }

        private void OnBubblePlaced(int row, int col)
        {
            SpawnBubble((row, col));
        }

        private void OnBubblesPopped(IReadOnlyCollection<(int Row, int Col)> cells, BubbleColor color)
        {
            foreach (var cell in cells)
                if (_bubbles.Remove(cell, out var bubble))
                    Destroy(bubble);
        }

        private void OnClusterDropped(IReadOnlyCollection<(int Row, int Col)> cells)
        {
            foreach (var cell in cells)
                if (_bubbles.Remove(cell, out var bubble))
                    bubble.AddComponent<FallingBubble>();
        }

        private void OnRowPushedDown(bool wasLastRowOccupied)
        {
            RebuildAllInstant();
        }

        private void OnLevelLoaded(int levelNumber)
        {
            RebuildAllAnimated();
        }

        private void RebuildAllInstant()
        {
            ClearAllBubbles();
            foreach (var cell in gameBoard.Grid.OccupiedCells())
                SpawnBubble(cell);
        }

        private void RebuildAllAnimated()
        {
            ClearAllBubbles();
            foreach (var cell in gameBoard.Grid.OccupiedCells())
                SpawnAnimatedBubble(cell);
        }

        private void ClearAllBubbles()
        {
            foreach (var bubble in _bubbles.Values)
                Destroy(bubble);
            _bubbles.Clear();
        }

        private void SpawnAnimatedBubble((int Row, int Col) cell)
        {
            var bubble = SpawnBubble(cell);
            var animator = bubble.AddComponent<BubbleSpawnAnimator>();
            animator.Configure(cell.Row * PerRowSpawnDelaySeconds);
        }

        private GameObject SpawnBubble((int Row, int Col) cell)
        {
            var bubble = new GameObject($"Bubble_{cell.Row}_{cell.Col}");
            bubble.transform.SetParent(gameBoard.transform);
            bubble.transform.localPosition = gameBoard.Grid.GetWorldPosition(cell.Row, cell.Col);
            var spriteRenderer = bubble.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = _sprite;
            spriteRenderer.color = BubbleColorPalette.ToColor(gameBoard.Grid.GetColor(cell.Row, cell.Col));
            _bubbles[cell] = bubble;
            return bubble;
        }
    }
}
