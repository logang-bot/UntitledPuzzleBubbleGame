using Game.Grid;
using UnityEngine;

namespace Game.Shooter
{
    /// <summary>
    /// Ghost bubble shown at the cell a shot would land in during aiming -
    /// resolved via the same fire-time-accurate BubbleLandingResolver logic
    /// FiredBubbleController uses, so it can never disagree with a real shot.
    /// Allowed to pop discretely between cells frame to frame; unlike the
    /// trajectory line, a snapping discrete marker reads as normal, not
    /// laggy - see docs/features/core-gameplay/shooter-and-trajectory.md.
    /// </summary>
    public class LandingIndicator
    {
        private const float Alpha = 0.45f;

        private readonly SpriteRenderer _renderer;

        public LandingIndicator()
        {
            _renderer = CreateRenderer();
        }

        public void Show(Vector2 worldPosition)
        {
            _renderer.transform.position = worldPosition;
            _renderer.enabled = true;
        }

        public void Hide()
        {
            _renderer.enabled = false;
        }

        public void Destroy()
        {
            Object.Destroy(_renderer.gameObject);
        }

        private static SpriteRenderer CreateRenderer()
        {
            var go = new GameObject("LandingIndicator");
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = CircleSpriteFactory.CreateWhiteCircle();
            renderer.color = new Color(1f, 1f, 1f, Alpha);
            renderer.enabled = false;
            return renderer;
        }
    }
}
