using Game.Art;
using Game.Pets;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Deck
{
    /// <summary>
    /// One pixel pet at the launcher, drawn from the game pet sheets at 5
    /// device px per art px on whole screen pixels and recolored with its
    /// saved fur. Every frame sits on the rig's floor (art y 84), left-aligned
    /// to its spot, right-aligned for the right hand, so taller front poses
    /// rise above it and wider ones grow away from the pointer.
    /// </summary>
    public class PetRigView : MonoBehaviour
    {
        private const int DesignDevicePixels = 5;
        private const float FloorArtY = DeckLayout.PetArtY + 32f;

        private MenuArtLibrary _art;
        private Image _image;
        private PixelSnap _snap;
        private DeckGeometry _geometry;
        private float _artX;
        private Vector2 _artSize;

        public static PetRigView Create(DeckUi ui, RectTransform layer, (DeckGeometry Geometry, float ArtX, PetLook Look) spot)
        {
            var image = RuntimeUi.CreateImage(layer, "Pet_" + spot.Look.SpeciesId, UiPlacement.PivotAt(Vector2.zero, Vector2.zero, new Vector2(0.5f, 0f)));
            image.raycastTarget = false;
            image.material = FurRecolor.CreateInstance(ui.Art.FurRecolorMaterial);
            FurRecolor.Apply(image.material, spot.Look.Species, spot.Look.Ramp);
            var view = image.gameObject.AddComponent<PetRigView>();
            (view._art, view._image, view._geometry, view._artX) = (ui.Art, image, spot.Geometry, spot.ArtX);
            view._snap = PixelSnap.Attach(image.rectTransform, Vector2.one, DesignDevicePixels);
            return view;
        }

        public void Show(PetPose pose)
        {
            var entry = _art.Find(pose.Id);
            if (entry.Frames.Length == 0) return;
            _image.sprite = entry.Frames[FrameOf(pose, entry)];
            Fit(_image.sprite.rect.size);
            _image.rectTransform.localScale = new Vector3(pose.Flip ? -1f : 1f, 1f, 1f);
        }

        private static int FrameOf(PetPose pose, SpriteAnimEntry entry)
        {
            var count = entry.Frames.Length;
            if (pose.FixedFrame != PetPose.NoFixedFrame) return Mathf.Clamp(pose.FixedFrame, 0, count - 1);
            return pose.Once ? FrameClock.OnceFrame(pose.Time, entry.Fps, count) : FrameClock.LoopingFrame(pose.Time, entry.Fps, count);
        }

        private void Fit(Vector2 artSize)
        {
            if (artSize == _artSize) return;
            _artSize = artSize;
            _snap.SetArtSize(artSize);
            var centerX = _geometry.PetLeft(_artX, artSize.x) + artSize.x * DeckLayout.PtPerArtPx * 0.5f;
            var floor = _geometry.MechanismTop + FloorArtY * DeckLayout.PtPerArtPx;
            _image.rectTransform.anchoredPosition = UiPlacement.SmoothPosition(new Vector2(centerX, floor));
        }
    }
}
