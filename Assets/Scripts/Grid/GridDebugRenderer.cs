using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    /// Event-driven bubble rendering: draws whatever GameBoard already holds at
    /// Start, then spawns one more sprite each time OnBubblePlaced fires. Still
    /// the Milestone-1 plain-circle debug visuals, and no sprite pooling yet —
    /// nothing removes bubbles until Milestone 4/5 (see
    /// docs/features/core-gameplay/hex-grid.md).
    /// </summary>
    public class GridDebugRenderer : MonoBehaviour
    {
        [SerializeField] private GameBoard gameBoard;

        private Sprite _sprite;

        private void Start()
        {
            _sprite = CircleSpriteFactory.CreateWhiteCircle();
            foreach (var cell in gameBoard.Grid.OccupiedCells())
                SpawnBubble(cell);
            gameBoard.OnBubblePlaced += OnBubblePlaced;
        }

        private void OnDestroy()
        {
            gameBoard.OnBubblePlaced -= OnBubblePlaced;
        }

        private void OnBubblePlaced(int row, int col)
        {
            SpawnBubble((row, col));
        }

        private void SpawnBubble((int Row, int Col) cell)
        {
            var bubble = new GameObject($"Bubble_{cell.Row}_{cell.Col}");
            bubble.transform.SetParent(gameBoard.transform);
            bubble.transform.localPosition = gameBoard.Grid.GetWorldPosition(cell.Row, cell.Col);
            var spriteRenderer = bubble.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = _sprite;
            spriteRenderer.color = BubbleColorPalette.ToColor(gameBoard.Grid.GetColor(cell.Row, cell.Col));
        }
    }
}
