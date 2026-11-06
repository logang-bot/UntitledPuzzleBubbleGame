using Game.Art;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Results
{
    /// <summary>
    /// The 200×104 level/score panel shared by the victory overlay and the
    /// retry screen: "LEVEL nn" in Mist, "SCORE" in Stone, the score in Gold.
    /// </summary>
    public class ResultPanelView
    {
        public static readonly RectInt PanelRect = new RectInt(35, 286, 200, 104);

        private readonly Text _level;
        private readonly Text _score;

        public ResultPanelView(MenuUiFactory ui, RectTransform stage)
        {
            var panel = ui.Sprite(stage, "panel", PanelRect).rectTransform;
            _level = ui.CenteredLabel(panel, "", new LabelLine(14, ui.Styles.Small.WithColor(MenuPalette.Mist)));
            ui.CenteredLabel(panel, "SCORE", new LabelLine(34, ui.Styles.Small.WithColor(MenuPalette.Stone)));
            _score = ui.CenteredLabel(panel, "", new LabelLine(47, ui.Styles.Title.WithColor(MenuPalette.Gold)));
        }

        public void Show(int level, int score)
        {
            _level.text = $"LEVEL {level:00}";
            _score.text = score.ToString("000000");
        }
    }
}
