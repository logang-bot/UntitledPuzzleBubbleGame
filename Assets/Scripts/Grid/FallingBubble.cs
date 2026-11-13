using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    /// Attached to a bubble that lost its connection to the ceiling (the
    /// guide's fall_scared): a short jitter, then a toss with gravity and
    /// spin, removed once it drops below the view. Speeds are in pt.
    /// </summary>
    public class FallingBubble : MonoBehaviour
    {
        private const float JitterSeconds = 0.14f;
        private const float JitterAmplitudePt = 1.2f;
        private const float JitterHz = 30f;
        private const float GravityPt = 1400f;
        private const float MaxLifetimeSeconds = 3f;

        private float _unitsPerPt;
        private float _floorY;
        private float _delay;
        private float _elapsed;
        private Vector3 _origin;
        private Vector2 _velocityPt;
        private float _spin;

        public void Configure(float unitsPerPt, float floorY, float delaySeconds)
        {
            _unitsPerPt = unitsPerPt;
            _floorY = floorY;
            _delay = delaySeconds;
            _origin = transform.position;
            _velocityPt = new Vector2(Random.Range(-50f, 50f), Random.Range(60f, 160f));
            _spin = Random.Range(-80f, 80f);
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            var time = _elapsed - _delay;
            if (time < 0f) return;
            if (time < JitterSeconds) Jitter(time);
            else Toss(Time.deltaTime);
            if (transform.position.y < _floorY || _elapsed > MaxLifetimeSeconds) Destroy(gameObject);
        }

        private void Jitter(float time)
        {
            var offsetPt = JitterAmplitudePt * Mathf.Sin(2f * Mathf.PI * JitterHz * time);
            transform.position = _origin + new Vector3(offsetPt * _unitsPerPt, 0f, 0f);
        }

        private void Toss(float deltaTime)
        {
            _velocityPt.y -= GravityPt * deltaTime;
            transform.position += (Vector3)(_velocityPt * (_unitsPerPt * deltaTime));
            transform.Rotate(0f, 0f, _spin * deltaTime);
        }
    }
}
