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
        public void RowsForWorldHeight_ExactFit_ReturnsWholeRowCount()
        {
            var rows = PlayfieldSizer.RowsForWorldHeight(worldHeight: 10f, cellWidth: 1f);

            Assert.That(rows, Is.EqualTo(11));
        }

        [Test]
        public void RowsForWorldHeight_PartialRow_FloorsDownToLastFullRow()
        {
            var rows = PlayfieldSizer.RowsForWorldHeight(worldHeight: 4.5f, cellWidth: 1f);

            Assert.That(rows, Is.EqualTo(5));
        }
    }
}
