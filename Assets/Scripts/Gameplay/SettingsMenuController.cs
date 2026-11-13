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
    /// dotted divider under it, rows stacking at a 52 pt pitch: landing animation, then the thumb side
    /// the in-game rotate slabs sit on.
    /// </summary>
    public class SettingsMenuController : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private MenuArtLibrary art;

        private static readonly RectInt BackRect = new RectInt(14, 40, 52, 56);
        private static readonly RectInt PanelRect = new RectInt(26, 300, 308, 184);
        private static readonly RectInt PlateRect = new RectInt(90, 280, 180, 44);
        private static readonly RectInt LabelRect = new RectInt(46, 344, 116, 22);
        private static readonly RectInt SubLabelRect = new RectInt(46, 366, 116, 16);
        private static readonly RectInt ToggleRect = new RectInt(164, 342, 152, 44);
        private static readonly RectInt DividerRect = new RectInt(46, 404, 268, 6);
        private const int RowPitch = 52;

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
            SpawnRow(ui, layers.Stage, (0, LandingRow()));
            SpawnRow(ui, layers.Stage, (1, HandRow()));
        }

        private static void BuildBackdrop(MenuUiFactory ui, RectTransform background)
        {
            ui.Cover(background, "space_deep");
            StarField.Create(ui, NativeStage.CreateStageAligned(background, "SpaceStage"));
            ui.Fill(background, "Dim", MenuColors.SettingsBackdrop).raycastTarget = false;
        }

        private static SettingsRowSpec LandingRow() => new SettingsRowSpec
        {
            Name = "Landing",
            Label = "LANDING",
            SubLabel = "ANIMATION",
            Options = new[] { "BOUNCE", "SQUASH" },
            Selected = GameSettings.LandingAnimationStyle == LandingAnimationStyle.OvershootBounce ? 0 : 1,
            OnSelected = index => GameSettings.LandingAnimationStyle = index == 0 ? LandingAnimationStyle.OvershootBounce : LandingAnimationStyle.SquashPop
        };

        private static SettingsRowSpec HandRow() => new SettingsRowSpec
        {
            Name = "Hand",
            Label = "CONTROLS",
            SubLabel = "THUMB SIDE",
            Options = new[] { "LEFT", "RIGHT" },
            Selected = GameSettings.Handedness == Handedness.Left ? 0 : 1,
            OnSelected = index => GameSettings.Handedness = index == 0 ? Handedness.Left : Handedness.Right
        };

        private static void SpawnRow(MenuUiFactory ui, RectTransform stage, (int Index, SettingsRowSpec Spec) row)
        {
            var down = row.Index * RowPitch;
            ui.LabelIn(stage, Below(LabelRect, down), (row.Spec.Label, TextStyles.Label.Aligned(TextAlignmentOptions.Left)));
            ui.LabelIn(stage, Below(SubLabelRect, down), (row.Spec.SubLabel, TextStyles.SmallLabel.Aligned(TextAlignmentOptions.Left)));
            var toggle = new SegmentedToggle(ui, stage, new SegmentedToggleSpec { Name = row.Spec.Name + "Toggle", Track = Below(ToggleRect, down), Labels = row.Spec.Options });
            toggle.Select(row.Spec.Selected);
            toggle.OnSelected += row.Spec.OnSelected;
            ui.Tiled(stage, "divider", Below(DividerRect, down));
        }

        private static RectInt Below(RectInt rect, int down) => new RectInt(rect.x, rect.y + down, rect.width, rect.height);
    }
}
