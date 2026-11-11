using Game.Art;
using Game.Flow;
using Game.Menu;
using Game.Motion;
using Game.Pets;
using Game.UI;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>
    /// Entry-point menu, lives in its own scene (MainMenu.unity): the deep
    /// space backdrop with drifting cosmos and pets, the logo and four
    /// buttons. New Game goes through pet customization; Continue resumes
    /// the saved solo run and is disabled until one exists. See
    /// docs/features/game-flow/overview.md.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private MenuArtLibrary art;
        [SerializeField] private PetAppearanceCatalog appearanceCatalog;

        private static readonly Vector2 LogoPivot = new Vector2(180f, 52f);
        private const int ButtonX = 66;
        private const int FirstButtonY = 520;
        private const int ButtonPitch = 60;
        private static readonly Vector2Int ButtonSize = new Vector2Int(228, 52);

        private MenuUiFactory _ui;

        private void Start()
        {
            SceneTransition.Install(art);
            MenuCanvas.Configure(canvasRect.GetComponent<Canvas>());
            _ui = new MenuUiFactory(art);
            var layers = NativeStage.Create(canvasRect);
            SpaceBackdrop.Build(_ui, layers.Background, appearanceCatalog);
            MotionPlayer.AttachAndPlay(_ui.PivotSprite(layers.Stage, "logo_lockup", LogoPivot).rectTransform, MenuMotions.LogoBob);
            SpawnButtons(layers.Stage);
        }

        private void SpawnButtons(RectTransform stage)
        {
            SpawnSceneButton(stage, new MenuEntry("New Game", ButtonKind.Primary, SceneNames.PetCustomization), 0);
            var resume = SpawnSceneButton(stage, new MenuEntry("Continue", ButtonKind.Secondary, SceneNames.Game), 1);
            resume.SetInteractable(SoloRunSave.HasRun);
            SpawnSceneButton(stage, new MenuEntry("2 Players", ButtonKind.Secondary, SceneNames.BattleMode), 2);
            SpawnSceneButton(stage, new MenuEntry("Settings", ButtonKind.Secondary, SceneNames.SettingsMenu), 3);
        }

        private SkinnedButton SpawnSceneButton(RectTransform stage, MenuEntry entry, int row)
        {
            return _ui.Button(stage, new UiSkinButtonSpec
            {
                Name = entry.Label.Replace(" ", "") + "Button",
                Kind = entry.Kind,
                Label = entry.Label.ToUpperInvariant(),
                Rect = new RectInt(ButtonX, FirstButtonY + row * ButtonPitch, ButtonSize.x, ButtonSize.y),
                OnClick = () => SceneTransition.LoadScene(entry.SceneName)
            });
        }

        private readonly struct MenuEntry
        {
            public readonly string Label;
            public readonly ButtonKind Kind;
            public readonly string SceneName;

            public MenuEntry(string label, ButtonKind kind, string sceneName)
            {
                Label = label;
                Kind = kind;
                SceneName = sceneName;
            }
        }
    }
}
