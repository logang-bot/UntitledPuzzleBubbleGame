using System.Collections.Generic;
using Game.Art;
using Game.Gameplay;
using Game.Shooter;
using Game.Grid;
using UnityEngine;

namespace Game.BoardArt
{
    /// <summary>
    /// Draws a GameBoard in the cartoon game guide's art, driven by its
    /// events: bubbles (BubbleLayer) and the ceiling band (CeilingBand) under
    /// a root that slides on pushes, plus the backdrop, rails and danger line
    /// fixed to the view. The debug toggle swaps in the Milestone-1 plain
    /// circles. While a level's build-in plays, IsBuildingIn holds firing
    /// (ShooterBuildInGate).
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        public bool IsBuildingIn => _buildInRemaining > 0f;
        public IBubbleSkin Skin { get; private set; }
        public MenuArtLibrary Art => art;

        [SerializeField] private GameBoard gameBoard;
        [SerializeField] private MenuArtLibrary art;
        [SerializeField] private bool useDebugCircles;
        [SerializeField] private GameStateManager gameState;
        [SerializeField] private FiredBubbleController firedBubbles;

        private BubbleLayer _bubbles;
        private CeilingBand _ceiling;
        private BackgroundView _background;
        private BoardPushSlide _slide;
        private float _buildInRemaining;
        private float _warningSince;

        public void PlanBurstDelays(System.Func<(int Row, int Col), float> delay) => _bubbles.PlanBurstDelays(delay);

        private void Awake()
        {
            Skin = useDebugCircles ? new DebugCircleSkin(gameBoard.CellWidth) : new CartoonBubbleSkin(art, gameBoard.CellWidth);
            if (firedBubbles != null) SuperpowerBursts.Attach(this, (gameBoard, art, firedBubbles));
        }

        private void Start()
        {
            _slide = CreateSlidingRoot();
            _bubbles = new BubbleLayer(gameBoard, Skin, _slide.transform);
            _ceiling = new CeilingBand(gameBoard, art, _slide.transform);
            BuildViewDressing();
            ShowLevel();
            Subscribe();
        }

        private BoardPushSlide CreateSlidingRoot()
        {
            var root = new GameObject("BoardSprites").AddComponent<BoardPushSlide>();
            root.transform.SetParent(gameBoard.transform, worldPositionStays: false);
            return root;
        }

        private void BuildViewDressing()
        {
            var anchor = ViewAnchor.Create(gameBoard.ViewCamera, transform);
            PlayfieldEdges.Create(gameBoard, art, anchor);
            _background = new BackgroundView(gameBoard, art, anchor);
        }

        private void ShowLevel()
        {
            _ceiling.Refresh();
            _background.Refresh();
            _buildInRemaining = _bubbles.SpawnAllBuildingIn();
        }

        private void Subscribe()
        {
            gameBoard.OnBubblePlaced += OnBubblePlaced;
            gameBoard.OnBubblesPopped += OnBubblesPopped;
            gameBoard.OnClusterDropped += OnClusterDropped;
            gameBoard.OnRowPushedDown += OnRowPushedDown;
            gameBoard.OnLevelLoaded += OnLevelLoaded;
        }

        private void OnDestroy()
        {
            gameBoard.OnBubblePlaced -= OnBubblePlaced;
            gameBoard.OnBubblesPopped -= OnBubblesPopped;
            gameBoard.OnClusterDropped -= OnClusterDropped;
            gameBoard.OnRowPushedDown -= OnRowPushedDown;
            gameBoard.OnLevelLoaded -= OnLevelLoaded;
        }

        private void Update()
        {
            if (_buildInRemaining > 0f) _buildInRemaining -= Time.deltaTime;
            var warning = gameState != null && gameState.IsCeilingWarning;
            _warningSince = warning ? _warningSince + Time.deltaTime : 0f;
            _ceiling.ShowWarning(warning, _warningSince);
        }

        private void OnBubblePlaced(int row, int col) => _bubbles.Place((row, col));

        private void OnBubblesPopped(IReadOnlyCollection<(int Row, int Col)> cells, BubbleColor color) => _bubbles.Pop(cells, color);

        private void OnClusterDropped(IReadOnlyCollection<(int Row, int Col)> cells) => _bubbles.Drop(cells, FloorY());

        private void OnRowPushedDown(bool wasLastRowOccupied)
        {
            _bubbles.RebuildAfterPush();
            _ceiling.Refresh();
            _slide.Play(HexGridMath.RowHeight(gameBoard.CellWidth));
        }

        private void OnLevelLoaded(int levelNumber)
        {
            _bubbles.ClearAll();
            ShowLevel();
        }

        private float FloorY()
        {
            var view = gameBoard.ViewCamera;
            return view.transform.position.y - view.orthographicSize - gameBoard.CellWidth;
        }
    }
}
