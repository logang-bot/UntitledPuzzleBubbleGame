using Game.Art;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Menu
{
    /// <summary>
    /// A 320×633 background layer that scrolls left at a fixed speed and
    /// wraps every 320 px (two side-by-side copies), animating its frames.
    /// Placed at stage (−24, −24) so its bleed covers the 270×585 frame.
    /// </summary>
    public class LoopingLayer : MonoBehaviour
    {
        public const float WrapWidth = 320f;
        public static readonly RectInt LayerRect = new RectInt(-24, -24, 320, 633);

        private Image[] _copies;
        private SpriteAnimEntry _entry;
        private float _speed;

        public static LoopingLayer Create(MenuUiFactory ui, RectTransform parent, LoopingLayerSpec spec)
        {
            var container = RuntimeUi.CreateRect(parent, spec.SpriteId, UiPlacement.TopLeft(LayerRect));
            var layer = container.gameObject.AddComponent<LoopingLayer>();
            layer._entry = ui.Art.Find(spec.SpriteId);
            layer._speed = spec.Speed;
            layer._copies = new[] { SpawnCopy(ui, container, 0), SpawnCopy(ui, container, (int)WrapWidth) };
            layer.Update();
            return layer;
        }

        private static Image SpawnCopy(MenuUiFactory ui, RectTransform container, int x)
        {
            var copy = RuntimeUi.CreateImage(container, "Copy", UiPlacement.TopLeft(new RectInt(x, 0, LayerRect.width, LayerRect.height)));
            copy.raycastTarget = false;
            return copy;
        }

        private void Update()
        {
            var time = Time.realtimeSinceStartup;
            var offset = Mathf.Floor(time * _speed % WrapWidth);
            var frame = FrameClock.LoopingFrame(time, _entry.Fps, _entry.Frames.Length);
            for (var i = 0; i < _copies.Length; i++)
                ShowCopy(_copies[i], new Vector2(i * WrapWidth - offset, 0f), _entry.Frames[frame]);
        }

        private static void ShowCopy(Image copy, Vector2 position, Sprite sprite)
        {
            copy.rectTransform.anchoredPosition = position;
            copy.sprite = sprite;
        }
    }
}
