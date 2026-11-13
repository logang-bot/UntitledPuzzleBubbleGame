using System.Collections.Generic;
using Game.BoardArt;
using Game.Grid;
using NUnit.Framework;

namespace Game.Tests
{
    public class BoardArtScheduleTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void BuildIn_LowerRowsAndLaterColumns_StartLater()
        {
            Assert.That(BuildInSchedule.DelayFor((2, 3)), Is.EqualTo(2 * 0.07f + 3 * 0.012f).Within(Tolerance));
        }

        [Test]
        public void BuildIn_Total_EndsWhenTheLastBubbleHasGrown()
        {
            var total = BuildInSchedule.TotalSeconds(new[] { (0, 0), (4, 1) });

            Assert.That(total, Is.EqualTo(4 * 0.07f + 0.012f + 0.36f).Within(Tolerance));
        }

        [Test]
        public void Pop_ChainOfThree_BurstsOneStepApart()
        {
            var cells = new List<(int Row, int Col)> { (0, 0), (0, 1), (0, 2) };
            var delays = PopSchedule.Delays(cells, RowNeighbors);

            Assert.That(delays[(0, 0)], Is.EqualTo(0.12f).Within(Tolerance));
            Assert.That(delays[(0, 2)], Is.EqualTo(0.12f + 2 * 0.035f).Within(Tolerance));
        }

        [Test]
        public void Pop_CellOutsideTheWalk_BurstsWithTheFirst()
        {
            var cells = new List<(int Row, int Col)> { (0, 0), (5, 5) };

            Assert.That(PopSchedule.Delays(cells, RowNeighbors)[(5, 5)], Is.EqualTo(0.12f).Within(Tolerance));
        }

        [Test]
        public void Theme_ChangesEveryFiveLevelsAndRepeats()
        {
            Assert.AreEqual(BoardTheme.Living, ThemeForLevel.For(5));
            Assert.AreEqual(BoardTheme.Garden, ThemeForLevel.For(6));
            Assert.AreEqual(BoardTheme.Park, ThemeForLevel.For(20));
            Assert.AreEqual(BoardTheme.Living, ThemeForLevel.For(21));
        }

        private static IEnumerable<(int Row, int Col)> RowNeighbors((int Row, int Col) cell)
        {
            yield return (cell.Row, cell.Col - 1);
            yield return (cell.Row, cell.Col + 1);
        }
    }
}
