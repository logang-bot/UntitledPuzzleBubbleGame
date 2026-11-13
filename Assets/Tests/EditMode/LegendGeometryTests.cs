using Game.EditorTools.Art;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class LegendGeometryTests
    {
        [Test]
        public void FrameRect_FractionalExport_RoundsToWholePixels()
        {
            var sprite = new SheetLegendSprite { x = 1489.99f, y = 4f, frameW = 129.99f, frameH = 228f, frames = 1 };
            var rect = LegendGeometry.FrameRect(sprite, 0, 733.99f);
            Assert.AreEqual(new Rect(1490f, 502f, 130f, 228f), rect);
        }

        [Test]
        public void FrameRect_LaterFrame_StepsAlongX()
        {
            var sprite = new SheetLegendSprite { x = 10f, y = 0f, frameW = 40f, frameH = 32f, frames = 4, stepX = 42f };
            Assert.AreEqual(94f, LegendGeometry.FrameRect(sprite, 2, 100f).x);
        }

        [Test]
        public void NormalizedPivot_NamedBottomCenterWithoutPixels_UsesTheNamedPoint()
        {
            var sprite = new SheetLegendSprite { frameW = 40f, frameH = 44f, pivot = "bottom-center" };
            Assert.AreEqual(new Vector2(0.5f, 0f), LegendGeometry.NormalizedPivot(sprite));
        }

        [Test]
        public void NormalizedPivot_ExplicitPixels_WinOverTheName()
        {
            var sprite = new SheetLegendSprite { frameW = 84f, frameH = 48f, pivot = "custom", pivotX = 42f, pivotY = 12f };
            Assert.AreEqual(new Vector2(0.5f, 0.75f), LegendGeometry.NormalizedPivot(sprite));
        }

        [Test]
        public void MaxTextureSize_SheetTallerThan4096_GrowsToTheNextPowerOfTwo()
        {
            Assert.AreEqual(8192, LegendGeometry.MaxTextureSize(new SheetLegend { width = 3832f, height = 5076f }));
        }
    }
}
