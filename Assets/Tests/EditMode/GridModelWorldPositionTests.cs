using Game.Grid;
using NUnit.Framework;

namespace Game.Tests
{
    public class GridModelWorldPositionTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void GetWorldPosition_EvenRow_IsUnshiftedHorizontally()
        {
            var grid = new GridModel(rows: 3, cols: 3, cellWidth: 1f);

            var position = grid.GetWorldPosition(row: 0, col: 1);

            Assert.That(position.x, Is.EqualTo(1f).Within(Tolerance));
        }

        [Test]
        public void GetWorldPosition_OddRow_IsShiftedByHalfCell()
        {
            var grid = new GridModel(rows: 3, cols: 3, cellWidth: 1f);

            var position = grid.GetWorldPosition(row: 1, col: 0);

            Assert.That(position.x, Is.EqualTo(0.5f).Within(Tolerance));
        }

        [Test]
        public void GetWorldPosition_RowHeightIsHexPacked()
        {
            var grid = new GridModel(rows: 3, cols: 3, cellWidth: 1f);

            var position = grid.GetWorldPosition(row: 1, col: 0);

            Assert.That(position.y, Is.EqualTo(0.8660254f).Within(Tolerance));
        }
    }
}
