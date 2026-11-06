using Game.Art;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Battle
{
    /// <summary>
    /// One player's stats card on the 2-player result: a WIN / LOSE / DRAW
    /// plate over score, bubbles popped, best chain and shots, with dotted
    /// dividers between rows.
    /// </summary>
    public class BattleStatsPanel
    {
        private static readonly RectInt PanelRect = new RectInt(20, 78, 230, 150);
        private static readonly RectInt PlateRect = new RectInt(73, 68, 124, 24);
        private static readonly string[] RowLabels = { "SCORE", "POPPED", "BEST CHAIN", "SHOTS" };
        private const int FirstRowY = 110;
        private const int RowPitch = 28;
        private const int LabelX = 36;
        private const int ValueRight = 234;

        private readonly Text _title;
        private readonly Text[] _values = new Text[4];

        public BattleStatsPanel(MenuUiFactory ui, RectTransform half)
        {
            ui.Sprite(half, "panel", PanelRect);
            _title = ui.Plate(half, PlateRect, "");
            for (var row = 0; row < RowLabels.Length; row++)
                _values[row] = SpawnRow(ui, half, row);
        }

        public void Show(BattleHalfOutcome outcome, MatchStats stats)
        {
            _title.text = outcome.ToString().ToUpperInvariant();
            _values[0].text = stats.Score.ToString("000000");
            _values[1].text = stats.Popped.ToString();
            _values[2].text = stats.BestChain.ToString();
            _values[3].text = stats.Shots.ToString();
        }

        private static Text SpawnRow(MenuUiFactory ui, RectTransform half, int row)
        {
            var y = FirstRowY + row * RowPitch;
            var left = ui.Styles.Small.WithAlignment(TextAnchor.MiddleLeft);
            ui.Label(half, new UiLabelSpec { Name = RowLabels[row], Text = RowLabels[row], Style = left, Placement = UiPlacement.TopLeft(new RectInt(LabelX, y - 2, 120, 11)) });
            if (row < RowLabels.Length - 1) DottedLine.Create(half, new RectInt(LabelX, y + 18, ValueRight - LabelX + 2, 1), MenuPalette.Night3);
            var right = ui.Styles.Small.WithColor(MenuPalette.Cream).WithAlignment(TextAnchor.MiddleRight);
            return ui.Label(half, new UiLabelSpec { Name = "Value", Style = right, Placement = UiPlacement.TopLeft(new RectInt(ValueRight - 90, y - 2, 90, 11)) });
        }
    }
}
