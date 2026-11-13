using Game.Motion;
using UnityEngine;

namespace Game.BoardArt
{
    /// <summary>
    /// Plays a MotionClip on a world-space object, relative to its rest pose:
    /// pt offsets (y down) become world units, scale multiplies the rest
    /// scale and alpha fades every child sprite. Removes itself when the clip
    /// ends; a new clip on the same object replaces the old one.
    /// </summary>
    public class TransformMotionPlayer : MonoBehaviour
    {
        private MotionClip _clip;
        private float _unitsPerPt;
        private float _elapsed;
        private Vector3 _restPosition;
        private Vector3 _restScale;
        private SpriteRenderer[] _renderers;

        public static void Play(GameObject target, MotionClip clip, float unitsPerPt)
        {
            Stop(target);
            target.AddComponent<TransformMotionPlayer>().Begin(clip, unitsPerPt);
        }

        public static void Stop(GameObject target)
        {
            foreach (var running in target.GetComponents<TransformMotionPlayer>())
                if (running.enabled) running.Finish();
        }

        private void Begin(MotionClip clip, float unitsPerPt)
        {
            _clip = clip;
            _unitsPerPt = unitsPerPt;
            _restPosition = transform.localPosition;
            _restScale = transform.localScale;
            _renderers = GetComponentsInChildren<SpriteRenderer>();
            Apply(clip.Evaluate(0f));
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            Apply(_clip.Evaluate(_elapsed));
            if (_clip.IsFinished(_elapsed)) Finish();
        }

        private void Apply(MotionPose pose)
        {
            transform.localPosition = _restPosition + new Vector3(pose.Offset.x, -pose.Offset.y, 0f) * _unitsPerPt;
            transform.localRotation = Quaternion.Euler(0f, 0f, -pose.Rotation);
            transform.localScale = Vector3.Scale(_restScale, new Vector3(pose.Scale.x, pose.Scale.y, 1f));
            if (_clip.Alpha != null) SetAlpha(pose.Alpha);
        }

        private void SetAlpha(float alpha)
        {
            foreach (var spriteRenderer in _renderers)
            {
                var color = spriteRenderer.color;
                spriteRenderer.color = new Color(color.r, color.g, color.b, alpha);
            }
        }

        private void Finish()
        {
            Apply(MotionPose.Rest);
            enabled = false;
            Destroy(this);
        }
    }
}
