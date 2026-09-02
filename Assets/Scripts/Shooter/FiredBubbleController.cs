using System.Collections.Generic;
using Game.Grid;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private RectTransform fireZoneRect;
        [SerializeField] private float bubbleSpeed = 8f;

        // The indicator is a UI element (not a world-space sprite) so it can be anchored
        // directly to the left of the fire-zone square, at the same height, on the same Canvas.
        private const float IndicatorSize = 60f;
        private const float IndicatorMargin = 15f;

        private TrajectoryPredictor _predictor;
        private List<Vector2> _path;
        private (int Row, int Col)? _struckCell;
        private int _segmentIndex;
        private GameObject _flyingBubble;
        private BubbleColor _color;
        private BubbleColor _nextColor;
        private GameObject _nextBubbleIndicator;

        private void Start()
        {
            RebuildPredictor();
            shooterController.OnFireRequested += HandleFireRequested;
            gameBoard.OnRowPushedDown += HandleBoardChanged;
            gameBoard.OnLevelLoaded += HandleBoardChanged;
            _nextColor = BubbleColorPalette.Random();
            _nextBubbleIndicator = SpawnIndicator();
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

        private GameObject SpawnIndicator()
        {
            var indicator = new GameObject("NextBubbleIndicator", typeof(RectTransform));
            var rect = (RectTransform)indicator.transform;
            ConfigureIndicatorRect(rect);
            var image = indicator.AddComponent<Image>();
            image.sprite = CircleSpriteFactory.CreateWhiteCircle();
            image.color = BubbleColorPalette.ToColor(_nextColor);
            return indicator;
        }

        private void ConfigureIndicatorRect(RectTransform rect)
        {
            rect.SetParent(fireZoneRect.parent, worldPositionStays: false);
            rect.anchorMin = fireZoneRect.anchorMin;
            rect.anchorMax = fireZoneRect.anchorMax;
            rect.pivot = fireZoneRect.pivot;
            rect.sizeDelta = new Vector2(IndicatorSize, IndicatorSize);
            var xOffset = fireZoneRect.sizeDelta.x * 0.5f + IndicatorSize * 0.5f + IndicatorMargin;
            rect.anchoredPosition = fireZoneRect.anchoredPosition + new Vector2(-xOffset, 0f);
        }

        private void OnDestroy()
        {
            shooterController.OnFireRequested -= HandleFireRequested;
            gameBoard.OnRowPushedDown -= HandleBoardChanged;
            gameBoard.OnLevelLoaded -= HandleBoardChanged;
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
            _color = _nextColor;
            _nextBubbleIndicator.SetActive(false);
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
            var landingCell = BubbleLandingResolver.ResolveLandingCell(BoardSpace(), _path[^1], _struckCell, gameBoard.CellWidth);
            if (landingCell != null) gameBoard.PlaceBubble(landingCell.Value.Row, landingCell.Value.Col, _color);
            PrepareNextBubble();
        }

        private void PrepareNextBubble()
        {
            _nextColor = BubbleColorPalette.Random();
            _nextBubbleIndicator.GetComponent<Image>().color = BubbleColorPalette.ToColor(_nextColor);
            _nextBubbleIndicator.SetActive(true);
        }

        private (GridModel Grid, Vector2 Origin) BoardSpace()
        {
            return (gameBoard.Grid, gameBoard.transform.position);
        }
    }
}
