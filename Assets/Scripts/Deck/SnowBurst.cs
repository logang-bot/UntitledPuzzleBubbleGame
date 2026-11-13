using System.Collections.Generic;
using Game.BoardArt;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Deck
{
    /// <summary>
    /// The Freeze snowflake burst (freeze_snow): 14 flakes fly out from a
    /// point at 160–280 pt/s, slowed by drag, spinning up to ±180°/s at
    /// 0.6–1.2× size, and fade over the last 0.4 s of their 1.1 s life.
    /// </summary>
    public class SnowBurst
    {
        private const int FlakeCount = 14;
        private const float Drag = 2.2f;
        private const float LifeSeconds = 1.1f;
        private const float FadeSeconds = 0.4f;

        private readonly List<(Image Flake, Vector2 Velocity, float Spin)> _flakes = new();
        private readonly Vector2 _origin;
        private float _age = LifeSeconds;

        public SnowBurst(DeckUi ui, RectTransform layer, Vector2 origin)
        {
            _origin = origin;
            for (var i = 0; i < FlakeCount; i++) _flakes.Add(NewFlake(ui, layer, i));
            Show(false);
        }

        public void Burst() => _age = 0f;

        /// <summary>A flake's distance after t seconds under drag k is v(1 − e^(−kt)) / k.</summary>
        public void Advance(float deltaTime)
        {
            _age += deltaTime;
            Show(_age < LifeSeconds);
            if (_age >= LifeSeconds) return;
            var travel = (1f - Mathf.Exp(-Drag * _age)) / Drag;
            var alpha = Mathf.Clamp01((LifeSeconds - _age) / FadeSeconds);
            foreach (var (flake, velocity, spin) in _flakes) Place(flake, (_origin + velocity * travel, spin * _age, alpha));
        }

        private static (Image, Vector2, float) NewFlake(DeckUi ui, RectTransform layer, int index)
        {
            var flake = ui.At(layer, "snowflake", Vector2.zero);
            flake.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.6f, 1.2f, Seed.Unit(index, 94));
            var angle = index * 2f * Mathf.PI / FlakeCount + Seed.Unit(index, 91);
            var speed = Mathf.Lerp(160f, 280f, Seed.Unit(index, 92));
            return (flake, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed, (Seed.Unit(index, 93) - 0.5f) * 360f);
        }

        private static void Place(Image flake, (Vector2 Point, float Rotation, float Alpha) pose)
        {
            flake.rectTransform.anchoredPosition = new Vector2(pose.Point.x, -pose.Point.y);
            flake.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -pose.Rotation);
            flake.color = new Color(1f, 1f, 1f, pose.Alpha);
        }

        private void Show(bool visible)
        {
            foreach (var (flake, _, _) in _flakes) flake.enabled = visible;
        }
    }
}
