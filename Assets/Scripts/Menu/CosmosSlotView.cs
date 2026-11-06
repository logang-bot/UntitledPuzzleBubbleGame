using Game.Art;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Menu
{
    /// <summary>
    /// One drifting galaxy/planet slot. On each new cycle it picks a sprite
    /// and a horizontal crossing (the big slot squashed into its y band, the
    /// comet mirrored so its tail trails), then glides along it.
    /// </summary>
    public class CosmosSlotView
    {
        private readonly MenuUiFactory _ui;
        private readonly SpawnSlot _slot;
        private readonly bool _big;
        private readonly Image _image;
        private SpriteAnimEntry _entry;
        private CrossingPath _path;
        private int _cycle = int.MinValue;

        public CosmosSlotView(MenuUiFactory ui, RectTransform parent, CosmosSlotSpec spec)
        {
            _ui = ui;
            _slot = spec.Slot;
            _big = spec.Big;
            _image = RuntimeUi.CreateImage(parent, "CosmosSlot" + spec.Slot.Id, UiPlacement.CenteredAt(Vector2.zero, Vector2.zero));
            _image.raycastTarget = false;
        }

        public void Tick(float time)
        {
            var visible = _slot.TryGetState(time, out var state);
            _image.enabled = visible;
            if (!visible) return;
            if (state.Cycle != _cycle) Repick(state.Cycle);
            _image.sprite = _entry.Frames[FrameClock.LoopingFrame(time, _entry.Fps, _entry.Frames.Length)];
            _image.rectTransform.anchoredPosition = UiPlacement.NativePosition(_path.At(state.Progress));
        }

        private void Repick(int cycle)
        {
            _cycle = cycle;
            var random = _slot.RandomFor(cycle);
            var id = _big ? CosmosSprites.BigForCycle(cycle) : CosmosSprites.Small[random.Next(CosmosSprites.Small.Length)];
            _entry = _ui.Art.Find(id);
            var size = _entry.First.rect.size;
            _path = PickPath(random, size);
            _image.rectTransform.sizeDelta = size;
            _image.rectTransform.localScale = new Vector3(id == CosmosSprites.Comet && _path.MovesRight ? -1f : 1f, 1f, 1f);
        }

        private CrossingPath PickPath(System.Random random, Vector2 size)
        {
            var path = CrossingPath.Pick(random, size / 2f + Vector2.one * 2f, random.Next(2));
            return _big ? path.SquashedIntoBigBand() : path;
        }
    }
}
