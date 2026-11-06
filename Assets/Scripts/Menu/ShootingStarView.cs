using Game.Art;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Menu
{
    /// <summary>
    /// The shooting-star slot: every 6 s it flashes for 0.9 s at a random
    /// spot, mirrored half the time; its 6 frames follow progress, not fps.
    /// </summary>
    public class ShootingStarView
    {
        private static readonly SpawnSlot Slot = new SpawnSlot { Id = 9, Period = 6f, Offset = 0.3f, VisibleShare = 0.15f };

        private readonly SpriteAnimEntry _entry;
        private readonly Image _image;
        private int _cycle = int.MinValue;

        public ShootingStarView(MenuUiFactory ui, RectTransform parent)
        {
            _entry = ui.Art.Find(CosmosSprites.ShootingStar);
            var size = _entry.First.rect.size;
            _image = RuntimeUi.CreateImage(parent, "ShootingStar", UiPlacement.CenteredAt(Vector2.zero, size));
            _image.raycastTarget = false;
        }

        public void Tick(float time)
        {
            var visible = Slot.TryGetState(time, out var state);
            _image.enabled = visible;
            if (!visible) return;
            if (state.Cycle != _cycle) Repick(state.Cycle);
            var frame = Mathf.Min(_entry.Frames.Length - 1, (int)(state.Progress * _entry.Frames.Length));
            _image.sprite = _entry.Frames[frame];
        }

        private void Repick(int cycle)
        {
            _cycle = cycle;
            var random = Slot.RandomFor(cycle);
            var spot = new Vector2(40f + (float)random.NextDouble() * 190f, 40f + (float)random.NextDouble() * 300f);
            _image.rectTransform.anchoredPosition = UiPlacement.NativePosition(spot);
            _image.rectTransform.localScale = new Vector3(random.NextDouble() < 0.5 ? -1f : 1f, 1f, 1f);
        }
    }
}
