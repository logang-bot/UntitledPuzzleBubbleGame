using System;
using Game.Art;
using Game.UI;
using UnityEngine;

namespace Game.Battle
{
    /// <summary>
    /// The 2-player stats screen after the result moment: the board under a
    /// 78 % Abyss dim, a stats card facing each player, the center bar with
    /// Rematch, and a Home button at each player's right.
    /// </summary>
    public class BattleStatsView
    {
        private const float DimAmount = 0.78f;
        private static readonly RectInt CenterBarRect = new RectInt(0, 284, 270, 17);
        private static readonly RectInt RematchRect = new RectInt(118, 271, 34, 38);
        private static readonly RectInt BottomHomeRect = new RectInt(218, 277, 26, 30);
        private static readonly RectInt TopHomeRect = new RectInt(26, 277, 26, 30);

        private readonly GameObject _root;
        private readonly BattleStatsPanel _top;
        private readonly BattleStatsPanel _bottom;

        public BattleStatsView(MenuUiFactory ui, RectTransform canvas, (Action Rematch, Action Home) actions)
        {
            var root = RuntimeUi.CreateRect(canvas, "BattleStats", UiPlacement.Fill());
            var layers = NativeStage.Create(root);
            ui.Fill(layers.Background, "Dim", MenuPalette.AbyssDim(DimAmount));
            _top = new BattleStatsPanel(ui, BattleHalfFrame.Create(layers.Stage, isTop: true));
            _bottom = new BattleStatsPanel(ui, BattleHalfFrame.Create(layers.Stage, isTop: false));
            SpawnCenterBar(ui, layers.Stage);
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

        private static void SpawnCenterBar(MenuUiFactory ui, RectTransform stage)
        {
            var bar = RuntimeUi.CreateImage(stage, "CenterBar", UiPlacement.TopLeft(CenterBarRect));
            bar.color = MenuPalette.Night0;
            var edgeLength = CenterBarRect.width;
            RuntimeUi.CreateImage(bar.rectTransform, "TopEdge", UiPlacement.TopLeft(new RectInt(0, 0, edgeLength, 1))).color = MenuPalette.GoldDark;
            RuntimeUi.CreateImage(bar.rectTransform, "BottomEdge", UiPlacement.TopLeft(new RectInt(0, 16, edgeLength, 1))).color = MenuPalette.GoldDark;
        }

        private static void SpawnButtons(MenuUiFactory ui, RectTransform stage, (Action Rematch, Action Home) actions)
        {
            ui.Button(stage, new UiSkinButtonSpec { Name = "RematchButton", Kind = ButtonKind.Rematch, Rect = RematchRect, OnClick = actions.Rematch });
            ui.Button(stage, new UiSkinButtonSpec { Name = "HomeButton", Kind = ButtonKind.Home, Rect = BottomHomeRect, OnClick = actions.Home });
            var topHome = ui.Button(stage, new UiSkinButtonSpec { Name = "TopHomeButton", Kind = ButtonKind.Home, Rect = TopHomeRect, OnClick = actions.Home });
            var rect = (RectTransform)topHome.transform;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition += new Vector2(TopHomeRect.width / 2f, -TopHomeRect.height / 2f);
            rect.localEulerAngles = new Vector3(0f, 0f, 180f);
        }
    }
}
