using Game.Menu;
using NUnit.Framework;

namespace Game.Tests
{
    public class FloaterScheduleTests
    {
        [Test]
        public void Flight_TakesPathLengthOverItsSizeSpeed()
        {
            var flight = FloaterFlight.Roll(new System.Random(3), 0);
            Assert.AreEqual(flight.Path.Length / FloaterSpec.Speed(flight.Pick.Size), flight.Duration, 1e-3f);
        }

        [Test]
        public void SameSlot_AtTheSameTime_GivesTheSameFlight()
        {
            new FloaterSchedule(21, 100f).TryGetFlight(130f, out var a, out var progressA);
            new FloaterSchedule(21, 100f).TryGetFlight(130f, out var b, out var progressB);
            Assert.AreEqual(a.Number, b.Number);
            Assert.AreEqual(progressA, progressB, 1e-5f);
        }

        [Test]
        public void Schedule_AdvancesToLaterFlights()
        {
            var schedule = new FloaterSchedule(22, 0f);
            schedule.TryGetFlight(1000f, out var flight, out _);
            Assert.Greater(flight.Number, 0);
        }

        [Test]
        public void Progress_WhileFlying_StaysBetweenZeroAndOne()
        {
            var schedule = new FloaterSchedule(23, 0f);
            for (var t = 0f; t < 400f; t += 0.5f)
                if (schedule.TryGetFlight(t, out _, out var progress)) Assert.That(progress, Is.InRange(0f, 1f));
        }

        [Test]
        public void SmallFloaters_DriftSlowerThanLargeOnes()
        {
            Assert.Less(FloaterSpec.Speed(FloaterSize.S), FloaterSpec.Speed(FloaterSize.L));
        }
    }
}
