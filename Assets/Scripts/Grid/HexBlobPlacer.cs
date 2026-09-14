using System.Collections.Generic;
using System.Linq;
using Game.Superpowers;
using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    /// Fills a row band with separated, single-color hexagonal blobs. The
    /// configured radius is clamped to what the band can actually hold (a
    /// blob needs 2*radius+1 rows in the worst case, centered mid-band) so a
    /// short band still places something instead of silently nothing. See
    /// docs/features/level-content/specs/2026-09-11-curated-levels-and-transitions-design.md.
    /// </summary>
    public static class HexBlobPlacer
    {
        public static void Place(PatternPlacementContext context, (int Start, int EndExclusive) rowBand)
        {
            var layout = new BlobLayout
            {
                RowBand = rowBand,
                Radius = EffectiveRadius(context.Settings.HexBlobRadius, rowBand),
            };
            var reserved = new HashSet<(int Row, int Col)>();
            foreach (var center in ShuffledCenters(context, rowBand))
                if (IsValidCenter(context, layout, reserved, center))
                    PlaceBlob(context, layout, reserved, center);
        }

        private static int EffectiveRadius(int configuredRadius, (int Start, int EndExclusive) rowBand)
        {
            var maxRadius = (rowBand.EndExclusive - rowBand.Start - 1) / 2;
            return Mathf.Clamp(configuredRadius, 0, Mathf.Max(0, maxRadius));
        }

        private static List<(int Row, int Col)> ShuffledCenters(PatternPlacementContext context, (int Start, int EndExclusive) rowBand)
        {
            var cells = new List<(int Row, int Col)>();
            for (var row = rowBand.Start; row < rowBand.EndExclusive; row++)
                for (var col = 0; col < context.Grid.Cols; col++)
                    cells.Add((row, col));
            return Shuffle(cells, context.Rng);
        }

        private static List<T> Shuffle<T>(List<T> items, System.Random rng)
        {
            for (var i = items.Count - 1; i > 0; i--)
            {
                var j = rng.Next(i + 1);
                (items[i], items[j]) = (items[j], items[i]);
            }
            return items;
        }

        private static bool IsValidCenter(PatternPlacementContext context, BlobLayout layout, HashSet<(int Row, int Col)> reserved, (int Row, int Col) center)
        {
            if (reserved.Contains(center)) return false;
            var blobCells = HexRadius.CellsWithinRadius(context.Grid, center, layout.Radius);
            return blobCells.All(cell => cell.Row >= layout.RowBand.Start && cell.Row < layout.RowBand.EndExclusive)
                && blobCells.All(cell => !context.Grid.IsOccupied(cell.Row, cell.Col))
                && blobCells.All(cell => !reserved.Contains(cell));
        }

        private static void PlaceBlob(PatternPlacementContext context, BlobLayout layout, HashSet<(int Row, int Col)> reserved, (int Row, int Col) center)
        {
            var color = BubbleColorPalette.AllColors[context.Rng.Next(context.ColorCount)];
            var blobCells = HexRadius.CellsWithinRadius(context.Grid, center, layout.Radius);
            foreach (var cell in blobCells)
                context.Grid.PlaceBubble(cell.Row, cell.Col, color);

            var reservedZone = HexRadius.CellsWithinRadius(context.Grid, center, layout.Radius + context.Settings.HexBlobGapCells);
            reserved.UnionWith(reservedZone);
        }

        private sealed class BlobLayout
        {
            public (int Start, int EndExclusive) RowBand { get; set; }
            public int Radius { get; set; }
        }
    }
}
