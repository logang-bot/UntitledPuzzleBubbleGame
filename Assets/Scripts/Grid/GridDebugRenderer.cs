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
        [SerializeField] private int cols = 8;
        [SerializeField] private float cellWidth = 1f;
        [SerializeField] private int filledRows = 4;

        private void Start()
        {
            var rows = FitCameraAndComputeRows();
            var grid = new GridModel(rows, cols, cellWidth);
            FillWithRandomBubbles(grid, rows);
            RenderBubbles(grid, rows);
        }

        private int FitCameraAndComputeRows()
        {
            var boardWidth = cols * cellWidth;
            var camera = Camera.main;
            camera.orthographicSize = PlayfieldSizer.OrthographicSizeForWidth(boardWidth, Screen.width, Screen.height);
            PositionBoard(camera);
            return PlayfieldSizer.RowsForWorldHeight(camera.orthographicSize * 2f, cellWidth);
        }

        private void PositionBoard(Camera camera)
        {
            var x = camera.transform.position.x - (cols - 1) * cellWidth * 0.5f;
            var y = camera.transform.position.y - camera.orthographicSize + cellWidth * 0.5f;
            transform.position = new Vector3(x, y, transform.position.z);
        }

        private void FillWithRandomBubbles(GridModel grid, int rows)
        {
            var filled = Mathf.Min(filledRows, rows);
            var startRow = rows - filled;
            for (var row = startRow; row < rows; row++)
                for (var col = 0; col < cols; col++)
                    grid.PlaceBubble(row, col, RandomColor());
        }

        private static BubbleColor RandomColor()
        {
            var values = (BubbleColor[])Enum.GetValues(typeof(BubbleColor));
            return values[UnityEngine.Random.Range(0, values.Length)];
        }

        private void RenderBubbles(GridModel grid, int rows)
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
