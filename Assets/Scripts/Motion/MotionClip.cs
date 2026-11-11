using UnityEngine;

namespace Game.Motion
{
    /// <summary>
    /// A pose offset from the rest pose: Offset in pt with y down, Rotation
    /// in degrees clockwise (the guide's convention), Scale and Alpha as
    /// multipliers.
    /// </summary>
    public struct MotionPose
    {
        public Vector2 Offset;
        public float Rotation;
        public Vector2 Scale;
        public float Alpha;

        public static MotionPose Rest => new MotionPose { Scale = Vector2.one, Alpha = 1f };
    }

    /// <summary>
    /// One motion-spec entry: optional tracks per channel (x, y and rot add,
    /// sx, sy and alpha multiply onto the rest pose), an optional start delay,
    /// and a duration that the clip loops over when Loop is set.
    /// </summary>
    public class MotionClip
    {
        public IMotionTrack X;
        public IMotionTrack Y;
        public IMotionTrack Rotation;
        public IMotionTrack ScaleX;
        public IMotionTrack ScaleY;
        public IMotionTrack Alpha;
        public float Duration;
        public float Delay;
        public bool Loop;

        public bool MovesPosition => X != null || Y != null;
        public bool MovesRotation => Rotation != null;
        public bool MovesScale => ScaleX != null || ScaleY != null;

        public bool IsFinished(float time) => !Loop && Duration > 0f && time >= Delay + Duration;

        public MotionPose Evaluate(float time)
        {
            var local = LocalTime(time);
            return new MotionPose
            {
                Offset = new Vector2(Sample(X, local, 0f), Sample(Y, local, 0f)),
                Rotation = Sample(Rotation, local, 0f),
                Scale = new Vector2(Sample(ScaleX, local, 1f), Sample(ScaleY, local, 1f)),
                Alpha = Sample(Alpha, local, 1f)
            };
        }

        private float LocalTime(float time)
        {
            var local = Mathf.Max(0f, time - Delay);
            return Loop && Duration > 0f ? local % Duration : local;
        }

        private static float Sample(IMotionTrack track, float time, float rest) => track?.Evaluate(time) ?? rest;
    }
}
