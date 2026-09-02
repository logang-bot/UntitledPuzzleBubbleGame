using Game.Grid;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay
{
    /// <summary>
    /// Bottom-bar HUD (score, shots fired, level) built at runtime, anchored
    /// just above an existing bottom-of-screen RectTransform (the fire zone)
    /// rather than overlaid on the board, which fills the full screen with no
    /// letterbox. Mirrors ShotTimerDisplay's runtime-build/anchor recipe. See
    /// docs/features/core-gameplay/hud-and-level-flow.md.
    /// </summary>
    public class HudDisplay : MonoBehaviour
    {
        [SerializeField] private ScoreTracker scoreTracker;
        [SerializeField] private ShotsFiredCounter shotsFiredCounter;
        [SerializeField] private GameBoard gameBoard;
        [SerializeField] private RectTransform anchorRect;

        private const float StatWidth = 160f;
        private const float StatHeight = 40f;
        private const float StatMargin = 15f;

        private Text _scoreText;
        private Text _shotsText;
        private Text _levelText;

        private void Start()
        {
            _scoreText = SpawnStatText("StatScore", anchorX: 0.15f);
            _shotsText = SpawnStatText("StatShots", anchorX: 0.5f);
            _levelText = SpawnStatText("StatLevel", anchorX: 0.85f);
            SubscribeToTrackers();
            RefreshAllText();
        }

        private void OnDestroy()
        {
            UnsubscribeFromTrackers();
        }

        private void SubscribeToTrackers()
        {
            scoreTracker.OnScoreChanged += UpdateScoreText;
            shotsFiredCounter.OnShotsFiredChanged += UpdateShotsText;
            gameBoard.OnLevelLoaded += UpdateLevelText;
        }

        private void UnsubscribeFromTrackers()
        {
            scoreTracker.OnScoreChanged -= UpdateScoreText;
            shotsFiredCounter.OnShotsFiredChanged -= UpdateShotsText;
            gameBoard.OnLevelLoaded -= UpdateLevelText;
        }

        private void RefreshAllText()
        {
            UpdateScoreText(scoreTracker.Score);
            UpdateShotsText(shotsFiredCounter.ShotsFired);
            UpdateLevelText(gameBoard.LevelNumber);
        }

        private void UpdateScoreText(int score) => _scoreText.text = $"Score: {score}";
        private void UpdateShotsText(int shotsFired) => _shotsText.text = $"Shots: {shotsFired}";
        private void UpdateLevelText(int levelNumber) => _levelText.text = $"Level: {levelNumber}";

        private Text SpawnStatText(string name, float anchorX)
        {
            var display = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)display.transform;
            ConfigureStatRect(rect, anchorX);
            var text = display.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            return text;
        }

        private void ConfigureStatRect(RectTransform rect, float anchorX)
        {
            rect.SetParent(anchorRect.parent, worldPositionStays: false);
            rect.anchorMin = new Vector2(anchorX, 0f);
            rect.anchorMax = new Vector2(anchorX, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(StatWidth, StatHeight);
            rect.anchoredPosition = new Vector2(0f, anchorRect.sizeDelta.y + StatMargin);
        }
    }
}
