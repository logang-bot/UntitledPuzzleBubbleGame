using Game.Art;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Pets
{
    /// <summary>
    /// The 80×80 portrait pet on the customization stage: idles in a loop,
    /// hops once (happy) whenever the player picks something, recolored with
    /// the chosen fur through its own material instance.
    /// </summary>
    public class PortraitView
    {
        private const string Idle = "idle";
        private const string Happy = "happy";

        private readonly MenuUiFactory _ui;
        private readonly SpriteFrameAnimator _animator;
        private readonly Material _fur;
        private PetLook _look;

        public PortraitView(MenuUiFactory ui, RectTransform parent, RectInt rect)
        {
            _ui = ui;
            var image = RuntimeUi.CreateImage(parent, "Portrait", UiPlacement.TopLeft(rect));
            image.raycastTarget = false;
            _fur = FurRecolor.CreateInstance(ui.Art.FurRecolorMaterial);
            image.material = _fur;
            _animator = image.gameObject.AddComponent<SpriteFrameAnimator>();
        }

        public void Show(PetLook look)
        {
            _look = look;
            FurRecolor.Apply(_fur, look.Species, look.Ramp);
            PlayIdle();
        }

        public void Celebrate()
        {
            _animator.Play(_ui.Art.Find(_look.PortraitId(Happy)), loop: false, onDone: PlayIdle);
        }

        private void PlayIdle() => _animator.Play(_ui.Art.Find(_look.PortraitId(Idle)));
    }
}
