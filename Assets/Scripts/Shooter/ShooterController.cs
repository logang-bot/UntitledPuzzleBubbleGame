using System;
using Game.BoardArt;
using Game.Grid;
using UnityEngine;

namespace Game.Shooter
{
    /// <summary>
    /// Fixed-speed rotating gun (arcade Puzzle Bobble-style): holding the
    /// rotate zones turns the aim angle at a constant rate; the fire zone
    /// raises OnFireRequested, consumed by FiredBubbleController. The aim
    /// dots follow the occupancy-truncated path, so they end where the shot
    /// would stop (the guide has no separate landing ghost). See
    /// docs/features/core-gameplay/shooter-and-trajectory.md.
    /// </summary>
    public class ShooterController : MonoBehaviour
    {
        public event Action<Vector2, float> OnFireRequested;
        public int MaxBounces => maxBounces;
        public float AimAngleDegrees => _aimAngleDegrees;
        public int RotateDirection { get; private set; }
        public bool IsFirePressed => fireZone.IsPressed;

        // Blocks every fire path (manual press and external auto-fire timers
        // calling Fire()) while aiming stays live - kept separate from
        // `enabled`, which game-over/match-end flows already own.
        public bool IsFireLocked { get; set; }

        [SerializeField] private GameBoard gameBoard;
        [SerializeField] private BoardView boardView;
        [SerializeField] private float maxAimAngleDegrees = 60f;
        [SerializeField] private float rotateSpeedDegreesPerSecond = 25f;
        [SerializeField] private int maxBounces = 10;
        [SerializeField] private HoldInputZone rotateLeftZone;
        [SerializeField] private HoldInputZone rotateRightZone;
        [SerializeField] private HoldInputZone fireZone;

        private TrajectoryPredictor _predictor;
        private AimDotsView _aimDots;
        private Vector2 _shooterOrigin;
        private float _aimAngleDegrees;
        private bool _firePressedLastFrame;

        public void BindControls(HoldInputZone rotateLeft, HoldInputZone rotateRight, HoldInputZone fire)
        {
            rotateLeftZone = rotateLeft;
            rotateRightZone = rotateRight;
            fireZone = fire;
        }

        private void Start()
        {
            _shooterOrigin = gameBoard.ShooterOrigin;
            RebuildPredictor();
            _aimDots = AimDotsView.Create(transform, boardView.Art, gameBoard.Frame.WorldPerPt);
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

        private void UpdateAimAngle()
        {
            RotateDirection = (rotateRightZone.IsPressed ? 1 : 0) - (rotateLeftZone.IsPressed ? 1 : 0);
            _aimAngleDegrees += RotateDirection * rotateSpeedDegreesPerSecond * Time.deltaTime;
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
            if (IsFireLocked) return;
            OnFireRequested?.Invoke(_shooterOrigin, _aimAngleDegrees);
        }

        private void DrawPreview()
        {
            var rawPoints = _predictor.Simulate(_shooterOrigin, _aimAngleDegrees, maxBounces);
            var board = (gameBoard.Grid, Origin: (Vector2)gameBoard.transform.position);
            _aimDots.Show(OccupancyCollision.Truncate(rawPoints, board, gameBoard.CellWidth).Points);
        }

        private void OnDisable()
        {
            if (_aimDots != null) _aimDots.Hide();
        }
    }
}
