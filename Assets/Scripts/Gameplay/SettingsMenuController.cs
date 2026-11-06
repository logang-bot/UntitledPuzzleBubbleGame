using Game.Art;
using Game.Flow;
using Game.Settings;
using Game.UI;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>
    /// Settings screen, lives in its own scene (SettingsMenu.unity), reached
    /// from and returning to MainMenu. One 9-slice panel under a SETTINGS
    /// plate; each setting is a label pair plus a segmented toggle, rows
    /// stacking at a 40 px pitch.
    /// </summary>
    public class SettingsMenuController : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private MenuArtLibrary art;

        private static readonly RectInt BackRect = new RectInt(10, 28, 26, 30);
        private static readonly RectInt PanelRect = new RectInt(20, 236, 230, 100);
        private static readonly RectInt PlateRect = new RectInt(73, 226, 124, 24);
        private static readonly RectInt LandingLabelRect = new RectInt(34, 281, 76, 10);
        private static readonly RectInt LandingSubLabelRect = new RectInt(34, 291, 76, 10);
        private static readonly RectInt LandingToggleRect = new RectInt(112, 272, 126, 28);
        private const int ToggleSegmentWidth = 58;

        private void Start()
        {
            SceneTransition.Install(art);
            PixelCanvas.Configure(canvasRect.GetComponent<Canvas>());
            var ui = new MenuUiFactory(art);
            var layers = NativeStage.Create(canvasRect);
            ui.Fill(layers.Background, "NightFill", MenuPalette.Night0).raycastTarget = false;
            ui.Button(layers.Stage, new UiSkinButtonSpec { Name = "BackButton", Kind = ButtonKind.Back, Rect = BackRect, OnClick = () => SceneTransition.LoadScene(SceneNames.MainMenu) });
            ui.Sprite(layers.Stage, "panel", PanelRect);
            ui.Plate(layers.Stage, PlateRect, "SETTINGS");
            SpawnLandingRow(ui, layers.Stage);
        }

        private static void SpawnLandingRow(MenuUiFactory ui, RectTransform stage)
        {
            SpawnRowLabel(ui, stage, ("LANDING", LandingLabelRect, MenuPalette.Cream));
            SpawnRowLabel(ui, stage, ("ANIMATION", LandingSubLabelRect, MenuPalette.Stone));
            var toggle = new SegmentedToggle(ui, stage, new SegmentedToggleSpec { Name = "LandingToggle", Track = LandingToggleRect, SegmentWidth = ToggleSegmentWidth, Labels = new[] { "BOUNCE", "SQUASH" } });
            toggle.Select(GameSettings.LandingAnimationStyle == LandingAnimationStyle.OvershootBounce ? 0 : 1);
            toggle.OnSelected += index => GameSettings.LandingAnimationStyle = index == 0 ? LandingAnimationStyle.OvershootBounce : LandingAnimationStyle.SquashPop;
        }

        private static void SpawnRowLabel(MenuUiFactory ui, RectTransform stage, (string Text, RectInt Rect, Color Color) row)
        {
            ui.Label(stage, new UiLabelSpec
            {
                Name = row.Text,
                Text = row.Text,
                Style = ui.Styles.Small.WithColor(row.Color).WithAlignment(TextAnchor.MiddleLeft),
                Placement = UiPlacement.TopLeft(row.Rect)
            });
        }
    }
}
