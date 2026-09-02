using System;
using Game.Grid;
using UnityEngine;

namespace Game.Shooter
{
    /// <summary>
    /// Fixed-speed rotating gun (arcade Puzzle Bobble-style): holding the
    /// rotate zones turns the aim angle at a constant rate; the fire zone
    /// raises OnFireRequested, consumed by FiredBubbleController. The preview
    /// line is occupancy-truncated the same way as the fired bubble's path, so
    /// they can never disagree. See
    /// docs/features/core-gameplay/firing-and-snapping.md.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class ShooterController : MonoBehaviour
    {
        [SerializeField] private GameBoard gameBoard;
        [SerializeField] private float maxAimAngleDegrees = 60f;
        [SerializeField] private float rotateSpeedDegreesPerSecond = 25f;
        [SerializeField] private int maxBounces = 10;
        [SerializeField] private float lineWidth = 0.08f;
        [SerializeField] private Material lineMaterial;
        [SerializeField] private HoldInputZone rotateLeftZone;
        [SerializeField] private HoldInputZone rotateRightZone;
        [SerializeField] private HoldInputZone fireZone;

        public event Action<Vector2, float> OnFireRequested;
        public int MaxBounces => maxBounces;

        private LineRenderer _lineRenderer;
        private TrajectoryPredictor _predictor;
        private Vector2 _shooterOrigin;
        private float _aimAngleDegrees;
        private bool _firePressedLastFrame;

        private void Awake()
        {
            ConfigureLineRenderer();
        }

        private void Start()
        {
            _shooterOrigin = gameBoard.ShooterOrigin;
            RebuildPredictor();
            gameBoard.OnRowPushedDown += HandleBoardChanged;
            gameBoard.OnLevelLoaded += HandleBoardChanged;
        }

        private void OnDestroy()
        {
            gameBoard.OnRowPushedDown -= HandleBoardChanged;
            gameBoard.OnLevelLoaded -= HandleBoardChanged;
        }

        // gameBoard.Bounds.CeilingY advances with the wall (see
        // GameBoard.RecomputeBounds); TrajectoryPredictor takes a snapshot of
        // it in its constructor, so it must be rebuilt whenever Bounds changes
        // or the preview would keep simulating shots against the old boundary.
        private void HandleBoardChanged(bool wasLastRowOccupied) => RebuildPredictor();
        private void HandleBoardChanged(int levelNumber) => RebuildPredictor();

        private void RebuildPredictor()
        {
            _predictor = new TrajectoryPredictor(gameBoard.Bounds);
        }

        private void Update()
        {
            UpdateAimAngle();
            UpdateFireInput();
            DrawPreview();
        }

        private void ConfigureLineRenderer()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            _lineRenderer.widthMultiplier = lineWidth;
            _lineRenderer.useWorldSpace = true;
        }

        private void UpdateAimAngle()
        {
            var direction = (rotateRightZone.IsPressed ? 1 : 0) - (rotateLeftZone.IsPressed ? 1 : 0);
            _aimAngleDegrees += direction * rotateSpeedDegreesPerSecond * Time.deltaTime;
            _aimAngleDegrees = Mathf.Clamp(_aimAngleDegrees, -maxAimAngleDegrees, maxAimAngleDegrees);
        }

        private void UpdateFireInput()
        {
            if (fireZone.IsPressed && !_firePressedLastFrame)
                Fire();
            _firePressedLastFrame = fireZone.IsPressed;
        }

        public void Fire()
        {
            OnFireRequested?.Invoke(_shooterOrigin, _aimAngleDegrees);
        }

        private void DrawPreview()
        {
            var rawPoints = _predictor.Simulate(_shooterOrigin, _aimAngleDegrees, maxBounces);
            var board = (gameBoard.Grid, (Vector2)gameBoard.transform.position);
            var truncated = OccupancyCollision.Truncate(rawPoints, board, gameBoard.CellWidth);
            var targetCenter = StruckCellCenter(truncated.StruckCell, board);
            var points = PreviewPointsCalculator.TrimToSurface(truncated.Points, targetCenter, gameBoard.CellWidth);
            _lineRenderer.positionCount = points.Count;
            for (var i = 0; i < points.Count; i++)
                _lineRenderer.SetPosition(i, points[i]);
        }

        private static Vector2? StruckCellCenter((int Row, int Col)? struckCell, (GridModel Grid, Vector2 Origin) board)
        {
            if (struckCell == null) return null;
            return board.Grid.GetWorldPosition(struckCell.Value.Row, struckCell.Value.Col) + board.Origin;
        }
    }
}
