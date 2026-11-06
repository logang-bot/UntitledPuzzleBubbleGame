using Game.Results;
using Game.UI;
using UnityEngine;

namespace Game.Battle
{
    /// <summary>
    /// One player's half during the 2–3 s result moment: YOU WIN banner with
    /// confetti, YOU LOSE banner, or a DRAW plate.
    /// </summary>
    public class BattleResultHalfView
    {
        private static readonly RectInt BannerRect = new RectInt(10, 84, 250, 60);
        private static readonly RectInt WinTextRect = new RectInt(44, 20, 162, 20);
        private static readonly RectInt LoseTextRect = new RectInt(44, 22, 162, 20);
        private static readonly RectInt DrawPlateRect = new RectInt(73, 96, 124, 24);

        private readonly GameObject _win;
        private readonly GameObject _lose;
        private readonly GameObject _draw;

        public BattleResultHalfView(MenuUiFactory ui, RectTransform half)
        {
            _win = SpawnBanner(ui, half, ("banner_win", "YOU WIN", WinTextRect));
            var confetti = CelebrationParticles.Create(ui, (RectTransform)_win.transform, new CelebrationSpec { ConfettiCount = 18, FieldHeight = 200f });
            confetti.transform.SetAsFirstSibling();
            _lose = SpawnBanner(ui, half, ("banner_lose", "YOU LOSE", LoseTextRect));
            _draw = ui.Plate(half, DrawPlateRect, "DRAW").transform.parent.gameObject;
        }

        public void Show(BattleHalfOutcome outcome)
        {
            _win.SetActive(outcome == BattleHalfOutcome.Win);
            _lose.SetActive(outcome == BattleHalfOutcome.Lose);
            _draw.SetActive(outcome == BattleHalfOutcome.Draw);
        }

        private static GameObject SpawnBanner(MenuUiFactory ui, RectTransform half, (string SpriteId, string Text, RectInt TextRect) banner)
        {
            var group = RuntimeUi.CreateRect(half, banner.Text, UiPlacement.Fill());
            var image = ui.Sprite(group, banner.SpriteId, BannerRect).rectTransform;
            ui.Label(image, new UiLabelSpec { Name = "Title", Text = banner.Text, Style = ui.Styles.Title, Placement = UiPlacement.TopLeft(banner.TextRect) });
            return group.gameObject;
        }
    }
}
