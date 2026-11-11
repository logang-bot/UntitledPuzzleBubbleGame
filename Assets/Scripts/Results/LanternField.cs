using System.Collections.Generic;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Results
{
    /// <summary>
    /// Five sky lanterns rising through the victory overlay (motion.json
    /// "lanterns"): 18–26 pt/s, swaying ±6 pt over 3 s and rocking ±4°, each
    /// over a glow that pulses 0.8 ± 0.2 over 1.8 s, scaled 0.8–1.2. They
    /// re-enter below the screen after leaving the top.
    /// </summary>
    public class LanternField : MonoBehaviour
    {
        private const int Count = 5;
        private const float Height = 780f;
        private const float SwayAmplitude = 6f;
        private const float SwayPeriod = 3f;
        private const float GlowPeriod = 1.8f;

        private readonly List<Lantern> _lanterns = new();
        private float _time;

        private class Lantern
        {
            public RectTransform Root;
            public Image Glow;
            public float X;
            public float Rise;
            public float Offset;
            public float Phase;
        }

        public static LanternField Create(MenuUiFactory ui, RectTransform parent)
        {
            var layer = RuntimeUi.CreateRect(parent, "Lanterns", UiPlacement.Fill());
            var field = layer.gameObject.AddComponent<LanternField>();
            var random = new System.Random(5);
            for (var i = 0; i < Count; i++)
                field._lanterns.Add(Spawn(ui, layer, (i, random)));
            field.Update();
            return field;
        }

        private static Lantern Spawn(MenuUiFactory ui, RectTransform layer, (int Index, System.Random Random) seed)
        {
            var lantern = Roll(seed);
            lantern.Root = RuntimeUi.CreateRect(layer, "Lantern", UiPlacement.CenteredAt(Vector2.zero, Vector2.zero));
            lantern.Glow = ui.PivotSprite(lantern.Root, "lantern_glow", Vector2.zero);
            ui.PivotSprite(lantern.Root, "lantern", Vector2.zero);
            lantern.Root.localScale = Vector3.one * ParticleMotion.Between(seed.Random, 0.8f, 1.2f);
            return lantern;
        }

        private static Lantern Roll((int Index, System.Random Random) seed) => new Lantern
        {
            X = 30f + seed.Index * 72f + ParticleMotion.Between(seed.Random, 0f, 20f),
            Rise = ParticleMotion.Between(seed.Random, 18f, 26f),
            Offset = (float)seed.Random.NextDouble(),
            Phase = (float)seed.Random.NextDouble()
        };

        private void Update()
        {
            _time += Time.unscaledDeltaTime;
            for (var i = 0; i < _lanterns.Count; i++)
                Place(_lanterns[i], i);
        }

        /// <summary>The guide's loop: y = (H + 50) − ((t·rise + offset·(H + 80)) mod (H + 100)), H the 780 pt screen.</summary>
        private void Place(Lantern lantern, int index)
        {
            var y = Height + 50f - Mathf.Repeat(_time * lantern.Rise + lantern.Offset * (Height + 80f), Height + 100f);
            var x = lantern.X + SwayAmplitude * ParticleMotion.Wave(_time, SwayPeriod, lantern.Phase);
            lantern.Root.anchoredPosition = UiPlacement.SmoothPosition(new Vector2(x, y));
            lantern.Root.localEulerAngles = new Vector3(0f, 0f, -4f * Mathf.Sin(_time * 1.3f + index));
            lantern.Glow.color = new Color(1f, 1f, 1f, 0.8f + 0.2f * ParticleMotion.Wave(_time, GlowPeriod, lantern.Phase));
        }
    }
}
