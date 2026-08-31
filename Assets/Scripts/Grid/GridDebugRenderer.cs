using System;
using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    /// Milestone 1 debug view: fills a GridModel with a few rows of random
    /// bubbles and renders them as plain circles. Stand-in for the real
    /// grid view until placeholder/real art and level generation exist.
    /// </summary>
    public class GridDebugRenderer : MonoBehaviour
    {
        [SerializeField] private int rows = 8;
        [SerializeField] private int cols = 8;
        [SerializeField] private float cellWidth = 1f;
        [SerializeField] private int filledRows = 4;

        private void Start()
        {
            var grid = new GridModel(rows, cols, cellWidth);
            FillWithRandomBubbles(grid);
            RenderBubbles(grid);
        }

        private void FillWithRandomBubbles(GridModel grid)
        {
            for (var row = 0; row < filledRows; row++)
                for (var col = 0; col < cols; col++)
                    grid.PlaceBubble(row, col, RandomColor());
        }

        private static BubbleColor RandomColor()
        {
            var values = (BubbleColor[])Enum.GetValues(typeof(BubbleColor));
            return values[UnityEngine.Random.Range(0, values.Length)];
        }

        private void RenderBubbles(GridModel grid)
        {
            var sprite = CircleSpriteFactory.CreateWhiteCircle();
            for (var row = 0; row < rows; row++)
                for (var col = 0; col < cols; col++)
                    if (grid.IsOccupied(row, col))
                        SpawnBubble(grid, sprite, (row, col));
        }

        private void SpawnBubble(GridModel grid, Sprite sprite, (int Row, int Col) cell)
        {
            var bubble = new GameObject($"Bubble_{cell.Row}_{cell.Col}");
            bubble.transform.SetParent(transform);
            bubble.transform.localPosition = grid.GetWorldPosition(cell.Row, cell.Col);
            var spriteRenderer = bubble.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = BubbleColorPalette.ToColor(grid.GetColor(cell.Row, cell.Col));
        }
    }
}
