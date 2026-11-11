using System.Collections.Generic;
using Game.UI;
using UnityEngine;

namespace Game.Results
{
    /// <summary>
    /// Falling cartoon confetti (motion.json "confetti"): each piece falls at
    /// 50–90 pt/s and wraps, sways ±12 pt over 1.6 s, spins 180–420°/s either
    /// way, and fakes a 3D flip by swinging its x scale over 0.6 s.
    /// </summary>
    public class ConfettiField : MonoBehaviour
    {
        private const float WrapMargin = 20f;
        private const float SwayAmplitude = 12f;
        private const float SwayPeriod = 1.6f;
        private const float FlipPeriod = 0.6f;
        private static readonly string[] Sprites = { "confetti_gold", "confetti_coral", "confetti_teal", "confetti_violet" };

        private readonly List<Piece> _pieces = new();
        private Vector2 _field;
        private float _time;

        private class Piece
        {
            public RectTransform Rect;
            public Vector2 Origin;
            public float Fall;
            public float Spin;
            public float Phase;
        }

        public static ConfettiField Create(MenuUiFactory ui, RectTransform parent, (Vector2 Field, int Count) spec)
        {
            var layer = RuntimeUi.CreateRect(parent, "Confetti", UiPlacement.TopLeft(new RectInt(0, 0, (int)spec.Field.x, (int)spec.Field.y)));
            var field = layer.gameObject.AddComponent<ConfettiField>();
            field._field = spec.Field;
            var random = new System.Random(9);
            for (var i = 0; i < spec.Count; i++)
                field._pieces.Add(field.Spawn(ui, Sprites[i % Sprites.Length], random));
            field.Update();
            return field;
        }

        private Piece Spawn(MenuUiFactory ui, string spriteId, System.Random random) => new Piece
        {
            Rect = ui.PivotSprite((RectTransform)transform, spriteId, Vector2.zero).rectTransform,
            Origin = new Vector2(ParticleMotion.Between(random, 0f, _field.x), ParticleMotion.Between(random, 0f, _field.y + 2f * WrapMargin)),
            Fall = ParticleMotion.Between(random, 50f, 90f),
            Spin = ParticleMotion.Between(random, 180f, 420f) * (random.NextDouble() < 0.5 ? -1f : 1f),
            Phase = (float)random.NextDouble()
        };

        private void Update()
        {
            _time += Time.unscaledDeltaTime;
            foreach (var piece in _pieces)
                Place(piece);
        }

        private void Place(Piece piece)
        {
            var y = ParticleMotion.Wrap(piece.Origin.y + _time * piece.Fall, -WrapMargin, _field.y + 2f * WrapMargin);
            var x = piece.Origin.x + SwayAmplitude * ParticleMotion.Wave(_time, SwayPeriod, piece.Phase);
            piece.Rect.anchoredPosition = UiPlacement.SmoothPosition(new Vector2(x, y));
            piece.Rect.localEulerAngles = new Vector3(0f, 0f, -piece.Spin * _time);
            piece.Rect.localScale = new Vector3(Mathf.Cos(2f * Mathf.PI * (_time / FlipPeriod + piece.Phase)), 1f, 1f);
        }
    }
}
