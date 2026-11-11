using Game.UI;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class PixelArtScaleTests
    {
        [Test]
        public void UnitsPerArtPixel_OnTheReferencePhone_KeepsTheDesignScale()
        {
            Assert.AreEqual(5f / 3f, PixelArtScale.UnitsPerArtPixel(5, 3f), 1e-4f);
        }

        [Test]
        public void UnitsPerArtPixel_OnAWiderPhone_RoundsToWholeDevicePixels()
        {
            var units = PixelArtScale.UnitsPerArtPixel(5, 4f);
            Assert.AreEqual(7f, units * 4f, 1e-4f);
        }

        [Test]
        public void UnitsPerArtPixel_OnATinyScreen_NeverDropsBelowOneDevicePixel()
        {
            Assert.AreEqual(1f, PixelArtScale.UnitsPerArtPixel(1, 0.5f) * 0.5f, 1e-4f);
        }

        [Test]
        public void SnapToScreenPixel_RoundsXAndYOnly()
        {
            Assert.AreEqual(new Vector3(10f, 21f, 0.3f), PixelArtScale.SnapToScreenPixel(new Vector3(10.4f, 20.6f, 0.3f)));
        }
    }
}
