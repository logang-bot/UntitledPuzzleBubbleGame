using System.Collections.Generic;
using System.Linq;
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

        // True while a level's staggered build-in animation is still playing
        // (see BubbleSpawnAnimator) - ShooterBuildInGate locks firing until it ends.
        public bool IsBuildingIn => _buildInRemaining > 0f;

        private readonly Dictionary<(int Row, int Col), GameObject> _bubbles = new();
        private readonly List<GameObject> _fallingBubbles = new();
        private Sprite _sprite;
        private float _buildInRemaining;
        private const float PerRowSpawnDelaySeconds = 0.05f;

        private void Start()
        {
            _sprite = CircleSpriteFactory.CreateWhiteCircle();
            SpawnAllAnimated();
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

        private void Update()
        {
            if (_buildInRemaining > 0f) _buildInRemaining -= Time.deltaTime;
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
            _fallingBubbles.RemoveAll(bubble => bubble == null);
            foreach (var cell in cells)
                if (_bubbles.Remove(cell, out var bubble))
                    StartFalling(bubble);
        }

        private void StartFalling(GameObject bubble)
        {
            bubble.AddComponent<FallingBubble>();
            _fallingBubbles.Add(bubble);
        }

        private void OnRowPushedDown(bool wasLastRowOccupied)
        {
            RebuildAllInstant();
        }

        private void OnLevelLoaded(int levelNumber)
        {
            ClearFallingBubbles();
            ClearAllBubbles();
            SpawnAllAnimated();
        }

        private void ClearFallingBubbles()
        {
            foreach (var bubble in _fallingBubbles)
                if (bubble != null) Destroy(bubble);
            _fallingBubbles.Clear();
        }

        private void RebuildAllInstant()
        {
            ClearAllBubbles();
            foreach (var cell in gameBoard.Grid.OccupiedCells())
                SpawnBubble(cell);
        }

        private void ClearAllBubbles()
        {
            foreach (var bubble in _bubbles.Values)
                Destroy(bubble);
            _bubbles.Clear();
        }

        private void SpawnAllAnimated()
        {
            foreach (var cell in gameBoard.Grid.OccupiedCells())
                SpawnAnimatedBubble(cell);
            StartBuildInTimer();
        }

        private void SpawnAnimatedBubble((int Row, int Col) cell)
        {
            var bubble = SpawnBubble(cell);
            var animator = bubble.AddComponent<BubbleSpawnAnimator>();
            animator.Configure(cell.Row * PerRowSpawnDelaySeconds);
        }

        private void StartBuildInTimer()
        {
            var lastRow = gameBoard.Grid.OccupiedCells().Select(cell => cell.Row).DefaultIfEmpty(0).Max();
            _buildInRemaining = lastRow * PerRowSpawnDelaySeconds + BubbleSpawnAnimator.ScaleInDurationSeconds;
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
