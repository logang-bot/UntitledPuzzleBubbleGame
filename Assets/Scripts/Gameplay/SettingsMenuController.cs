using Game.Art;
using Game.Flow;
using Game.Menu;
using Game.Settings;
using Game.UI;
using TMPro;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>
    /// Settings screen, lives in its own scene (SettingsMenu.unity), reached
    /// from and returning to MainMenu. The deep-space sky and drifting stars
    /// under a settingsBackdrop dim; one 9-slice panel under a SETTINGS
    /// plate; each setting is a label pair plus a segmented toggle with a
    /// dotted divider under it, rows stacking at a 52 pt pitch.
    /// </summary>
    public class SettingsMenuController : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private MenuArtLibrary art;

        private static readonly RectInt BackRect = new RectInt(14, 40, 52, 56);
        private static readonly RectInt PanelRect = new RectInt(26, 300, 308, 132);
        private static readonly RectInt PlateRect = new RectInt(90, 280, 180, 44);
        private static readonly RectInt LandingLabelRect = new RectInt(46, 344, 116, 22);
        private static readonly RectInt LandingSubLabelRect = new RectInt(46, 366, 116, 16);
        private static readonly RectInt LandingToggleRect = new RectInt(164, 342, 152, 44);
        private static readonly RectInt LandingDividerRect = new RectInt(46, 404, 268, 6);

        private void Start()
        {
            SceneTransition.Install(art);
            MenuCanvas.Configure(canvasRect.GetComponent<Canvas>());
            var ui = new MenuUiFactory(art);
            var layers = NativeStage.Create(canvasRect);
            BuildBackdrop(ui, layers.Background);
            ui.Button(layers.Stage, new UiSkinButtonSpec { Name = "BackButton", Kind = ButtonKind.Back, Rect = BackRect, OnClick = () => SceneTransition.LoadScene(SceneNames.MainMenu) });
            ui.Sprite(layers.Stage, "panel", PanelRect);
            ui.Plate(layers.Stage, PlateRect, "SETTINGS");
            SpawnLandingRow(ui, layers.Stage);
        }

        private static void BuildBackdrop(MenuUiFactory ui, RectTransform background)
        {
            ui.Cover(background, "space_deep");
            StarField.Create(ui, NativeStage.CreateStageAligned(background, "SpaceStage"));
            ui.Fill(background, "Dim", MenuColors.SettingsBackdrop).raycastTarget = false;
        }

        private static void SpawnLandingRow(MenuUiFactory ui, RectTransform stage)
        {
            ui.LabelIn(stage, LandingLabelRect, ("LANDING", TextStyles.Label.Aligned(TextAlignmentOptions.Left)));
            ui.LabelIn(stage, LandingSubLabelRect, ("ANIMATION", TextStyles.SmallLabel.Aligned(TextAlignmentOptions.Left)));
            var toggle = new SegmentedToggle(ui, stage, new SegmentedToggleSpec { Name = "LandingToggle", Track = LandingToggleRect, Labels = new[] { "BOUNCE", "SQUASH" } });
            toggle.Select(GameSettings.LandingAnimationStyle == LandingAnimationStyle.OvershootBounce ? 0 : 1);
            toggle.OnSelected += index => GameSettings.LandingAnimationStyle = index == 0 ? LandingAnimationStyle.OvershootBounce : LandingAnimationStyle.SquashPop;
            ui.Tiled(stage, "divider", LandingDividerRect);
        }
    }
}
