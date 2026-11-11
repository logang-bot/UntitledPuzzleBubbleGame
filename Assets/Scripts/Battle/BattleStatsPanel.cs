using Game.UI;
using TMPro;
using UnityEngine;

namespace Game.Battle
{
    /// <summary>
    /// One player's stats card on the 2-player result: a WIN / LOSE / DRAW
    /// title plate over score, bubbles popped, best chain and shots (labels
    /// left, outlined values right), with dotted dividers between rows.
    /// Rects are pt in the player's half.
    /// </summary>
    public class BattleStatsPanel
    {
        private static readonly RectInt CardRect = new RectInt(26, 92, 308, 222);
        private static readonly RectInt PlateRect = new RectInt(100, 72, 160, 44);
        private static readonly string[] RowLabels = { "SCORE", "POPPED", "BEST CHAIN", "SHOTS" };
        private const int FirstRowY = 128;
        private const int RowPitch = 42;

        private readonly TMP_Text _title;
        private readonly TMP_Text[] _values = new TMP_Text[4];

        public BattleStatsPanel(MenuUiFactory ui, RectTransform half)
        {
            ui.Sprite(half, "stats_card", CardRect);
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

        private static TMP_Text SpawnRow(MenuUiFactory ui, RectTransform half, int row)
        {
            var y = FirstRowY + row * RowPitch;
            ui.LabelIn(half, new RectInt(50, y, 150, 30), (RowLabels[row], TextStyles.Label.Aligned(TextAlignmentOptions.Left)));
            if (row < RowLabels.Length - 1) ui.Tiled(half, "divider", new RectInt(48, y + 33, 264, 6));
            return ui.LabelIn(half, new RectInt(200, y, 110, 30), ("Value", TextStyles.StatValue.Aligned(TextAlignmentOptions.Right)));
        }
    }
}
