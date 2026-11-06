using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Art
{
    /// <summary>
    /// Plays a SpriteAnimEntry on a UI Image from Update(), on unscaled time
    /// so menus and overlays keep animating while gameplay is frozen.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class SpriteFrameAnimator : MonoBehaviour
    {
        public float Elapsed { get; set; }

        private Image _image;
        private SpriteAnimEntry _entry;
        private bool _loop;
        private Action _onDone;

        public void Play(SpriteAnimEntry entry, bool loop = true, Action onDone = null)
        {
            _image ??= GetComponent<Image>();
            _entry = entry;
            _loop = loop;
            _onDone = onDone;
            Elapsed = 0f;
            ShowCurrentFrame();
        }

        private void Update()
        {
            if (_entry == null) return;
            Elapsed += Time.unscaledDeltaTime;
            ShowCurrentFrame();
            if (!_loop && FrameClock.IsFinished(Elapsed, _entry.Fps, _entry.Frames.Length)) Finish();
        }

        private void ShowCurrentFrame()
        {
            var count = _entry.Frames.Length;
            if (count == 0) return;
            var frame = _loop
                ? FrameClock.LoopingFrame(Elapsed, _entry.Fps, count)
                : FrameClock.OnceFrame(Elapsed, _entry.Fps, count);
            _image.sprite = _entry.Frames[frame];
        }

        private void Finish()
        {
            var onDone = _onDone;
            _entry = null;
            _onDone = null;
            onDone?.Invoke();
        }
    }
}
