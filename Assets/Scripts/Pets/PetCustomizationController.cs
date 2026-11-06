using System.Collections.Generic;
using Game.Art;
using Game.Flow;
using Game.UI;
using UnityEngine;

namespace Game.Pets
{
    /// <summary>
    /// New Game's customization screen (PetCustomization.unity), two steps:
    /// step 1 picks and styles the aimer (CAT/DOG toggle), step 2 styles the
    /// other pet, the firer. Start saves both pets and the aimer, begins a
    /// fresh solo run and loads the Game scene. Back on step 2 returns to
    /// step 1 keeping the edits. See docs/features/game-flow/overview.md.
    /// </summary>
    public class PetCustomizationController : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private MenuArtLibrary art;
        [SerializeField] private PetAppearanceCatalog appearanceCatalog;

        private static readonly RectInt BackRect = new RectInt(10, 28, 26, 30);
        private static readonly RectInt ToggleRect = new RectInt(40, 76, 190, 28);
        private static readonly RectInt NextRect = new RectInt(35, 504, 200, 38);
        private const int ToggleSegmentWidth = 91;

        private readonly Dictionary<PetSpecies, PetProfile> _drafts = new();
        private CustomizationFlow _flow;
        private CustomizationHeader _header;
        private SegmentedToggle _roleToggle;
        private PetPanelView _panel;
        private SkinnedButton _nextButton;

        private void Start()
        {
            SceneTransition.Install(art);
            PixelCanvas.Configure(canvasRect.GetComponent<Canvas>());
            LoadDrafts();
            var ui = new MenuUiFactory(art);
            var layers = NativeStage.Create(canvasRect);
            ui.Fill(layers.Background, "NightFill", MenuPalette.Night0).raycastTarget = false;
            BuildScreen(ui, layers.Stage);
            ShowStep();
        }

        private void LoadDrafts()
        {
            _flow = new CustomizationFlow(PetProfileStore.LoadAimer());
            _drafts[PetSpecies.Cat] = PetProfileStore.Load(PetSpecies.Cat);
            _drafts[PetSpecies.Dog] = PetProfileStore.Load(PetSpecies.Dog);
        }

        private void BuildScreen(MenuUiFactory ui, RectTransform stage)
        {
            ui.Button(stage, new UiSkinButtonSpec { Name = "BackButton", Kind = ButtonKind.Back, Rect = BackRect, OnClick = HandleBack });
            _header = new CustomizationHeader(ui, stage);
            _roleToggle = new SegmentedToggle(ui, stage, new SegmentedToggleSpec { Name = "RoleToggle", Track = ToggleRect, SegmentWidth = ToggleSegmentWidth, Labels = new[] { "CAT", "DOG" } });
            _roleToggle.OnSelected += HandleSpeciesToggled;
            _panel = new PetPanelView(ui, stage, appearanceCatalog);
            _panel.OnProfileChanged += HandleProfileChanged;
            _nextButton = ui.Button(stage, new UiSkinButtonSpec { Name = "NextButton", Kind = ButtonKind.Primary, Label = "NEXT", Rect = NextRect, OnClick = HandleNext });
        }

        private void ShowStep()
        {
            var edited = _flow.Edited;
            _header.Show(_flow.Step, AimerDescription());
            _roleToggle.Select(edited == PetSpecies.Cat ? 0 : 1);
            _panel.Show(edited, _drafts[edited]);
            _nextButton.SetLabel(_flow.Step == CustomizationStep.Aimer ? "NEXT" : "START");
            RefreshNextButton();
        }

        private string AimerDescription()
        {
            var aimer = _flow.Aimer;
            return $"{_drafts[aimer].Name.ToUpperInvariant()} THE {(aimer == PetSpecies.Cat ? "CAT" : "DOG")}";
        }

        private void RefreshNextButton()
        {
            _nextButton.SetInteractable(CustomizationFlow.CanAdvance(_drafts[_flow.Edited].Name));
        }

        private void HandleSpeciesToggled(int index)
        {
            _flow.SelectSpecies(index == 0 ? PetSpecies.Cat : PetSpecies.Dog);
            ShowStep();
            _panel.Celebrate();
        }

        private void HandleProfileChanged(PetProfile profile)
        {
            _drafts[_flow.Edited] = profile;
            RefreshNextButton();
        }

        private void HandleNext()
        {
            if (_flow.Step == CustomizationStep.Firer) { StartRun(); return; }
            _flow.Next();
            ShowStep();
        }

        private void HandleBack()
        {
            if (_flow.TryStepBack()) ShowStep();
            else SceneTransition.LoadScene(SceneNames.MainMenu);
        }

        private void StartRun()
        {
            PetProfileStore.Save(PetSpecies.Cat, _drafts[PetSpecies.Cat]);
            PetProfileStore.Save(PetSpecies.Dog, _drafts[PetSpecies.Dog]);
            PetProfileStore.SaveAimer(_flow.Aimer);
            SoloRunSave.BeginNewRun();
            SceneTransition.LoadScene(SceneNames.Game);
        }
    }
}
