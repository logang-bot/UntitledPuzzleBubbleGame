using Game.Results;
using Game.UI;
using UnityEngine;

namespace Game.Battle
{
    /// <summary>
    /// One player's half during the 2–3 s result moment: the gold YOU WIN
    /// banner under falling confetti, the soft-blue YOU LOSE banner, or the
    /// DRAW plate, each dropping in (banner_drop). Rects are pt in the half.
    /// </summary>
    public class BattleResultHalfView
    {
        private const int WinConfettiCount = 16;

        private static readonly BannerSpec Win = new BannerSpec
        {
            SpriteId = "banner_win",
            Text = "YOU WIN",
            Style = TextStyles.Title,
            Rect = new RectInt(10, 116, 340, 104),
            TextRect = new RectInt(30, 28, 280, 44)
        };

        private static readonly BannerSpec Lose = new BannerSpec
        {
            SpriteId = "banner_lose",
            Text = "YOU LOSE",
            Style = TextStyles.TitleSad,
            Rect = new RectInt(10, 120, 340, 104),
            TextRect = new RectInt(30, 26, 280, 44)
        };

        private static readonly BannerSpec Draw = new BannerSpec
        {
            SpriteId = "draw_plate",
            Text = "DRAW",
            Style = TextStyles.TitlePlate,
            Rect = new RectInt(70, 150, 220, 60),
            TextRect = new RectInt(0, 0, 220, 56)
        };

        private readonly GameObject _confetti;
        private readonly BannerView _win;
        private readonly BannerView _lose;
        private readonly BannerView _draw;

        public BattleResultHalfView(MenuUiFactory ui, RectTransform half)
        {
            _confetti = ConfettiField.Create(ui, half, (new Vector2(BattleHalfFrame.Width, BattleHalfFrame.HalfHeight), WinConfettiCount)).gameObject;
            _win = new BannerView(ui, half, Win);
            _lose = new BannerView(ui, half, Lose);
            _draw = new BannerView(ui, half, Draw);
        }

        public void Show(BattleHalfOutcome outcome)
        {
            _confetti.SetActive(outcome == BattleHalfOutcome.Win);
            _win.Show(outcome == BattleHalfOutcome.Win);
            _lose.Show(outcome == BattleHalfOutcome.Lose);
            _draw.Show(outcome == BattleHalfOutcome.Draw);
        }
    }
}
