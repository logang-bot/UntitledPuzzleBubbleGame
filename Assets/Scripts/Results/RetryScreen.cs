using Game.Art;
using Game.Pets;
using Game.UI;
using UnityEngine;

namespace Game.Results
{
    /// <summary>
    /// Game-over screen, shown after the knock-out hold and the paw iris:
    /// flat Night 0 background, "KNOCKED OUT", the napping cat and dog
    /// (recolored as the player's saved pets), the level/score panel, then
    /// Retry and Exit.
    /// </summary>
    public class RetryScreen
    {
        private static readonly RectInt NapRect = new RectInt(15, 112, 240, 150);

        private readonly GameObject _root;
        private readonly ResultPanelView _panel;
        private readonly Material _fur;

        public RetryScreen(MenuUiFactory ui, RectTransform canvas, ResultButtonsSpec buttons)
        {
            var root = RuntimeUi.CreateRect(canvas, "RetryScreen", UiPlacement.Fill());
            var layers = NativeStage.Create(root);
            ui.Fill(layers.Background, "NightFill", MenuPalette.Night0);
            ui.CenteredLabel(layers.Stage, "KNOCKED OUT", new LabelLine(38, ui.Styles.Title));
            ui.CenteredLabel(layers.Stage, "THE PETS NEED A NAP", new LabelLine(63, ui.Styles.Small.WithColor(MenuPalette.Stone)));
            var nap = ui.AnimatedSprite(layers.Stage, "retry_nap", NapRect);
            _fur = FurRecolor.CreateInstance(ui.Art.FurRecolorMaterial);
            nap.GetComponent<UnityEngine.UI.Image>().material = _fur;
            _panel = new ResultPanelView(ui, layers.Stage);
            ResultButtons.Spawn(ui, layers.Stage, buttons);
            _root = root.gameObject;
            Hide();
        }

        public void Show(PetAppearanceCatalog catalog, (int Level, int Score) result)
        {
            ApplySavedFur(catalog);
            _panel.Show(result.Level, result.Score);
            _root.SetActive(true);
        }

        public void Hide() => _root.SetActive(false);

        private void ApplySavedFur(PetAppearanceCatalog catalog)
        {
            foreach (var species in new[] { PetSpecies.Cat, PetSpecies.Dog })
                FurRecolor.Apply(_fur, species, PetLook.From(catalog, species, PetProfileStore.Load(species)).Ramp);
        }
    }
}
