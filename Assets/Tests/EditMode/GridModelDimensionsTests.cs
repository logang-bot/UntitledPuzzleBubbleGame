using Game.Grid;
using NUnit.Framework;

namespace Game.Tests
{
    public class GridModelDimensionsTests
    {
        [Test]
        public void RowsAndCols_ExposeConstructorValues()
        {
            var grid = new GridModel(rows: 7, cols: 4);

            Assert.That(grid.Rows, Is.EqualTo(7));
            Assert.That(grid.Cols, Is.EqualTo(4));
        }
    }
}
