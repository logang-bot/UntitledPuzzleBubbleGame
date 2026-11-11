using UnityEngine;

namespace Game.Motion
{
    /// <summary>
    /// Plays a MotionClip on its RectTransform from Update(), on unscaled time,
    /// relative to the rest pose captured when it starts. Alpha goes through a
    /// CanvasGroup, added on demand. Attach() captures the rest pose and
    /// waits; Play() starts (or restarts) the clip. A one-shot clip settles on
    /// its last pose and the player goes idle until Play() is called again.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class MotionPlayer : MonoBehaviour
    {
        private MotionClip _clip;
        private RectTransform _rect;
        private CanvasGroup _group;
        private Vector2 _restPosition;
        private Vector3 _restScale;
        private float _restRotation;
        private float _elapsed;

        public static MotionPlayer Attach(RectTransform rect, MotionClip clip)
        {
            var player = rect.gameObject.AddComponent<MotionPlayer>();
            player._rect = rect;
            player._clip = clip;
            player.CaptureRest();
            player.enabled = false;
            return player;
        }

        public static MotionPlayer AttachAndPlay(RectTransform rect, MotionClip clip)
        {
            var player = Attach(rect, clip);
            player.Play();
            return player;
        }

        public void Play()
        {
            _elapsed = 0f;
            enabled = true;
            Apply(_clip.Evaluate(0f));
        }

        private void CaptureRest()
        {
            _restPosition = _rect.anchoredPosition;
            _restScale = _rect.localScale;
            _restRotation = _rect.localEulerAngles.z;
            if (_clip.Alpha != null && !TryGetComponent(out _group)) _group = gameObject.AddComponent<CanvasGroup>();
        }

        private void Update()
        {
            _elapsed += Time.unscaledDeltaTime;
            Apply(_clip.Evaluate(_elapsed));
            if (_clip.IsFinished(_elapsed)) enabled = false;
        }

        private void Apply(MotionPose pose)
        {
            if (_clip.MovesPosition) _rect.anchoredPosition = _restPosition + new Vector2(pose.Offset.x, -pose.Offset.y);
            if (_clip.MovesRotation) _rect.localEulerAngles = new Vector3(0f, 0f, _restRotation - pose.Rotation);
            if (_clip.MovesScale) _rect.localScale = new Vector3(_restScale.x * pose.Scale.x, _restScale.y * pose.Scale.y, _restScale.z);
            if (_group != null) _group.alpha = Mathf.Clamp01(pose.Alpha);
        }
    }
}
