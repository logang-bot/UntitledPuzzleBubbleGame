using System;
using System.Collections.Generic;
using Game.Grid;
using UnityEngine;

namespace Game.Shooter
{
    /// <summary>
    /// Fixed-speed rotating gun (arcade Puzzle Bobble-style): holding the
    /// rotate zones turns the aim angle at a constant rate; the fire zone
    /// raises OnFireRequested, consumed by FiredBubbleController. The preview
    /// line is always the raw, occupancy-unaware kinematic path - a direct,
    /// zero-lag function of the aim angle - and relies on opaque bubble
    /// sprites (sortingOrder above the line's) to visually occlude it where a
    /// shot would actually stop. LandingIndicator separately shows the
    /// occupancy-truncated, fire-time-accurate landing cell. See
    /// docs/features/core-gameplay/shooter-and-trajectory.md.
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
        private LandingIndicator _landingIndicator;
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
            _landingIndicator = new LandingIndicator();
            gameBoard.OnRowPushedDown += HandleBoardChanged;
            gameBoard.OnLevelLoaded += HandleBoardChanged;
        }

        private void OnDestroy()
        {
            gameBoard.OnRowPushedDown -= HandleBoardChanged;
            gameBoard.OnLevelLoaded -= HandleBoardChanged;
            _landingIndicator.Destroy();
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

        /// <summary>
        /// sortingOrder -1 puts the line behind bubbles (implicit order 0,
        /// same convention as CeilingRenderer), so opaque bubble sprites
        /// occlude it exactly where a shot would stop, with no truncation math.
        /// </summary>
        private void ConfigureLineRenderer()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            _lineRenderer.widthMultiplier = lineWidth;
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.sortingOrder = -1;
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
            SetLineRendererPoints(rawPoints);
            UpdateLandingIndicator(rawPoints);
        }

        private void SetLineRendererPoints(List<Vector2> points)
        {
            _lineRenderer.positionCount = points.Count;
            for (var i = 0; i < points.Count; i++)
                _lineRenderer.SetPosition(i, points[i]);
        }

        private void UpdateLandingIndicator(List<Vector2> rawPoints)
        {
            var board = (gameBoard.Grid, Origin: (Vector2)gameBoard.transform.position);
            var truncated = OccupancyCollision.Truncate(rawPoints, board, gameBoard.CellWidth);
            var landingCell = BubbleLandingResolver.ResolveLandingCell(board, truncated.Points[^1], truncated.StruckCell, gameBoard.CellWidth);
            if (landingCell == null) { _landingIndicator.Hide(); return; }
            _landingIndicator.Show(gameBoard.Grid.GetWorldPosition(landingCell.Value.Row, landingCell.Value.Col) + board.Origin);
        }
    }
}
