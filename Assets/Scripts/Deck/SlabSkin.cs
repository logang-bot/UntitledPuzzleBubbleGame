using Game.Shooter;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Deck
{
    /// <summary>Shows a rotate slab's held face while its zone is touched (btn_hold).</summary>
    public class SlabSkin : MonoBehaviour
    {
        private Image _image;
        private HoldInputZone _zone;
        private (Sprite Up, Sprite Held) _faces;

        public static void Attach(Image image, HoldInputZone zone, (Sprite Up, Sprite Held) faces)
        {
            var skin = image.gameObject.AddComponent<SlabSkin>();
            skin._image = image;
            skin._zone = zone;
            skin._faces = faces;
        }

        private void Update() => _image.sprite = _zone.IsPressed ? _faces.Held : _faces.Up;
    }
}
