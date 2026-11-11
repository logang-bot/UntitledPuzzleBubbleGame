using UnityEngine;

namespace Game.Motion
{
    /// <summary>One animated value of a MotionClip channel, sampled at a time in seconds.</summary>
    public interface IMotionTrack
    {
        float Evaluate(float time);
    }

    public readonly struct MotionKey
    {
        public readonly float Time;
        public readonly float Value;
        public readonly EaseKind Ease;

        public MotionKey(float time, float value, EaseKind ease = EaseKind.Linear)
        {
            Time = time;
            Value = value;
            Ease = ease;
        }
    }

    /// <summary>Piecewise keys; each segment eases with the curve named on its destination key. Holds the ends outside the range.</summary>
    public class KeyTrack : IMotionTrack
    {
        private readonly MotionKey[] _keys;

        public KeyTrack(params MotionKey[] keys)
        {
            _keys = keys;
        }

        public float Evaluate(float time)
        {
            if (time <= _keys[0].Time) return _keys[0].Value;
            for (var i = 1; i < _keys.Length; i++)
                if (time <= _keys[i].Time) return Between(_keys[i - 1], _keys[i], time);
            return _keys[_keys.Length - 1].Value;
        }

        private static float Between(MotionKey from, MotionKey to, float time)
        {
            var span = to.Time - from.Time;
            var progress = span > 0f ? (time - from.Time) / span : 1f;
            return from.Value + (to.Value - from.Value) * Ease.Apply(to.Ease, progress);
        }
    }

    /// <summary>base + amp·sin(2π(t / period + phase)).</summary>
    public class SineTrack : IMotionTrack
    {
        private readonly float _amplitude;
        private readonly float _period;
        private readonly float _phase;
        private readonly float _base;

        public SineTrack(float amplitude, float period, (float Phase, float Base) offset)
        {
            _amplitude = amplitude;
            _period = period;
            _phase = offset.Phase;
            _base = offset.Base;
        }

        public float Evaluate(float time) => _base + _amplitude * Mathf.Sin(2f * Mathf.PI * (time / _period + _phase));
    }

    /// <summary>Constant spin: degrees per second times t.</summary>
    public class SpinTrack : IMotionTrack
    {
        private readonly float _degreesPerSecond;

        public SpinTrack(float degreesPerSecond)
        {
            _degreesPerSecond = degreesPerSecond;
        }

        public float Evaluate(float time) => _degreesPerSecond * time;
    }
}
