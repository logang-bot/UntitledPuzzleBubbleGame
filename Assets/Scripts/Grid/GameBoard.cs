using System;
using System.Collections.Generic;
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
        public event Action<IReadOnlyCollection<(int Row, int Col)>, BubbleColor> OnBubblesPopped;
        public event Action<IReadOnlyCollection<(int Row, int Col)>> OnClusterDropped;

        public GridModel Grid { get; private set; }
        public Shooter.BoardBounds Bounds { get; private set; }
        public Vector2 ShooterOrigin { get; private set; }
        public float CellWidth => cellWidth;

        private void Awake()
        {
            var camera = Camera.main;
            var boardWidth = HexGridMath.BoardWidthWithOffsetMargin(cols, cellWidth);
            var rows = FitCameraAndComputeRows(camera, boardWidth);
            Grid = new GridModel(rows, cols, cellWidth);
            Bounds = Shooter.BoardBoundsCalculator.Compute(camera.transform.position, boardWidth, camera.orthographicSize);
            ShooterOrigin = new Vector2(camera.transform.position.x, camera.transform.position.y - camera.orthographicSize + cellWidth * 0.5f);
            FillWithRandomBubbles(rows);
        }

        public void PlaceBubble(int row, int col, BubbleColor color)
        {
            Grid.PlaceBubble(row, col, color);
            OnBubblePlaced?.Invoke(row, col);
        }

        public void PopCells(IReadOnlyCollection<(int Row, int Col)> cells, BubbleColor color)
        {
            ClearCells(cells);
            OnBubblesPopped?.Invoke(cells, color);
        }

        public void DropCells(IReadOnlyCollection<(int Row, int Col)> cells)
        {
            ClearCells(cells);
            OnClusterDropped?.Invoke(cells);
        }

        private void ClearCells(IReadOnlyCollection<(int Row, int Col)> cells)
        {
            foreach (var cell in cells) Grid.ClearCell(cell.Row, cell.Col);
        }

        private int FitCameraAndComputeRows(Camera camera, float boardWidth)
        {
            camera.orthographicSize = PlayfieldSizer.OrthographicSizeForWidth(boardWidth, Screen.width, Screen.height);
            PositionBoard(camera);
            return PlayfieldSizer.RowsForWorldHeight(camera.orthographicSize * 2f, cellWidth);
        }

        private void PositionBoard(Camera camera)
        {
            // Anchored at the ceiling (top of screen): row 0's local y is 0 (see
            // GridModel.GetWorldPosition), so this transform.position IS row 0's world position.
            // x is offset by HexGridMath.BoardOriginXOffset rather than the plain column
            // center, since odd rows' half-cell shift makes the occupied footprint
            // wider than cols*cellWidth (see BoardWidthWithOffsetMargin) and off-center.
            var x = camera.transform.position.x - HexGridMath.BoardOriginXOffset(cols, cellWidth);
            var y = camera.transform.position.y + camera.orthographicSize - cellWidth * 0.5f;
            transform.position = new Vector3(x, y, transform.position.z);
        }

        private void FillWithRandomBubbles(int rows)
        {
            var filled = Mathf.Min(filledRows, rows);
            for (var row = 0; row < filled; row++)
                for (var col = 0; col < cols; col++)
                    Grid.PlaceBubble(row, col, BubbleColorPalette.Random());
        }
    }
}
