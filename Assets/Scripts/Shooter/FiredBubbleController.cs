using System;
using System.Collections.Generic;
using Game.BoardArt;
using Game.Grid;
using Game.Motion;
using Game.Superpowers;
using UnityEngine;

namespace Game.Shooter
{
    /// <summary>
    /// Fires, animates, and snaps a bubble in response to ShooterController's
    /// OnFireRequested — Milestone 3's hook-in point. Uses the same
    /// occupancy-truncated path the preview line draws, so the bubble always
    /// stops exactly where the player saw it going. See
    /// docs/features/core-gameplay/firing-and-snapping.md.
    /// </summary>
    public class FiredBubbleController : MonoBehaviour
    {
        [SerializeField] private GameBoard gameBoard;
        [SerializeField] private BoardView boardView;
        [SerializeField] private ShooterController shooterController;
        [SerializeField] private float bubbleSpeed = 8f;

        public event Action<SuperpowerId, (int Row, int Col)> OnSuperpowerLanded;
        public event Action OnQueueAdvanced;

        public bool HasArmedOrInFlightSuperpower => _armedAbility.HasValue || _firedAbility.HasValue;
        public BubbleColor LoadedColor => _nextColor;
        public BubbleColor QueuedColor => _queuedColor;
        public SuperpowerId? ArmedAbility => _armedAbility;

        private TrajectoryPredictor _predictor;
        private List<Vector2> _path;
        private (int Row, int Col)? _struckCell;
        private int _segmentIndex;
        private GameObject _flyingBubble;
        private BubbleColor _color;
        private BubbleColor _nextColor;
        private BubbleColor _queuedColor;
        private SuperpowerId? _armedAbility;
        private SuperpowerId? _firedAbility;
        private Vector2 _settleFrom;
        private Vector2 _settleTo;
        private (int Row, int Col)? _settleCell;
        private float _settleElapsed;

        public void ArmSuperpower(SuperpowerId ability) => _armedAbility = ability;

        private void Awake()
        {
            _nextColor = BubbleColorPalette.Random();
            _queuedColor = BubbleColorPalette.Random();
        }

        private void Start()
        {
            RebuildPredictor();
            shooterController.OnFireRequested += HandleFireRequested;
            gameBoard.OnRowPushedDown += HandleBoardChanged;
            gameBoard.OnLevelLoaded += HandleBoardChanged;
        }

        // gameBoard.Bounds.CeilingY advances with the wall (see
        // GameBoard.RecomputeBounds); TrajectoryPredictor takes a snapshot of
        // it in its constructor, so it must be rebuilt whenever Bounds changes
        // or a fired bubble would keep simulating against the old boundary.
        private void HandleBoardChanged(bool wasLastRowOccupied) => RebuildPredictor();
        private void HandleBoardChanged(int levelNumber) => RebuildPredictor();

        private void RebuildPredictor()
        {
            _predictor = new TrajectoryPredictor(gameBoard.Bounds);
        }

        private void OnDestroy()
        {
            shooterController.OnFireRequested -= HandleFireRequested;
            gameBoard.OnRowPushedDown -= HandleBoardChanged;
            gameBoard.OnLevelLoaded -= HandleBoardChanged;
        }

        private void Update()
        {
            if (_settleCell != null) AdvanceSettle();
            else if (_flyingBubble != null) AdvanceTowardNextPoint();
        }

        private void AdvanceSettle()
        {
            _settleElapsed += Time.deltaTime;
            var t = Mathf.Clamp01(_settleElapsed / GameMotions.SettleSlideSeconds);
            _flyingBubble.transform.position = Vector2.Lerp(_settleFrom, _settleTo, Ease.Apply(EaseKind.OutQuad, t));
            if (t >= 1f) Land(_settleCell);
        }

        private void Land((int Row, int Col)? landingCell)
        {
            if (_firedAbility.HasValue && landingCell.HasValue)
            {
                ClearFlyingBubble();
                OnSuperpowerLanded?.Invoke(_firedAbility.Value, landingCell.Value);
                _firedAbility = null;
                return;
            }
            ClearFlyingBubble();
            if (landingCell != null) gameBoard.PlaceBubble(landingCell.Value.Row, landingCell.Value.Col, _color);
        }

        private void ClearFlyingBubble()
        {
            Destroy(_flyingBubble);
            _flyingBubble = null;
            _settleCell = null;
        }

        private void HandleFireRequested(Vector2 origin, float angleDegrees)
        {
            var rawPoints = _predictor.Simulate(origin, angleDegrees, shooterController.MaxBounces);
            var truncated = OccupancyCollision.Truncate(rawPoints, BoardSpace(), gameBoard.CellWidth);
            _path = truncated.Points;
            _struckCell = truncated.StruckCell;
            _segmentIndex = 1;
            _color = _nextColor;
            _firedAbility = _armedAbility;
            _armedAbility = null;
            _flyingBubble = SpawnFlyingBubble(origin);
            AdvanceQueue();
        }

        private void AdvanceQueue()
        {
            _nextColor = _queuedColor;
            _queuedColor = BubbleColorPalette.Random();
            OnQueueAdvanced?.Invoke();
        }

        private GameObject SpawnFlyingBubble(Vector2 origin)
        {
            var bubble = _firedAbility.HasValue ? boardView.Skin.CreateSuperpower(_firedAbility.Value, null) : boardView.Skin.Create(_color, null);
            bubble.name = "FlyingBubble";
            bubble.transform.position = origin;
            return bubble;
        }

        private void AdvanceTowardNextPoint()
        {
            var target = _path[_segmentIndex];
            var position = Vector2.MoveTowards(_flyingBubble.transform.position, target, bubbleSpeed * Time.deltaTime);
            _flyingBubble.transform.position = position;
            if (position == target) AdvanceToNextSegmentOrLand();
        }

        private void AdvanceToNextSegmentOrLand()
        {
            _segmentIndex++;
            if (_segmentIndex >= _path.Count) BeginSettle();
        }

        private void BeginSettle()
        {
            var landingCell = BubbleLandingResolver.ResolveLandingCell(BoardSpace(), _path[^1], _struckCell, gameBoard.CellWidth);
            if (landingCell == null) { Land(null); return; }
            _settleFrom = _flyingBubble.transform.position;
            _settleTo = gameBoard.Grid.GetWorldPosition(landingCell.Value.Row, landingCell.Value.Col) + (Vector2)gameBoard.transform.position;
            _settleCell = landingCell;
            _settleElapsed = 0f;
        }

        private (GridModel Grid, Vector2 Origin) BoardSpace()
        {
            return (gameBoard.Grid, gameBoard.transform.position);
        }
    }
}
