using Game.Gameplay;
using Game.Grid;
using Game.UI;
using TMPro;
using UnityEngine;

namespace Game.Deck
{
    /// <summary>
    /// The solo HUD plate in the deck, opposite the rotate slabs: score
    /// (6 digits), shots fired and level, each behind its cream icon on the
    /// menu's dark toggle track.
    /// </summary>
    public class HudView : MonoBehaviour
    {
        public const float LeftHandX = 60f;
        public const float RightHandX = 196f;
        private const float Top = 613f;

        private TMP_Text _score;
        private TMP_Text _shots;
        private TMP_Text _level;
        private (ScoreTracker Score, ShotsFiredCounter Shots, GameBoard Board) _sources;

        public static HudView Create(DeckUi ui, RectTransform layer, float x)
        {
            var view = layer.gameObject.AddComponent<HudView>();
            ui.Sprite(layer, "toggle_track", new Rect(x, Top, 104f, 44f));
            ui.At(layer, "hud_icon_score", new Vector2(x + 16f, 624f));
            ui.At(layer, "hud_icon_shots", new Vector2(x + 16f, 646f));
            ui.At(layer, "hud_icon_level", new Vector2(x + 60f, 646f));
            view._score = Value(ui, layer, new Rect(x + 27f, 612f, 72f, 24f));
            view._shots = Value(ui, layer, new Rect(x + 27f, 634f, 26f, 24f));
            view._level = Value(ui, layer, new Rect(x + 71f, 634f, 28f, 24f));
            return view;
        }

        public void Bind(ScoreTracker score, ShotsFiredCounter shots, GameBoard board)
        {
            _sources = (score, shots, board);
            score.OnScoreChanged += ShowScore;
            shots.OnShotsFiredChanged += ShowShots;
            board.OnLevelLoaded += ShowLevel;
            ShowScore(score.Score);
            ShowShots(shots.ShotsFired);
            ShowLevel(board.LevelNumber);
        }

        private static TMP_Text Value(DeckUi ui, RectTransform layer, Rect rect)
        {
            return ui.Label(layer, rect, ("0", GameTextStyles.HudValue.Aligned(TextAlignmentOptions.Left)));
        }

        private void ShowScore(int score) => _score.text = score.ToString("D6");

        private void ShowShots(int shots) => _shots.text = shots.ToString();

        private void ShowLevel(int level) => _level.text = level.ToString("D2");

        private void OnDestroy()
        {
            if (_sources.Score == null) return;
            _sources.Score.OnScoreChanged -= ShowScore;
            _sources.Shots.OnShotsFiredChanged -= ShowShots;
            _sources.Board.OnLevelLoaded -= ShowLevel;
        }
    }
}
