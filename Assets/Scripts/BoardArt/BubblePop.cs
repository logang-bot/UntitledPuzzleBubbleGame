using Game.Art;
using UnityEngine;

namespace Game.BoardArt
{
    /// <summary>
    /// Turns a popped bubble into its baked burst after a delay: the body
    /// sprite plays the pop frames once (scaled like the bubble), the glyph
    /// and overlays go, and the object removes itself at the last frame.
    /// </summary>
    public class BubblePop : MonoBehaviour
    {
        private SpriteAnimEntry _frames;
        private SpriteRenderer _body;
        private float _delay;
        private float _elapsed;
        private bool _bursting;

        public static void Begin(GameObject bubble, SpriteAnimEntry frames, float delaySeconds)
        {
            var pop = bubble.AddComponent<BubblePop>();
            pop._frames = frames;
            pop._delay = delaySeconds;
            pop._body = bubble.GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed < _delay) return;
            if (!_bursting) Burst();
            else ShowFrame(_elapsed - _delay);
        }

        private void Burst()
        {
            _bursting = true;
            foreach (Transform child in transform) Destroy(child.gameObject);
            ShowFrame(0f);
        }

        private void ShowFrame(float time)
        {
            var count = _frames.Frames.Length;
            if (count == 0 || FrameClock.IsFinished(time, _frames.Fps, count)) { Destroy(gameObject); return; }
            _body.sprite = _frames.Frames[FrameClock.OnceFrame(time, _frames.Fps, count)];
        }
    }
}
