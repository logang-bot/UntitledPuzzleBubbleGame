using Game.Art;
using Game.UI;
using UnityEngine;

namespace Game.Results
{
    /// <summary>
    /// Level-complete overlay: the board shows through a flat 78 % Abyss dim,
    /// fireworks, confetti and lanterns play, and the win banner sits over
    /// the level/score panel with Continue and Menu. No pet, per the design.
    /// </summary>
    public class VictoryOverlay
    {
        private const float DimAmount = 0.78f;
        private static readonly RectInt BannerRect = new RectInt(10, 112, 250, 60);
        private static readonly RectInt BannerTextRect = new RectInt(44, 20, 162, 20);

        private readonly GameObject _root;
        private readonly ResultPanelView _panel;

        public VictoryOverlay(MenuUiFactory ui, RectTransform canvas, ResultButtonsSpec buttons)
        {
            var root = RuntimeUi.CreateRect(canvas, "VictoryOverlay", UiPlacement.Fill());
            var layers = NativeStage.Create(root);
            ui.Fill(layers.Background, "Dim", MenuPalette.AbyssDim(DimAmount));
            CelebrationParticles.Create(ui, NativeStage.CreateStageAligned(layers.Background, "Sky"), new CelebrationSpec { ConfettiCount = 26, FieldHeight = 585f, WithFireworksAndLanterns = true });
            SpawnBanner(ui, layers.Stage);
            _panel = new ResultPanelView(ui, layers.Stage);
            ResultButtons.Spawn(ui, layers.Stage, buttons);
            _root = root.gameObject;
            Hide();
        }

        public void Show(int level, int score)
        {
            _panel.Show(level, score);
            _root.SetActive(true);
        }

        public void Hide() => _root.SetActive(false);

        private static void SpawnBanner(MenuUiFactory ui, RectTransform stage)
        {
            var banner = ui.Sprite(stage, "banner_win", BannerRect).rectTransform;
            ui.Label(banner, new UiLabelSpec { Name = "Title", Text = "LEVEL COMPLETE", Style = ui.Styles.Title, Placement = UiPlacement.TopLeft(BannerTextRect) });
        }
    }
}
