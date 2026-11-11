using Game.UI;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class UiPlacementTests
    {
        [Test]
        public void NativePosition_RoundsToWholeArtPixelsAndFlipsY()
        {
            Assert.AreEqual(new Vector2(12f, -8f), UiPlacement.NativePosition(new Vector2(11.6f, 7.7f)));
        }

        [Test]
        public void SmoothPosition_KeepsFractionAndFlipsY()
        {
            Assert.AreEqual(new Vector2(11.25f, -7.75f), UiPlacement.SmoothPosition(new Vector2(11.25f, 7.75f)));
        }
    }
}
