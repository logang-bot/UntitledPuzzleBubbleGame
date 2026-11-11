using Game.Art;
using Game.UI;
using UnityEngine;

namespace Game.Pets
{
    /// <summary>
    /// The pixel portrait pet standing on the customization stage: its feet
    /// (legend pivot 40, 79) on the given point at 5 device px per art px,
    /// kept on whole screen pixels. Idles in a loop, hops once (happy) whenever
    /// the player picks something, recolored with the chosen fur through its
    /// own material instance.
    /// </summary>
    public class PortraitView
    {
        private const string Idle = "idle";
        private const string Happy = "happy";
        private const int DesignDevicePixels = 5;

        private readonly MenuUiFactory _ui;
        private readonly SpriteFrameAnimator _animator;
        private readonly Material _fur;
        private PetLook _look;

        public PortraitView(MenuUiFactory ui, RectTransform parent, (PetLook Look, Vector2 Feet) placement)
        {
            _ui = ui;
            var image = ui.PixelSprite(parent, placement.Look.PortraitId(Idle), new PixelPlacement(placement.Feet, DesignDevicePixels));
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
