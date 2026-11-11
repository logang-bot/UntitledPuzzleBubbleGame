using Game.Art;
using Game.Motion;
using Game.UI;
using UnityEngine;

namespace Game.Results
{
    /// <summary>
    /// Level-complete overlay: the board shows through the victoryDim (72 %),
    /// lanterns rise and fireworks burst behind the win banner, which drops
    /// in, then the level/score panel and Continue/Menu pop in, with confetti
    /// falling over everything. No pet, per the design.
    /// </summary>
    public class VictoryOverlay
    {
        private const int ConfettiCount = 28;
        private static readonly BannerSpec Banner = new BannerSpec
        {
            SpriteId = "banner_win",
            Text = "LEVEL COMPLETE",
            Style = TextStyles.Title,
            Rect = new RectInt(10, 132, 340, 104),
            TextRect = new RectInt(30, 28, 280, 44)
        };

        private readonly GameObject _root;
        private readonly BannerView _banner;
        private readonly ResultPanelView _panel;
        private readonly MotionPlayer _buttons;

        public VictoryOverlay(MenuUiFactory ui, RectTransform canvas, ResultButtonsSpec buttons)
        {
            var root = RuntimeUi.CreateRect(canvas, "VictoryOverlay", UiPlacement.Fill());
            var layers = NativeStage.Create(root);
            BuildSky(ui, layers.Background);
            _banner = new BannerView(ui, layers.Stage, Banner);
            _panel = new ResultPanelView(ui, layers.Stage);
            _buttons = ResultButtons.Spawn(ui, layers.Stage, buttons);
            ConfettiField.Create(ui, NativeStage.CreateStageAligned(root, "ConfettiStage"), (MenuCanvas.NativeSize, ConfettiCount));
            _root = root.gameObject;
            Hide();
        }

        public void Show(int level, int score)
        {
            _root.SetActive(true);
            _banner.Show(true);
            _panel.Show(level, score);
            _buttons.Play();
        }

        public void Hide() => _root.SetActive(false);

        private static void BuildSky(MenuUiFactory ui, RectTransform background)
        {
            ui.Fill(background, "Dim", MenuColors.VictoryDim).raycastTarget = false;
            var sky = NativeStage.CreateStageAligned(background, "Sky");
            LanternField.Create(ui, sky);
            FireworkShow.Create(ui, sky);
        }
    }
}
