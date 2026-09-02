using System.Collections.Generic;
using Game.Grid;
using NUnit.Framework;

namespace Game.Tests
{
    public class BubbleColorPaletteTests
    {
        [Test]
        public void Random_WithColorCount_NeverReturnsColorOutsideRange()
        {
            var allowed = new[] { BubbleColorPalette.AllColors[0], BubbleColorPalette.AllColors[1] };

            for (var i = 0; i < 50; i++)
                Assert.That(allowed, Does.Contain(BubbleColorPalette.Random(colorCount: 2)));
        }
    }
}
