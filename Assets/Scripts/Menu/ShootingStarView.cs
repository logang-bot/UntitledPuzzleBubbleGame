using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Menu
{
    /// <summary>
    /// The shooting star: every 6 s it streaks for 0.7 s at 420 pt/s, 20–35°
    /// below horizontal, left or right at random, fading in and out (alpha
    /// follows a half sine). The streak sprite points away from travel.
    /// </summary>
    public class ShootingStarView
    {
        private readonly Image _image;
        private int _flight = int.MinValue;
        private Vector2 _center;
        private Vector2 _direction;

        public ShootingStarView(MenuUiFactory ui, RectTransform parent)
        {
            _image = ui.PivotSprite(parent, CosmosSprites.ShootingStar, Vector2.zero);
        }

        public void Tick(float time)
        {
            var flight = Mathf.FloorToInt(time / CosmosSprites.ShootingStarEvery);
            var age = time - flight * CosmosSprites.ShootingStarEvery;
            _image.enabled = age < CosmosSprites.ShootingStarLife;
            if (!_image.enabled) return;
            if (flight != _flight) Repick(flight);
            var travel = CosmosSprites.ShootingStarSpeed * (age - CosmosSprites.ShootingStarLife / 2f);
            _image.rectTransform.anchoredPosition = UiPlacement.SmoothPosition(_center + _direction * travel);
            _image.color = new Color(1f, 1f, 1f, Mathf.Sin(Mathf.PI * age / CosmosSprites.ShootingStarLife));
        }

        private void Repick(int flight)
        {
            _flight = flight;
            var random = new System.Random(unchecked(flight * 7919 + 41));
            var angle = Mathf.Lerp(CosmosSprites.ShootingStarAngles.x, CosmosSprites.ShootingStarAngles.y, (float)random.NextDouble());
            var sign = random.NextDouble() < 0.5 ? 1f : -1f;
            _center = new Vector2(60f + (float)random.NextDouble() * 240f, 60f + (float)random.NextDouble() * 260f);
            _direction = new Vector2(sign * Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            _image.rectTransform.localEulerAngles = new Vector3(0f, 0f, sign > 0f ? 180f - angle : angle);
        }
    }
}
