using System;
using Game.Flow;
using Game.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Gameplay
{
    /// <summary>
    /// Entry-point menu, lives in its own scene (MainMenu.unity). New Game
    /// goes through pet customization; Continue resumes the saved solo run
    /// and is disabled until one exists. See docs/features/game-flow/overview.md.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRect;

        private const string PlaceholderTitle = "Untitled Pet Game";
        private const int TitleFontSize = 56;
        private static readonly Vector2 ButtonSize = new Vector2(320f, 90f);

        private void Start()
        {
            SpawnTitle();
            SpawnSceneButton("New Game", 0.58f, SceneNames.PetCustomization);
            SpawnSceneButton("Continue", 0.46f, SceneNames.Game).interactable = SoloRunSave.HasRun;
            SpawnSceneButton("2 Players", 0.34f, SceneNames.BattleMode);
            SpawnSceneButton("Settings", 0.22f, SceneNames.SettingsMenu);
        }

        private void SpawnTitle()
        {
            var placement = UiPlacement.Stretch(new Vector2(0.05f, 0.72f), new Vector2(0.95f, 0.88f));
            var title = RuntimeUi.CreateLabel(canvasRect, "Title", placement);
            title.text = PlaceholderTitle;
            title.fontSize = TitleFontSize;
            title.color = Color.white;
        }

        private Button SpawnSceneButton(string label, float anchorY, string sceneName)
        {
            return RuntimeUi.CreateButton(canvasRect, new UiButtonSpec
            {
                Name = label.Replace(" ", "") + "Button",
                Label = label,
                Placement = UiPlacement.At(new Vector2(0.5f, anchorY), ButtonSize),
                OnClick = LoadSceneAction(sceneName)
            });
        }

        private static Action LoadSceneAction(string sceneName) => () => SceneManager.LoadScene(sceneName);
    }
}
