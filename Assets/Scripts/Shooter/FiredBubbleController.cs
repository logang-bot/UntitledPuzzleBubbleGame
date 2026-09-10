using System;
using System.Collections.Generic;
using Game.Grid;
using Game.Settings;
using Game.Superpowers;
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
        [SerializeField] private RectTransform rotateLeftZoneRect;
        [SerializeField] private float bubbleSpeed = 8f;

        public event Action<SuperpowerId, (int Row, int Col)> OnSuperpowerLanded;

        public bool HasArmedOrInFlightSuperpower => _armedAbility.HasValue || _firedAbility.HasValue;

        // The indicator is a UI element (not a world-space sprite) so it can be anchored
        // directly to the left of the fire-zone square, at the same height, on the same Canvas.
        private const float IndicatorSize = 60f;
        private const float IndicatorMargin = 15f;
        private const float SettleDurationSeconds = 0.18f;

        private TrajectoryPredictor _predictor;
        private List<Vector2> _path;
        private (int Row, int Col)? _struckCell;
        private int _segmentIndex;
        private GameObject _flyingBubble;
        private BubbleColor _color;
        private BubbleColor _nextColor;
        private SuperpowerId? _armedAbility;
        private SuperpowerId? _firedAbility;
        private GameObject _nextBubbleIndicator;
        private Vector2 _settleFrom;
        private Vector2 _settleTo;
        private (int Row, int Col)? _settleCell;
        private float _settleElapsed;

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
            rect.anchoredPosition = fireZoneRect.anchoredPosition + new Vector2(-LeftOffset(), 0f);
        }

        // A bigger offset moves the indicator closer to the rotate-left zone, so
        // it's capped at whatever clears that zone, not floored by it - Constant
        // Pixel Size means a fixed margin tuned for one screen width isn't safe
        // on a narrower one once either zone's size changes.
        private float LeftOffset()
        {
            var fireZoneOffset = fireZoneRect.sizeDelta.x * 0.5f + IndicatorSize * 0.5f + IndicatorMargin;
            var canvasHalfWidth = ((RectTransform)fireZoneRect.parent).rect.width * 0.5f;
            var rotateZoneInnerEdge = canvasHalfWidth - rotateLeftZoneRect.sizeDelta.x - Mathf.Abs(rotateLeftZoneRect.anchoredPosition.x);
            var maxSafeOffset = rotateZoneInnerEdge - IndicatorSize * 0.5f - IndicatorMargin;
            return Mathf.Min(fireZoneOffset, maxSafeOffset);
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

        public void ArmSuperpower(SuperpowerId ability) => _armedAbility = ability;

        private void AdvanceSettle()
        {
            _settleElapsed += Time.deltaTime;
            var t = Mathf.Clamp01(_settleElapsed / SettleDurationSeconds);
            var style = GameSettings.LandingAnimationStyle;
            _flyingBubble.transform.position = Vector2.LerpUnclamped(_settleFrom, _settleTo, BubbleSettleMotion.Ease(style, t));
            var scale = BubbleSettleMotion.SquashScale(style, t);
            _flyingBubble.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            if (t >= 1f) Land(_settleCell);
        }

        private void Land((int Row, int Col)? landingCell)
        {
            if (_firedAbility.HasValue && landingCell.HasValue)
            {
                ClearFlyingBubble();
                OnSuperpowerLanded?.Invoke(_firedAbility.Value, landingCell.Value);
                _firedAbility = null;
                PrepareNextBubble();
                return;
            }
            ClearFlyingBubble();
            if (landingCell != null) gameBoard.PlaceBubble(landingCell.Value.Row, landingCell.Value.Col, _color);
            PrepareNextBubble();
        }

        private void ClearFlyingBubble()
        {
            Destroy(_flyingBubble);
            _flyingBubble = null;
            _settleCell = null;
        }

        private void PrepareNextBubble()
        {
            _nextColor = BubbleColorPalette.Random();
            _nextBubbleIndicator.GetComponent<Image>().color = BubbleColorPalette.ToColor(_nextColor);
            _nextBubbleIndicator.SetActive(true);
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
