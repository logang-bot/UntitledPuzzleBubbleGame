using Game.Art;
using Game.Menu;
using Game.Motion;
using Game.Pets;
using Game.UI;
using UnityEngine;

namespace Game.Results
{
    /// <summary>
    /// Game-over screen, shown after the knock-out hold and the paw iris: the
    /// dusk sky with stars, "KNOCKED OUT", the pixel napping cat and dog
    /// (recolored as the player's saved pets, 4 device px per art px) on the
    /// cartoon nap blanket, then the level/score panel and Retry/Exit popping in.
    /// </summary>
    public class RetryScreen
    {
        private const int NapDevicePixels = 4;
        private static readonly RectInt TitleRect = new RectInt(30, 58, 300, 44);
        private static readonly RectInt SubtitleRect = new RectInt(30, 104, 300, 18);
        private static readonly Vector2 BlanketPivot = new Vector2(177f, 329f);
        private static readonly Vector2 NapTopLeft = new Vector2(20f, 156f);

        private readonly GameObject _root;
        private readonly ResultPanelView _panel;
        private readonly MotionPlayer _buttons;
        private readonly Material _fur;

        public RetryScreen(MenuUiFactory ui, RectTransform canvas, ResultButtonsSpec buttons)
        {
            var root = RuntimeUi.CreateRect(canvas, "RetryScreen", UiPlacement.Fill());
            var layers = NativeStage.Create(root);
            DuskBackdrop.Build(ui, layers.Background);
            ui.LabelIn(layers.Stage, TitleRect, ("KNOCKED OUT", TextStyles.TitleSad));
            ui.LabelIn(layers.Stage, SubtitleRect, ("THE PETS NEED A NAP", TextStyles.SmallLabel));
            _fur = BuildNap(ui, layers.Stage);
            _panel = new ResultPanelView(ui, layers.Stage);
            _buttons = ResultButtons.Spawn(ui, layers.Stage, buttons);
            _root = root.gameObject;
            Hide();
        }

        public void Show(PetAppearanceCatalog catalog, (int Level, int Score) result)
        {
            ApplySavedFur(catalog);
            _root.SetActive(true);
            _panel.Show(result.Level, result.Score);
            _buttons.Play();
        }

        public void Hide() => _root.SetActive(false);

        private static Material BuildNap(MenuUiFactory ui, RectTransform stage)
        {
            ui.PivotSprite(stage, "stage_nap", BlanketPivot);
            var nap = ui.PixelSprite(stage, "retry_nap", new PixelPlacement(NapTopLeft, NapDevicePixels));
            nap.material = FurRecolor.CreateInstance(ui.Art.FurRecolorMaterial);
            nap.gameObject.AddComponent<SpriteFrameAnimator>().Play(ui.Art.Find("retry_nap"));
            return nap.material;
        }

        private void ApplySavedFur(PetAppearanceCatalog catalog)
        {
            foreach (var species in new[] { PetSpecies.Cat, PetSpecies.Dog })
                FurRecolor.Apply(_fur, species, PetLook.From(catalog, species, PetProfileStore.Load(species)).Ramp);
        }
    }
}
