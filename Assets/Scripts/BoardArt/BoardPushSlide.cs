using Game.Motion;
using UnityEngine;

namespace Game.BoardArt
{
    /// <summary>
    /// The ceiling push (ceiling_push): the board logic moves a row down at
    /// once, so the visuals start one row higher and ease down over 0.35 s
    /// (outCubic). Sits on the root that holds the board's sprites.
    /// </summary>
    public class BoardPushSlide : MonoBehaviour
    {
        private float _distance;
        private float _elapsed = GameMotions.PushSlideSeconds;

        public void Play(float rowHeight)
        {
            _distance = rowHeight;
            _elapsed = 0f;
            Apply();
        }

        private void Update()
        {
            if (_elapsed >= GameMotions.PushSlideSeconds) return;
            _elapsed += Time.deltaTime;
            Apply();
        }

        private void Apply()
        {
            var progress = Mathf.Clamp01(_elapsed / GameMotions.PushSlideSeconds);
            transform.localPosition = new Vector3(0f, _distance * (1f - Ease.Apply(EaseKind.OutCubic, progress)), 0f);
        }
    }
}
