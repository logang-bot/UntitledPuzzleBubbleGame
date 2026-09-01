using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Grid
{
    public class GridModel
    {
        private static readonly (int Row, int Col)[] EvenRowOffsets =
        {
            (0, -1), (0, 1),
            (-1, -1), (-1, 0),
            (1, -1), (1, 0),
        };

        private static readonly (int Row, int Col)[] OddRowOffsets =
        {
            (0, -1), (0, 1),
            (-1, 0), (-1, 1),
            (1, 0), (1, 1),
        };

        private readonly int _rows;
        private readonly int _cols;
        private readonly float _cellWidth;
        private readonly bool[,] _occupied;
        private readonly BubbleColor[,] _colors;

        public int Rows => _rows;
        public int Cols => _cols;

        public GridModel(int rows, int cols, float cellWidth = 1f)
        {
            _rows = rows;
            _cols = cols;
            _cellWidth = cellWidth;
            _occupied = new bool[rows, cols];
            _colors = new BubbleColor[rows, cols];
        }

        public bool IsOccupied(int row, int col)
        {
            return _occupied[row, col];
        }

        public BubbleColor GetColor(int row, int col)
        {
            return _colors[row, col];
        }

        public void PlaceBubble(int row, int col, BubbleColor color)
        {
            _occupied[row, col] = true;
            _colors[row, col] = color;
        }

        public void ClearCell(int row, int col)
        {
            _occupied[row, col] = false;
        }

        public IEnumerable<(int Row, int Col)> OccupiedCells()
        {
            for (var row = 0; row < _rows; row++)
                for (var col = 0; col < _cols; col++)
                    if (_occupied[row, col])
                        yield return (row, col);
        }

        /// <summary>
        /// The (up to 6) hex-adjacent cells for the given cell, clipped to grid bounds.
        /// Even rows are unshifted; odd rows are shifted right by half a cell (see docs/features/core-gameplay/hex-grid.md).
        /// </summary>
        public List<(int Row, int Col)> GetNeighbors(int row, int col)
        {
            return CandidateNeighbors(row, col)
                .Where(cell => IsInBounds(cell.Row, cell.Col))
                .ToList();
        }

        private static IEnumerable<(int Row, int Col)> CandidateNeighbors(int row, int col)
        {
            var offsets = row % 2 == 0 ? EvenRowOffsets : OddRowOffsets;
            return offsets.Select(offset => (row + offset.Row, col + offset.Col));
        }

        private bool IsInBounds(int row, int col)
        {
            return row >= 0 && row < _rows && col >= 0 && col < _cols;
        }

        /// <summary>
        /// World-space position of a cell's center, in hex-packed rows (see docs/features/core-gameplay/hex-grid.md).
        /// Row 0 is the ceiling (y = 0, the board's anchor); increasing row moves down
        /// toward the shooter, so y decreases with row.
        /// </summary>
        public Vector2 GetWorldPosition(int row, int col)
        {
            var xOffset = row % 2 == 0 ? 0f : _cellWidth * 0.5f;
            var x = col * _cellWidth + xOffset;
            var y = -row * HexGridMath.RowHeight(_cellWidth);
            return new Vector2(x, y);
        }
    }
}
