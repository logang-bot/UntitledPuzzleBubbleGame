using System.Collections.Generic;
using Game.Grid;
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
        [SerializeField] private ShooterController shooterController;
        [SerializeField] private float bubbleSpeed = 8f;

        private TrajectoryPredictor _predictor;
        private List<Vector2> _path;
        private (int Row, int Col)? _struckCell;
        private int _segmentIndex;
        private GameObject _flyingBubble;
        private BubbleColor _color;

        private void Start()
        {
            _predictor = new TrajectoryPredictor(gameBoard.Bounds);
            shooterController.OnFireRequested += HandleFireRequested;
        }

        private void OnDestroy()
        {
            shooterController.OnFireRequested -= HandleFireRequested;
        }

        private void Update()
        {
            if (_flyingBubble != null) AdvanceTowardNextPoint();
        }

        private void HandleFireRequested(Vector2 origin, float angleDegrees)
        {
            var rawPoints = _predictor.Simulate(origin, angleDegrees, shooterController.MaxBounces);
            var truncated = OccupancyCollision.Truncate(rawPoints, BoardSpace(), gameBoard.CellWidth);
            _path = truncated.Points;
            _struckCell = truncated.StruckCell;
            _segmentIndex = 1;
            _color = BubbleColorPalette.Random();
            _flyingBubble = SpawnFlyingBubble(origin);
        }

        private GameObject SpawnFlyingBubble(Vector2 origin)
        {
            var bubble = new GameObject("FlyingBubble");
            bubble.transform.position = origin;
            var spriteRenderer = bubble.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CircleSpriteFactory.CreateWhiteCircle();
            spriteRenderer.color = BubbleColorPalette.ToColor(_color);
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
            if (_segmentIndex >= _path.Count) Land();
        }

        private void Land()
        {
            Destroy(_flyingBubble);
            _flyingBubble = null;
            var landingCell = BubbleLandingResolver.ResolveLandingCell(BoardSpace(), _path[^1], _struckCell);
            if (landingCell != null) gameBoard.PlaceBubble(landingCell.Value.Row, landingCell.Value.Col, _color);
        }

        private (GridModel Grid, Vector2 Origin) BoardSpace()
        {
            return (gameBoard.Grid, gameBoard.transform.position);
        }
    }
}
