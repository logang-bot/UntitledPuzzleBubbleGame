using Game.Motion;
using Game.UI;
using TMPro;
using UnityEngine;

namespace Game.Results
{
    /// <summary>
    /// The 268×148 pt level/score panel shared by the victory overlay and the
    /// retry screen: "LEVEL nn" (label), "SCORE" (small label) and the score
    /// (number). It pops in after the banner (panel_pop) whenever it is shown.
    /// </summary>
    public class ResultPanelView
    {
        private static readonly RectInt PanelRect = new RectInt(46, 372, 268, 148);

        private readonly TMP_Text _level;
        private readonly TMP_Text _score;
        private readonly MotionPlayer _pop;

        public ResultPanelView(MenuUiFactory ui, RectTransform stage)
        {
            var group = RuntimeUi.CreateRect(stage, "ResultPanel", UiPlacement.TopLeft(PanelRect));
            UiPlacement.MovePivot(group, new Vector2(0.5f, 0.5f));
            ui.Sprite(group, "panel", new RectInt(0, 0, PanelRect.width, PanelRect.height));
            _level = ui.LabelIn(group, new RectInt(0, 20, PanelRect.width, 22), ("Level", TextStyles.Label));
            ui.LabelIn(group, new RectInt(0, 50, PanelRect.width, 16), ("SCORE", TextStyles.SmallLabel));
            _score = ui.LabelIn(group, new RectInt(0, 70, PanelRect.width, 46), ("Score", TextStyles.Number));
            _pop = MotionPlayer.Attach(group, MenuMotions.PanelPop);
        }

        public void Show(int level, int score)
        {
            _level.text = $"LEVEL {level:00}";
            _score.text = score.ToString("000000");
            _pop.Play();
        }
    }
}
