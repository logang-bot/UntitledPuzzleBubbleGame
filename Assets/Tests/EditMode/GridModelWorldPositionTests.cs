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

            Assert.That(position.y, Is.EqualTo(-0.8660254f).Within(Tolerance));
        }

        [Test]
        public void GetWorldPosition_RowZero_IsTheCeilingAnchorAtYZero()
        {
            var grid = new GridModel(rows: 3, cols: 3, cellWidth: 1f);

            var position = grid.GetWorldPosition(row: 0, col: 0);

            Assert.That(position.y, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void GetWorldPosition_HigherRow_IsFartherFromTheCeiling()
        {
            var grid = new GridModel(rows: 5, cols: 3, cellWidth: 1f);

            var nearCeiling = grid.GetWorldPosition(row: 1, col: 0);
            var fartherFromCeiling = grid.GetWorldPosition(row: 3, col: 0);

            Assert.That(fartherFromCeiling.y, Is.LessThan(nearCeiling.y));
        }

        [Test]
        public void GetWorldPosition_SameBubbleAfterOnePush_XIsUnchangedYDropsByOneRowHeight()
        {
            var grid = new GridModel(rows: 6, cols: 3, cellWidth: 1f);
            var before = grid.GetWorldPosition(row: 2, col: 1);

            grid.PushRowsDown(out _);
            var after = grid.GetWorldPosition(row: 3, col: 1); // same bubble, now one row lower

            Assert.That(after.x, Is.EqualTo(before.x).Within(Tolerance));
            Assert.That(after.y, Is.EqualTo(before.y - HexGridMath.RowHeight(1f)).Within(Tolerance));
        }

        [Test]
        public void GetWorldPosition_SameBubbleAfterThreePushes_XIsStillUnchanged()
        {
            var grid = new GridModel(rows: 8, cols: 3, cellWidth: 1f);
            var before = grid.GetWorldPosition(row: 2, col: 1);

            for (var i = 0; i < 3; i++) grid.PushRowsDown(out _);
            var after = grid.GetWorldPosition(row: 5, col: 1); // same bubble, three rows lower

            Assert.That(after.x, Is.EqualTo(before.x).Within(Tolerance));
        }
    }
}
