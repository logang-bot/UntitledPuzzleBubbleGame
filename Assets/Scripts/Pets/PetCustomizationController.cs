using Game.Flow;
using Game.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Pets
{
    /// <summary>
    /// New Game's customization screen (PetCustomization.unity): name and
    /// style the cat and dog, then Start saves both pets, begins a fresh solo
    /// run and loads the Game scene. See docs/features/game-flow/overview.md.
    /// </summary>
    public class PetCustomizationController : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private PetAppearanceCatalog appearanceCatalog;

        private const int TitleFontSize = 44;
        private static readonly Vector2 StartButtonSize = new Vector2(320f, 90f);
        private static readonly Vector2 BackButtonSize = new Vector2(180f, 70f);

        private PetEditorPanel _catPanel;
        private PetEditorPanel _dogPanel;
        private Button _startButton;

        private void Start()
        {
            SpawnTitle();
            _catPanel = SpawnPanel(PetSpecies.Cat, UiPlacement.Stretch(new Vector2(0.03f, 0.5f), new Vector2(0.97f, 0.82f)));
            _dogPanel = SpawnPanel(PetSpecies.Dog, UiPlacement.Stretch(new Vector2(0.03f, 0.16f), new Vector2(0.97f, 0.48f)));
            SpawnBackButton();
            _startButton = SpawnStartButton();
            _catPanel.OnNameChanged += RefreshStartButton;
            _dogPanel.OnNameChanged += RefreshStartButton;
            RefreshStartButton();
        }

        private void SpawnTitle()
        {
            var placement = UiPlacement.Stretch(new Vector2(0.05f, 0.84f), new Vector2(0.95f, 0.9f));
            var title = RuntimeUi.CreateLabel(canvasRect, "Title", placement);
            title.text = "Meet Your Pets";
            title.fontSize = TitleFontSize;
            title.color = Color.white;
        }

        private PetEditorPanel SpawnPanel(PetSpecies species, UiPlacement placement)
        {
            return new PetEditorPanel(canvasRect, new PetEditorPanelSpec
            {
                Species = species,
                Appearance = appearanceCatalog.For(species),
                InitialProfile = PetProfileStore.Load(species),
                Placement = placement
            });
        }

        private void SpawnBackButton()
        {
            RuntimeUi.CreateButton(canvasRect, new UiButtonSpec
            {
                Name = "BackButton",
                Label = "Back",
                Placement = UiPlacement.At(new Vector2(0.15f, 0.95f), BackButtonSize),
                OnClick = () => SceneManager.LoadScene(SceneNames.MainMenu)
            });
        }

        private Button SpawnStartButton()
        {
            return RuntimeUi.CreateButton(canvasRect, new UiButtonSpec
            {
                Name = "StartButton",
                Label = "Start",
                Placement = UiPlacement.At(new Vector2(0.5f, 0.08f), StartButtonSize),
                OnClick = HandleStartClicked
            });
        }

        private void HandleStartClicked()
        {
            PetProfileStore.Save(PetSpecies.Cat, _catPanel.CurrentProfile);
            PetProfileStore.Save(PetSpecies.Dog, _dogPanel.CurrentProfile);
            SoloRunSave.BeginNewRun();
            SceneManager.LoadScene(SceneNames.Game);
        }

        private void RefreshStartButton()
        {
            _startButton.interactable = HasName(_catPanel) && HasName(_dogPanel);
        }

        private static bool HasName(PetEditorPanel panel) => panel.CurrentProfile.Name.Length > 0;
    }
}
