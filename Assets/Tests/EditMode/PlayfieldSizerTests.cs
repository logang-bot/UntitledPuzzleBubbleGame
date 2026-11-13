using Game.Grid;
using NUnit.Framework;

namespace Game.Tests
{
    public class PlayfieldSizerTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void OrthographicSizeForWidth_MatchesBoardWidthToScreenWidth()
        {
            var size = PlayfieldSizer.OrthographicSizeForWidth(boardWidth: 8f, screenWidth: 1080f, screenHeight: 1920f);

            var visibleWorldWidth = size * 2f * (1080f / 1920f);
            Assert.That(visibleWorldWidth, Is.EqualTo(8f).Within(Tolerance));
        }

        [Test]
        public void RowsFitting_OneCellTall_FitsOneRow()
        {
            Assert.That(PlayfieldSizer.RowsFitting(bandHeight: 1f, cellWidth: 1f), Is.EqualTo(1));
        }

        [Test]
        public void RowsFitting_ShorterThanOneCell_FitsNoRows()
        {
            Assert.That(PlayfieldSizer.RowsFitting(bandHeight: 0.9f, cellWidth: 1f), Is.EqualTo(0));
        }

        [Test]
        public void RowsFitting_SoloGuideBoard_FitsFifteenRows()
        {
            var band = (611f - 40f) / PlayfieldSpec.CellPt;

            Assert.That(PlayfieldSizer.RowsFitting(band, cellWidth: 1f), Is.EqualTo(15));
        }
    }
}
