using Game.Art;
using UnityEngine;

namespace Game.BoardArt
{
    /// <summary>A world-space baked animation played once, then removed (the bomb's explosion).</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class WorldFrameAnimator : MonoBehaviour
    {
        private SpriteAnimEntry _frames;
        private SpriteRenderer _renderer;
        private float _elapsed;

        public static void PlayOnce(GameObject target, SpriteAnimEntry frames)
        {
            var animator = target.AddComponent<WorldFrameAnimator>();
            animator._frames = frames;
            animator._renderer = target.GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            var count = _frames.Frames.Length;
            if (count == 0 || FrameClock.IsFinished(_elapsed, _frames.Fps, count)) { Destroy(gameObject); return; }
            _renderer.sprite = _frames.Frames[FrameClock.OnceFrame(_elapsed, _frames.Fps, count)];
        }
    }
}
