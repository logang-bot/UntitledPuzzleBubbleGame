using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    /// Attached to a freshly spawned bubble on level load so it scales in
    /// instead of appearing instantly, delayed by its row so the board
    /// fills in top-to-bottom. See
    /// docs/features/level-content/specs/2026-09-11-curated-levels-and-transitions-design.md.
    /// </summary>
    public class BubbleSpawnAnimator : MonoBehaviour
    {
        private const float ScaleInDurationSeconds = 0.25f;

        private float _delayRemaining;
        private float _elapsed;

        public void Configure(float delaySeconds)
        {
            _delayRemaining = delaySeconds;
            transform.localScale = Vector3.zero;
        }

        private void Update()
        {
            if (_delayRemaining > 0f) { _delayRemaining -= Time.deltaTime; return; }
            AdvanceScaleIn();
        }

        private void AdvanceScaleIn()
        {
            _elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(_elapsed / ScaleInDurationSeconds);
            var scale = BubbleSpawnMotion.ScaleForProgress(t);
            transform.localScale = new Vector3(scale, scale, 1f);
            if (t >= 1f) Destroy(this);
        }
    }
}
