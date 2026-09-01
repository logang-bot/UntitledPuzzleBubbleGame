using System;
using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    /// Single shared owner of the board's GridModel, camera-fit geometry, and
    /// bounds. Replaces the per-component cols/cellWidth duplication between
    /// ShooterController and GridDebugRenderer from Milestone 1/2 — see
    /// docs/features/core-gameplay/firing-and-snapping.md.
    /// </summary>
    public class GameBoard : MonoBehaviour
    {
        [SerializeField] private int cols = 8;
        [SerializeField] private float cellWidth = 1f;
        [SerializeField] private int filledRows = 4;

        public event Action<int, int> OnBubblePlaced;

        public GridModel Grid { get; private set; }
        public Shooter.BoardBounds Bounds { get; private set; }
        public Vector2 ShooterOrigin { get; private set; }
        public float CellWidth => cellWidth;

        private void Awake()
        {
            var camera = Camera.main;
            var rows = FitCameraAndComputeRows(camera);
            Grid = new GridModel(rows, cols, cellWidth);
            Bounds = Shooter.BoardBoundsCalculator.Compute(camera.transform.position, cols * cellWidth, camera.orthographicSize);
            ShooterOrigin = new Vector2(camera.transform.position.x, transform.position.y);
            FillWithRandomBubbles(rows);
        }

        public void PlaceBubble(int row, int col, BubbleColor color)
        {
            Grid.PlaceBubble(row, col, color);
            OnBubblePlaced?.Invoke(row, col);
        }

        private int FitCameraAndComputeRows(Camera camera)
        {
            var boardWidth = cols * cellWidth;
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

        private void FillWithRandomBubbles(int rows)
        {
            var filled = Mathf.Min(filledRows, rows);
            var startRow = rows - filled;
            for (var row = startRow; row < rows; row++)
                for (var col = 0; col < cols; col++)
                    Grid.PlaceBubble(row, col, BubbleColorPalette.Random());
        }
    }
}
