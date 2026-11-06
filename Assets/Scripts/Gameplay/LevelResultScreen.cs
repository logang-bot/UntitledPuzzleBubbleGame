using Game.Art;
using Game.Flow;
using Game.Grid;
using Game.Pets;
using Game.Results;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay
{
    /// <summary>
    /// Solo results, on their own pixel-art overlay canvas above the HUD.
    /// A win shows the victory overlay (Continue / Menu). A loss holds the
    /// knocked-out board for 3 s with input blocked, then the paw iris
    /// swaps in the retry screen (Retry / Exit). See
    /// docs/features/game-flow/overview.md and
    /// docs/features/core-gameplay/hud-and-level-flow.md.
    /// </summary>
    public class LevelResultScreen : MonoBehaviour
    {
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private ScoreTracker scoreTracker;
        [SerializeField] private GameBoard gameBoard;
        [SerializeField] private MenuArtLibrary art;
        [SerializeField] private PetAppearanceCatalog appearanceCatalog;

        private const int OverlaySortingOrder = 10;
        private const float KnockoutHoldSeconds = 3f;

        private VictoryOverlay _victory;
        private RetryScreen _retry;
        private Image _inputBlocker;
        private float _knockoutRemaining = -1f;

        private void Start()
        {
            SceneTransition.Install(art);
            BuildScreens(new MenuUiFactory(art), OverlayCanvasFactory.Create("ResultCanvas", OverlaySortingOrder));
            gameStateManager.OnLevelWon += HandleWon;
            gameStateManager.OnLevelLost += HandleLost;
        }

        private void OnDestroy()
        {
            gameStateManager.OnLevelWon -= HandleWon;
            gameStateManager.OnLevelLost -= HandleLost;
        }

        private void BuildScreens(MenuUiFactory ui, RectTransform canvas)
        {
            _inputBlocker = RuntimeUi.CreateImage(canvas, "KnockoutBlocker", UiPlacement.Fill());
            _inputBlocker.color = Color.clear;
            _inputBlocker.gameObject.SetActive(false);
            _victory = new VictoryOverlay(ui, canvas, new ResultButtonsSpec { PrimaryLabel = "CONTINUE", OnPrimary = HandleContinue, SecondaryLabel = "MENU", OnSecondary = GoToMenu });
            _retry = new RetryScreen(ui, canvas, new ResultButtonsSpec { PrimaryLabel = "RETRY", OnPrimary = HandleRetry, SecondaryLabel = "EXIT", OnSecondary = GoToMenu });
        }

        private void HandleWon() => _victory.Show(gameBoard.LevelNumber, scoreTracker.Score);

        private void HandleLost()
        {
            _inputBlocker.gameObject.SetActive(true);
            _knockoutRemaining = KnockoutHoldSeconds;
        }

        private void Update()
        {
            if (_knockoutRemaining < 0f) return;
            _knockoutRemaining -= Time.unscaledDeltaTime;
            if (_knockoutRemaining < 0f) SceneTransition.Cover(ShowRetryScreen);
        }

        private void ShowRetryScreen()
        {
            _inputBlocker.gameObject.SetActive(false);
            _retry.Show(appearanceCatalog, (gameBoard.LevelNumber, scoreTracker.Score));
        }

        private void HandleContinue()
        {
            _victory.Hide();
            gameStateManager.AdvanceToNextLevel();
        }

        private void HandleRetry()
        {
            SceneTransition.Cover(() =>
            {
                _retry.Hide();
                gameStateManager.RetryLevel();
            });
        }

        private static void GoToMenu() => SceneTransition.LoadScene(SceneNames.MainMenu);
    }
}
