using System;
using Game.Art;
using Game.UI;
using UnityEngine;

namespace Game.Battle
{
    /// <summary>
    /// The 2-player stats screen after the result moment: the board under the
    /// statsDim (78 %), a stats card facing each player, the night center
    /// strip edged in ink with Rematch on it, and a Home button at each
    /// player's right (the top one rotated with its half).
    /// </summary>
    public class BattleStatsView
    {
        private const int StripBleed = 200;
        private static readonly RectInt StripRect = FullBleed(374, 32);
        private static readonly RectInt[] StripInkEdges = { FullBleed(373, 3), FullBleed(405, 3) };
        private static readonly RectInt StripRimLine = FullBleed(376, 2);
        private static readonly RectInt RematchRect = new RectInt(148, 356, 64, 68);
        private static readonly RectInt BottomHomeRect = new RectInt(296, 362, 52, 56);
        private static readonly RectInt TopHomeRect = new RectInt(12, 362, 52, 56);

        private readonly GameObject _root;
        private readonly BattleStatsPanel _top;
        private readonly BattleStatsPanel _bottom;

        public BattleStatsView(MenuUiFactory ui, RectTransform canvas, (Action Rematch, Action Home) actions)
        {
            var root = RuntimeUi.CreateRect(canvas, "BattleStats", UiPlacement.Fill());
            var layers = NativeStage.Create(root);
            ui.Fill(layers.Background, "Dim", MenuColors.StatsDim);
            _top = new BattleStatsPanel(ui, BattleHalfFrame.Create(layers.Stage, isTop: true));
            _bottom = new BattleStatsPanel(ui, BattleHalfFrame.Create(layers.Stage, isTop: false));
            SpawnStrip(layers.Stage);
            SpawnButtons(ui, layers.Stage, actions);
            _root = root.gameObject;
            Hide();
        }

        public void Show((BattleHalfOutcome Outcome, MatchStats Stats) top, (BattleHalfOutcome Outcome, MatchStats Stats) bottom)
        {
            _top.Show(top.Outcome, top.Stats);
            _bottom.Show(bottom.Outcome, bottom.Stats);
            _root.SetActive(true);
        }

        public void Hide() => _root.SetActive(false);

        private static void SpawnStrip(RectTransform stage)
        {
            Bar(stage, StripRect, MenuColors.Night);
            foreach (var edge in StripInkEdges)
                Bar(stage, edge, MenuColors.Ink);
            Bar(stage, StripRimLine, MenuColors.PanelRim);
        }

        private static void Bar(RectTransform stage, RectInt rect, Color color)
        {
            var bar = RuntimeUi.CreateImage(stage, "Strip", UiPlacement.TopLeft(rect));
            bar.color = color;
            bar.raycastTarget = false;
        }

        /// <summary>A horizontal band running past both sides of the 360 pt grid, so it spans wider screens too.</summary>
        private static RectInt FullBleed(int y, int height) => new RectInt(-StripBleed, y, 360 + 2 * StripBleed, height);

        private static void SpawnButtons(MenuUiFactory ui, RectTransform stage, (Action Rematch, Action Home) actions)
        {
            ui.Button(stage, new UiSkinButtonSpec { Name = "RematchButton", Kind = ButtonKind.Rematch, Rect = RematchRect, OnClick = actions.Rematch });
            ui.Button(stage, new UiSkinButtonSpec { Name = "HomeButton", Kind = ButtonKind.Home, Rect = BottomHomeRect, OnClick = actions.Home });
            var topHome = ui.Button(stage, new UiSkinButtonSpec { Name = "TopHomeButton", Kind = ButtonKind.Home, Rect = TopHomeRect, OnClick = actions.Home });
            topHome.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
        }
    }
}
