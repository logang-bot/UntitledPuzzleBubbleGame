using UnityEngine;

namespace Game.Menu
{
    /// <summary>
    /// The timeline of one main-menu floater slot: crossings at constant
    /// per-size speed, each followed by a 2–8 s rest, every flight seeded from
    /// (slot, flight number) so it is reproducible. It starts pre-warmed at a
    /// random point of its first flight-plus-rest, so the sky is already busy
    /// on the first frame.
    /// </summary>
    public class FloaterSchedule
    {
        private readonly int _slotId;
        private FloaterFlight _flight;
        private float _start;
        private float _rest;

        public FloaterSchedule(int slotId, float now)
        {
            _slotId = slotId;
            Launch(0, now);
            var prewarm = (float)RandomFor(-1).NextDouble();
            _start = now - prewarm * (_flight.Duration + _rest);
        }

        public bool TryGetFlight(float time, out FloaterFlight flight, out float progress)
        {
            while (time >= _start + _flight.Duration + _rest)
                Launch(_flight.Number + 1, _start + _flight.Duration + _rest);
            flight = _flight;
            progress = (time - _start) / _flight.Duration;
            return progress <= 1f;
        }

        private void Launch(int number, float start)
        {
            var random = RandomFor(number);
            _flight = FloaterFlight.Roll(random, number);
            _rest = Mathf.Lerp(FloaterSpec.GapSeconds.x, FloaterSpec.GapSeconds.y, (float)random.NextDouble());
            _start = start;
        }

        private System.Random RandomFor(int number) => new System.Random(unchecked(_slotId * 7919 + number * 104729 + 13));
    }
}
