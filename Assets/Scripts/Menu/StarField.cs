using System.Collections.Generic;
using Game.Motion;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Menu
{
    /// <summary>
    /// The seamless star layer drifting left at 6 pt/s (two copies wrapping
    /// every 424 pt, placed with the 32 pt bleed) and 14 twinkling sparkles at
    /// fixed spots, each with its own period (1.6–3.2 s), phase and size.
    /// Shared by the main menu, customization, settings and retry screens.
    /// </summary>
    public class StarField : MonoBehaviour
    {
        private const float Speed = 6f;
        private const float WrapWidth = 424f;
        private const int SparkleCount = 14;
        private static readonly RectInt LayerRect = new RectInt(-32, -32, 424, 844);

        private readonly List<(Image Image, MotionClip Twinkle, float Size)> _sparkles = new();
        private readonly List<Image> _layers = new();

        public static StarField Create(MenuUiFactory ui, RectTransform stage)
        {
            var root = RuntimeUi.CreateRect(stage, "StarField", UiPlacement.Fill());
            var field = root.gameObject.AddComponent<StarField>();
            field._layers.Add(ui.Sprite(root, "space_stars", LayerRect));
            field._layers.Add(ui.Sprite(root, "space_stars", LayerRect));
            field.SpawnSparkles(ui, root);
            field.Update();
            return field;
        }

        private void SpawnSparkles(MenuUiFactory ui, RectTransform root)
        {
            var random = new System.Random(7);
            for (var i = 0; i < SparkleCount; i++)
            {
                var spot = new Vector2(16f + Next(random) * 328f, 20f + Next(random) * 740f);
                var sparkle = ui.PivotSprite(root, "sparkle", spot);
                _sparkles.Add((sparkle, MenuMotions.StarTwinkle(1.6f + Next(random) * 1.6f, Next(random)), 0.6f + Next(random) * 0.6f));
            }
        }

        private static float Next(System.Random random) => (float)random.NextDouble();

        private void Update()
        {
            var time = Time.realtimeSinceStartup;
            var offset = time * Speed % WrapWidth;
            for (var i = 0; i < _layers.Count; i++)
                _layers[i].rectTransform.anchoredPosition = new Vector2(LayerRect.x - offset + i * WrapWidth, -LayerRect.y);
            foreach (var sparkle in _sparkles)
                Twinkle(sparkle, time);
        }

        private static void Twinkle((Image Image, MotionClip Twinkle, float Size) sparkle, float time)
        {
            var pose = sparkle.Twinkle.Evaluate(time);
            sparkle.Image.rectTransform.localScale = new Vector3(pose.Scale.x * sparkle.Size, pose.Scale.y * sparkle.Size, 1f);
            sparkle.Image.color = new Color(1f, 1f, 1f, Mathf.Clamp01(pose.Alpha));
        }
    }
}
