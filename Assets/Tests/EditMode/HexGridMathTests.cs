using Game.Grid;
using NUnit.Framework;

namespace Game.Tests
{
    public class HexGridMathTests
    {
        private const float Tolerance = 0.0001f;
        private const int Cols = 8;
        private const float CellWidth = 1f;

        [Test]
        public void BoardWidthWithOffsetMargin_AddsHalfCellForOddRowOverhang()
        {
            var width = HexGridMath.BoardWidthWithOffsetMargin(Cols, CellWidth);

            Assert.That(width, Is.EqualTo(8.5f).Within(Tolerance));
        }

        [Test]
        public void BoardOriginXOffset_CentersCombinedEvenOddRowFootprintOnCamera()
        {
            var offset = HexGridMath.BoardOriginXOffset(Cols, CellWidth);
            var boardOriginX = -offset; // camera sits at world x = 0 in this test

            // Even row 0's leftmost bubble (col 0, unshifted) vs. odd row 1's
            // rightmost bubble (col Cols-1, shifted right by half a cell) — their
            // outer edges (center +/- the 0.5*CellWidth bubble radius) should be
            // equidistant from the camera once the board is centered correctly.
            var evenRowLeftEdge = boardOriginX + 0f - CellWidth * 0.5f;
            var oddRowRightEdge = boardOriginX + (Cols - 1) * CellWidth + CellWidth * 0.5f + CellWidth * 0.5f;

            Assert.That(-evenRowLeftEdge, Is.EqualTo(oddRowRightEdge).Within(Tolerance));
        }
    }
}
