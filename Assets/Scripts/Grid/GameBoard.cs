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
        [SerializeField] private float ceilingHeight = 1f;
        [SerializeField] private int levelNumber = 1;
        [SerializeField] private DifficultyCurveConfig difficultyCurve;

        public event Action<int, int> OnBubblePlaced;
        public event Action<IReadOnlyCollection<(int Row, int Col)>, BubbleColor> OnBubblesPopped;
        public event Action<IReadOnlyCollection<(int Row, int Col)>> OnClusterDropped;
        public event Action<bool> OnRowPushedDown;
        public event Action<int> OnLevelLoaded;

        public GridModel Grid { get; private set; }
        public Shooter.BoardBounds Bounds { get; private set; }
        public Vector2 ShooterOrigin { get; private set; }
        public float CellWidth => cellWidth;
        public int Cols => cols;
        public float CeilingHeight => ceilingHeight;
        public DifficultyConfig CurrentDifficulty { get; private set; }
        public int LevelNumber => levelNumber;

        private int _rows;
        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
            var boardWidth = HexGridMath.BoardWidthWithOffsetMargin(cols, cellWidth);
            _rows = FitCameraAndComputeRows(_camera, boardWidth);
            ShooterOrigin = new Vector2(_camera.transform.position.x, _camera.transform.position.y - _camera.orthographicSize + cellWidth * 0.5f);
            LoadLevel(levelNumber);
        }

        public void LoadLevel(int newLevelNumber)
        {
            levelNumber = newLevelNumber;
            CurrentDifficulty = difficultyCurve.ForLevel(levelNumber);
            Grid = LevelGenerator.Generate(new GridModel(_rows, cols, cellWidth), levelNumber, CurrentDifficulty);
            RecomputeBounds();
            OnLevelLoaded?.Invoke(levelNumber);
        }

        // The wall's advance grows the reserved ceiling band by one row height
        // per push (see CeilingRenderer), so an unobstructed shot must stop
        // that much sooner too - otherwise it would sail past the wall's
        // actual current position into space that no longer exists.
        private void RecomputeBounds()
        {
            var boardWidth = HexGridMath.BoardWidthWithOffsetMargin(cols, cellWidth);
            var advancedCeilingHeight = ceilingHeight + Grid.RowsPushed * HexGridMath.RowHeight(cellWidth);
            Bounds = Shooter.BoardBoundsCalculator.Compute(_camera.transform.position, boardWidth, _camera.orthographicSize, advancedCeilingHeight);
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

        public void PushRowDown()
        {
            Grid.PushRowsDown(out var wasLastRowOccupied);
            RecomputeBounds();
            OnRowPushedDown?.Invoke(wasLastRowOccupied);
        }

        private void ClearCells(IReadOnlyCollection<(int Row, int Col)> cells)
        {
            foreach (var cell in cells) Grid.ClearCell(cell.Row, cell.Col);
        }

        private int FitCameraAndComputeRows(Camera camera, float boardWidth)
        {
            camera.orthographicSize = PlayfieldSizer.OrthographicSizeForWidth(boardWidth, Screen.width, Screen.height);
            PositionBoard(camera);
            var availableHeight = camera.orthographicSize * 2f - ceilingHeight;
            return PlayfieldSizer.RowsForWorldHeight(availableHeight, cellWidth);
        }

        private void PositionBoard(Camera camera)
        {
            // Anchored so row 0's top edge (local y = cellWidth * 0.5, see
            // GridModel.GetWorldPosition) touches the bottom of the reserved
            // ceilingHeight band at the screen's actual top edge - row 0 itself
            // sits ceilingHeight below that, leaving room for CeilingRenderer's
            // band rather than row 0 sharing the screen's top edge directly.
            // x is offset by HexGridMath.BoardOriginXOffset rather than the plain column
            // center, since odd rows' half-cell shift makes the occupied footprint
            // wider than cols*cellWidth (see BoardWidthWithOffsetMargin) and off-center.
            var x = camera.transform.position.x - HexGridMath.BoardOriginXOffset(cols, cellWidth);
            var y = camera.transform.position.y + camera.orthographicSize - ceilingHeight - cellWidth * 0.5f;
            transform.position = new Vector3(x, y, transform.position.z);
        }
    }
}
