using System.Collections.Generic;
using Game.Art;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Results
{
    /// <summary>
    /// Victory particles from the menu art guide: confetti falling 40 px/s,
    /// firework bursts at fixed spots and sky lanterns rising 12 px/s with a
    /// ±3 px sway. Everything loops on unscaled time.
    /// </summary>
    public class CelebrationParticles : MonoBehaviour
    {
        private const float ConfettiSpeed = 40f;
        private const float LanternSpeed = 12f;
        private const float LanternSway = 3f;
        private const float FireworkPeriod = 1.6f;
        private static readonly string[] ConfettiIds = { "confetti_red", "confetti_gold", "confetti_blue", "confetti_leaf" };
        private static readonly (string Id, Vector2 Spot)[] Fireworks =
        {
            ("firework_gold", new Vector2(44, 74)), ("firework_red", new Vector2(222, 92)), ("firework_blue", new Vector2(132, 52))
        };
        private static readonly Vector2[] LanternSpots = { new Vector2(24, 520), new Vector2(244, 470), new Vector2(60, 430) };

        private readonly List<Particle> _confetti = new();
        private readonly List<Particle> _fireworks = new();
        private readonly List<Particle> _lanterns = new();
        private float _fieldHeight;
        private float _time;

        private class Particle
        {
            public Image Image;
            public SpriteAnimEntry Entry;
            public Vector2 Origin;
            public float Phase;
        }

        public static CelebrationParticles Create(MenuUiFactory ui, RectTransform parent, CelebrationSpec spec)
        {
            var layer = RuntimeUi.CreateRect(parent, "Celebration", UiPlacement.Fill());
            var particles = layer.gameObject.AddComponent<CelebrationParticles>();
            particles._fieldHeight = spec.FieldHeight;
            var random = new System.Random(9);
            for (var i = 0; i < spec.ConfettiCount; i++)
                particles._confetti.Add(Spawn(ui, layer, (ConfettiIds[i % ConfettiIds.Length], new Vector2(random.Next(270), random.Next((int)spec.FieldHeight)))));
            if (spec.WithFireworksAndLanterns) particles.SpawnSkyShow(ui, layer);
            return particles;
        }

        private void SpawnSkyShow(MenuUiFactory ui, RectTransform layer)
        {
            for (var i = 0; i < Fireworks.Length; i++)
                _fireworks.Add(Spawn(ui, layer, Fireworks[i], i * FireworkPeriod / Fireworks.Length));
            for (var i = 0; i < LanternSpots.Length; i++)
                _lanterns.Add(Spawn(ui, layer, ("lantern", LanternSpots[i]), i * 2.1f));
        }

        private static Particle Spawn(MenuUiFactory ui, RectTransform layer, (string Id, Vector2 Origin) source, float phase = 0f)
        {
            var entry = ui.Art.Find(source.Id);
            var image = RuntimeUi.CreateImage(layer, source.Id, UiPlacement.CenteredAt(source.Origin, entry.First.rect.size));
            image.raycastTarget = false;
            return new Particle { Image = image, Entry = entry, Origin = source.Origin, Phase = phase + source.Origin.x };
        }

        private void Update()
        {
            _time += Time.unscaledDeltaTime;
            foreach (var piece in _confetti) MoveConfetti(piece);
            foreach (var burst in _fireworks) ShowFirework(burst);
            foreach (var lantern in _lanterns) MoveLantern(lantern);
        }

        private void MoveConfetti(Particle piece)
        {
            var y = ParticleMotion.Wrap(piece.Origin.y + _time * ConfettiSpeed, 0f, _fieldHeight);
            Place(piece, new Vector2(piece.Origin.x, y), FrameClock.LoopingFrame(_time + piece.Phase, piece.Entry.Fps, piece.Entry.Frames.Length));
        }

        private void ShowFirework(Particle burst)
        {
            var frame = ParticleMotion.BurstFrame(_time + burst.Phase, FireworkPeriod, (burst.Entry.Frames.Length, burst.Entry.Fps));
            burst.Image.enabled = frame >= 0;
            if (frame >= 0) burst.Image.sprite = burst.Entry.Frames[frame];
        }

        private void MoveLantern(Particle lantern)
        {
            var y = ParticleMotion.Wrap(lantern.Origin.y - _time * LanternSpeed, -20f, _fieldHeight + 40f);
            var x = lantern.Origin.x + ParticleMotion.Sway(_time, LanternSway, lantern.Phase);
            Place(lantern, new Vector2(x, y), FrameClock.LoopingFrame(_time, lantern.Entry.Fps, lantern.Entry.Frames.Length));
        }

        private static void Place(Particle particle, Vector2 nativeCenter, int frame)
        {
            particle.Image.rectTransform.anchoredPosition = UiPlacement.NativePosition(nativeCenter);
            particle.Image.sprite = particle.Entry.Frames[frame];
        }
    }
}
