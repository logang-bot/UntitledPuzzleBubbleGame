using Game.Shooter;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Deck
{
    /// <summary>
    /// The deck's touch controls: two rotate slabs side by side on the thumb
    /// side (held), and the launcher pointer itself as the fire button
    /// (tapped), a circle of 27.5 pt radius.
    /// </summary>
    public static class DeckControls
    {
        public static (HoldInputZone RotateLeft, HoldInputZone RotateRight, HoldInputZone Fire) Build(DeckUi ui, MechanismParts parts)
        {
            var slabsX = parts.Geometry.RightHanded ? DeckLayout.RightHandSlabsX : DeckLayout.LeftHandSlabsX;
            var top = parts.Geometry.MechanismTop + DeckLayout.SlabTop;
            var left = Slab(ui, parts.ControlsLayer, ("rotate_left", new Vector2(slabsX, top)));
            var right = Slab(ui, parts.ControlsLayer, ("rotate_right", new Vector2(slabsX + DeckLayout.SlabSize.x, top)));
            return (left, right, PointerZone(parts));
        }

        private static HoldInputZone Slab(DeckUi ui, RectTransform layer, (string Id, Vector2 TopLeft) slab)
        {
            var image = ui.Sprite(layer, slab.Id + "_up", new Rect(slab.TopLeft, DeckLayout.SlabSize));
            image.raycastTarget = true;
            var zone = image.gameObject.AddComponent<HoldInputZone>();
            SlabSkin.Attach(image, zone, (ui.Art.Frame(slab.Id + "_up", 0), ui.Art.Frame(slab.Id + "_held", 0)));
            return zone;
        }

        private static HoldInputZone PointerZone(MechanismParts parts)
        {
            var size = Vector2.one * DeckLayout.PointerHitRadius * 2f;
            var image = RuntimeUi.CreateImage(parts.ControlsLayer, "FirePointer", UiPlacement.CenteredAt(parts.Geometry.Point(DeckLayout.Pointer), size));
            image.color = Color.clear;
            image.gameObject.AddComponent<CircleHitFilter>();
            return image.gameObject.AddComponent<HoldInputZone>();
        }
    }
}
