using Game.Art;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Menu
{
    /// <summary>
    /// One space pet on the main menu. Each cycle re-rolls a FloaterPick,
    /// recolors it with the picked colorway, turns/mirrors it, optionally
    /// wraps it in a bubble, and floats it across with a ±2 px bob.
    /// </summary>
    public class FloaterSlotView
    {
        private const float BobAmplitude = 2f;
        private const float BobSpeed = 1.4f;
        private const float EdgeMargin = 6f;

        private readonly FloaterContext _context;
        private readonly SpawnSlot _slot;
        private readonly RectTransform _root;
        private readonly Image _pet;
        private readonly Image _bubble;
        private readonly Material _fur;
        private FloaterPick _pick;
        private SpriteAnimEntry _entry;
        private CrossingPath _path;
        private int _cycle = int.MinValue;

        public FloaterSlotView(FloaterContext context, SpawnSlot slot)
        {
            _context = context;
            _slot = slot;
            _root = RuntimeUi.CreateRect(context.SizeLayers[0], "Floater" + slot.Id, UiPlacement.CenteredAt(Vector2.zero, Vector2.zero));
            _pet = CreateLayer("Pet");
            _fur = FurRecolor.CreateInstance(context.Ui.Art.FurRecolorMaterial);
            _pet.material = _fur;
            _bubble = CreateLayer("Bubble");
        }

        public void Tick(float time)
        {
            var visible = _slot.TryGetState(time, out var state);
            _root.gameObject.SetActive(visible);
            if (!visible) return;
            if (state.Cycle != _cycle) Repick(state.Cycle);
            var bob = new Vector2(0f, BobAmplitude * Mathf.Sin(time * BobSpeed + _pick.Phase));
            _root.anchoredPosition = UiPlacement.NativePosition(_path.At(state.Progress) + bob);
            _pet.sprite = _entry.Frames[(int)(time * _entry.Fps + _pick.Phase * 3f) % _entry.Frames.Length];
        }

        private Image CreateLayer(string name)
        {
            var image = RuntimeUi.CreateImage(_root, name, UiPlacement.At(new Vector2(0.5f, 0.5f), Vector2.zero));
            image.raycastTarget = false;
            return image;
        }

        private void Repick(int cycle)
        {
            _cycle = cycle;
            var random = _slot.RandomFor(cycle);
            _pick = FloaterPick.Roll(random);
            var appearance = _context.Catalog.For(_pick.Species);
            _entry = _context.Ui.Art.Find(_pick.SpriteId(appearance.Shapes[_pick.ShapeIndex].ArtId));
            FurRecolor.Apply(_fur, _pick.Species, appearance.Colors[_pick.ColorwayIndex % appearance.Colors.Count].ToRamp());
            ShowPet(_entry.First.rect.size);
            _path = CrossingPath.Pick(random, _entry.First.rect.size / 2f + Vector2.one * EdgeMargin, CrossingPath.RandomEdge(random));
        }

        private void ShowPet(Vector2 size)
        {
            _root.SetParent(_context.SizeLayers[(int)_pick.Size], worldPositionStays: false);
            _pet.rectTransform.sizeDelta = size;
            _pet.rectTransform.localEulerAngles = new Vector3(0f, 0f, -90f * _pick.QuarterTurns);
            _pet.rectTransform.localScale = new Vector3(_pick.Mirror ? -1f : 1f, 1f, 1f);
            ShowBubble();
        }

        private void ShowBubble()
        {
            _bubble.gameObject.SetActive(_pick.InBubble);
            if (!_pick.InBubble) return;
            _bubble.sprite = _context.Ui.Art.Frame("bubble", (int)_pick.Size);
            _bubble.rectTransform.sizeDelta = _bubble.sprite.rect.size;
        }
    }
}
