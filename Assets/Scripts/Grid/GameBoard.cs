using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    /// Single shared owner of the board's GridModel, camera-fit geometry, and
    /// bounds. Replaces the per-component cols/cellWidth duplication between
    /// ShooterController and the board renderer from Milestone 1/2 — see
    /// docs/features/core-gameplay/firing-and-snapping.md.
    /// </summary>
    public class GameBoard : MonoBehaviour
    {
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
        public float CeilingHeight => Frame.FirstRowTop;
        public PlayfieldFrame Frame { get; private set; }
        public DifficultyConfig CurrentDifficulty { get; private set; }
        public int LevelNumber => levelNumber;
        public PlayfieldLayout Layout => layout;
        public Camera ViewCamera => _camera;
        public BoardTheme Theme => themeFollowsLevel ? ThemeForLevel.For(levelNumber) : fixedTheme;

        [SerializeField] private int cols = 8;
        [SerializeField] private float cellWidth = 1f;
        [SerializeField] private PlayfieldLayout layout = PlayfieldLayout.Solo;
        [SerializeField] private int levelNumber = 1;
        [SerializeField] private Camera targetCamera;
        [SerializeField, Range(0f, 1f)] private float viewportHeightFraction = 1f;
        [SerializeField] private DifficultyCurveConfig difficultyCurve;
        [SerializeField] private PatternLevelCatalog patternCatalog;
        [SerializeField] private bool startAtSavedSoloLevel;
        [SerializeField] private bool themeFollowsLevel = true;
        [SerializeField] private BoardTheme fixedTheme;

        private int _rows;
        private Camera _camera;

        private void Awake()
        {
            _camera = targetCamera != null ? targetCamera : Camera.main;
            Frame = new PlayfieldFrame(PlayfieldSpec.For(layout), PlayfieldFrame.WorldPerPtFor(cellWidth), SafeAreaRaisePt());
            _rows = FitCameraAndComputeRows(_camera);
            ShooterOrigin = Frame.Pointer(_camera.transform.position, _camera.orthographicSize);
            LoadLevel(startAtSavedSoloLevel ? Flow.SoloRunSave.CurrentLevel : levelNumber);
        }

        private static float SafeAreaRaisePt() => PlayfieldFrame.RaisePt(Screen.safeArea.yMin, Screen.width);

        public void LoadLevel(int newLevelNumber)
        {
            levelNumber = newLevelNumber;
            CurrentDifficulty = difficultyCurve.ForLevel(levelNumber, _rows);
            Grid = GenerateGrid();
            RecomputeBounds();
            OnLevelLoaded?.Invoke(levelNumber);
        }

        private GridModel GenerateGrid()
        {
            var grid = new GridModel(_rows, cols, cellWidth);
            var pattern = patternCatalog != null && patternCatalog.TryGetPlan(levelNumber, out var plan)
                ? (plan, patternCatalog.Settings)
                : ((LevelPatternPlan, PatternGenerationSettings)?)null;
            return LevelContentGenerator.Generate(grid, levelNumber, CurrentDifficulty, pattern);
        }

        // The wall's advance grows the reserved ceiling band by one row height
        // per push (see BoardArt.CeilingBand), so an unobstructed shot must stop
        // that much sooner too - otherwise it would sail past the wall's
        // actual current position into space that no longer exists.
        private void RecomputeBounds()
        {
            var boardWidth = HexGridMath.BoardWidthWithOffsetMargin(cols, cellWidth);
            var advancedCeilingHeight = CeilingHeight + Grid.RowsPushed * HexGridMath.RowHeight(cellWidth);
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

        public void DropFloatingCells()
        {
            var floaters = MatchResolver.FindFloatingCells(Grid);
            if (floaters.Count > 0) DropCells(floaters);
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

        private int FitCameraAndComputeRows(Camera camera)
        {
            var screenWidth = HexGridMath.BoardWidthWithOffsetMargin(cols, cellWidth) + 2f * Frame.RailWidth;
            camera.orthographicSize = PlayfieldSizer.OrthographicSizeForWidth(screenWidth, Screen.width, Screen.height * viewportHeightFraction);
            PositionBoard(camera);
            var position = camera.transform.position;
            var band = Frame.BoardTopY(position, camera.orthographicSize) - Frame.DangerY(position, camera.orthographicSize);
            return PlayfieldSizer.RowsFitting(band, cellWidth);
        }

        private void PositionBoard(Camera camera)
        {
            var x = camera.transform.position.x - HexGridMath.BoardOriginXOffset(cols, cellWidth);
            var y = Frame.BoardTopY(camera.transform.position, camera.orthographicSize) - cellWidth * 0.5f;
            transform.position = new Vector3(x, y, transform.position.z);
        }
    }
}
