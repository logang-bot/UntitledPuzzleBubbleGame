using Game.Art;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Results
{
    /// <summary>
    /// The victory fireworks (motion.json "fireworks"): a new burst every
    /// 0.7 s at a random spot in the top band (x 40–320, y 70–270), scaled
    /// 0.7–1.1, cycling gold, coral and teal. Each plays its 10 baked frames
    /// once at 15 fps, so up to three overlap. Burst k always uses image k mod 3.
    /// </summary>
    public class FireworkShow : MonoBehaviour
    {
        private const float Every = 0.7f;
        private static readonly string[] Sprites = { "firework_gold", "firework_coral", "firework_teal" };
        private static readonly Rect Area = Rect.MinMaxRect(40f, 70f, 320f, 270f);

        private readonly Image[] _bursts = new Image[3];
        private readonly SpriteAnimEntry[] _entries = new SpriteAnimEntry[3];
        private float _time;

        public static FireworkShow Create(MenuUiFactory ui, RectTransform parent)
        {
            var layer = RuntimeUi.CreateRect(parent, "Fireworks", UiPlacement.Fill());
            var show = layer.gameObject.AddComponent<FireworkShow>();
            for (var i = 0; i < Sprites.Length; i++)
            {
                show._entries[i] = ui.Art.Find(Sprites[i]);
                show._bursts[i] = ui.PivotSprite(layer, Sprites[i], Vector2.zero);
            }
            show.Update();
            return show;
        }

        private void Update()
        {
            _time += Time.unscaledDeltaTime;
            var newest = Mathf.FloorToInt(_time / Every);
            for (var burst = newest - 2; burst <= newest; burst++)
                Show(burst);
        }

        private void Show(int burst)
        {
            var slot = (burst % 3 + 3) % 3;
            var frame = burst < 0 ? -1 : ParticleMotion.BurstFrame(_time - burst * Every, (_entries[slot].Frames.Length, _entries[slot].Fps));
            _bursts[slot].enabled = frame >= 0;
            if (frame < 0) return;
            _bursts[slot].sprite = _entries[slot].Frames[frame];
            Place(_bursts[slot].rectTransform, new System.Random(unchecked(burst * 7919 + 31)));
        }

        private static void Place(RectTransform burst, System.Random random)
        {
            var spot = new Vector2(ParticleMotion.Between(random, Area.xMin, Area.xMax), ParticleMotion.Between(random, Area.yMin, Area.yMax));
            burst.anchoredPosition = UiPlacement.SmoothPosition(spot);
            burst.localScale = Vector3.one * ParticleMotion.Between(random, 0.7f, 1.1f);
        }
    }
}
