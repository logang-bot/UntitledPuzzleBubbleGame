using Game.Art;
using Game.Grid;
using Game.Motion;
using UnityEngine;

namespace Game.BoardArt
{
    /// <summary>
    /// The backdrops' small life (bg_lamp_flicker, bg_fireflies, bg_clouds,
    /// bg_twinkle): lamp glows, fireflies, drifting clouds and twinkling
    /// sparkles, placed in screen pt on a backdrop holder that is already
    /// scaled to pt. Positions are seeded so a theme always looks the same.
    /// </summary>
    public static class BackgroundAmbience
    {
        private const float CloudSpeedPt = 4f;
        private const float WrapPt = 424f;

        public static void Populate(Transform holder, BoardTheme theme, MenuArtLibrary art)
        {
            var scene = new AmbientScene(holder, art);
            if (theme == BoardTheme.Living) AddLamp(scene, new Vector2(44f, 288f), 1f);
            if (theme == BoardTheme.Park) AddLamp(scene, new Vector2(260f, 434f), 0.8f);
            if (theme is BoardTheme.Garden or BoardTheme.Park) AddFireflies(scene);
            if (theme == BoardTheme.Rooftop) AddClouds(scene);
            if (theme is BoardTheme.Rooftop or BoardTheme.Park) AddSparkles(scene);
        }

        private static void AddLamp(AmbientScene scene, Vector2 screenPt, float strength)
        {
            var clip = new MotionClip
            {
                Alpha = new SineTrack(0.12f * strength, 3.1f, (0f, 0.85f * strength)),
                ScaleX = new SineTrack(0.03f, 3.1f, (0f, strength)),
                ScaleY = new SineTrack(0.03f, 3.1f, (0f, strength))
            };
            scene.Add("bg_lamp_glow", screenPt, clip);
        }

        private static void AddFireflies(AmbientScene scene)
        {
            for (var i = 0; i < 7; i++)
            {
                var spot = new Vector2(30f + Seed.Unit(i, 1) * 300f, 140f + Seed.Unit(i, 2) * 400f);
                scene.Add("bg_firefly", spot, Firefly(i));
            }
        }

        private static MotionClip Firefly(int i) => new MotionClip
        {
            X = new SineTrack(8f, 7f, (Seed.Unit(i, 3), 0f)),
            Y = new SineTrack(6f, 5f, (Seed.Unit(i, 4), 0f)),
            Alpha = new SineTrack(0.5f, 2f + Seed.Unit(i, 5) * 2f, (Seed.Unit(i, 6), 0.5f))
        };

        private static void AddClouds(AmbientScene scene)
        {
            var drift = new MotionClip { X = new KeyTrack(new MotionKey(0f, 0f), new MotionKey(WrapPt / CloudSpeedPt, WrapPt)), Duration = WrapPt / CloudSpeedPt, Loop = true };
            for (var i = 0; i < 3; i++)
            {
                var start = new Vector2(Seed.Unit(i, 7) * WrapPt - 12f, 150f + i * 70f);
                scene.Add("bg_cloud", start, drift);
                scene.Add("bg_cloud", start - new Vector2(WrapPt, 0f), drift);
            }
        }

        private static void AddSparkles(AmbientScene scene)
        {
            for (var i = 0; i < 6; i++)
            {
                var spot = new Vector2(20f + Seed.Unit(i, 8) * 320f, 130f + Seed.Unit(i, 9) * 220f);
                scene.Add("sparkle", spot, MenuMotions.StarTwinkle(1.6f + Seed.Unit(i, 10) * 1.6f, Seed.Unit(i, 11)));
            }
        }

        private readonly struct AmbientScene
        {
            private readonly Transform _holder;
            private readonly MenuArtLibrary _art;

            public AmbientScene(Transform holder, MenuArtLibrary art)
            {
                _holder = holder;
                _art = art;
            }

            public void Add(string id, Vector2 screenPt, MotionClip clip)
            {
                var sprite = SpriteChild.Create(_holder, id, _art.Frame(id, 0), BoardSorting.BackgroundAmbience);
                sprite.transform.localPosition = new Vector3(screenPt.x + BackgroundView.BleedPt, -(screenPt.y + BackgroundView.BleedPt), 0f);
                TransformMotionPlayer.Play(sprite, clip, 1f);
            }
        }
    }
}
