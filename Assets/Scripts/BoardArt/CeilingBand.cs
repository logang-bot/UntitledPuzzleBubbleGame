using Game.Art;
using Game.Grid;
using UnityEngine;

namespace Game.BoardArt
{
    /// <summary>
    /// The advancing ceiling: the theme's tiled band, reaching from above the
    /// screen down to 6 pt over the current first row, with its edge piece on
    /// that line. The band grows a row per push (GridModel.RowsPushed), so
    /// nothing can land in the space the wall has taken.
    /// </summary>
    public class CeilingBand
    {
        private const float WidthPt = 376f;
        private const float BandHeightPt = 2400f;
        private const float EdgeHeightPt = 14f;
        private const float BandOverlapPt = 6f;
        private const float WarnGlowHeightPt = 26f;
        private const float WarnFlashSeconds = 0.233f;

        private readonly GameBoard _board;
        private readonly MenuArtLibrary _art;
        private readonly SpriteRenderer _band;
        private readonly SpriteRenderer _edge;
        private readonly SpriteRenderer _warnGlow;

        public CeilingBand(GameBoard board, MenuArtLibrary art, Transform parent)
        {
            _board = board;
            _art = art;
            var holder = new GameObject("Ceiling").transform;
            holder.SetParent(parent, worldPositionStays: false);
            holder.localScale = Vector3.one * board.Frame.WorldPerPt;
            _band = SpriteChild.Tiled(holder, "Band", null, BoardSorting.Ceiling);
            _edge = SpriteChild.Tiled(holder, "Edge", null, BoardSorting.Ceiling + 1);
            _warnGlow = SpriteChild.Tiled(holder, "WarnGlow", art.Frame("ceiling_warn_glow", 0), BoardSorting.Ceiling + 2);
            _warnGlow.enabled = false;
        }

        public void Refresh()
        {
            var theme = ThemeForLevel.Key(_board.Theme);
            _band.sprite = _art.Frame($"ceiling_{theme}_tile", 0);
            _edge.sprite = _art.Frame($"ceiling_{theme}_edge", 0);
            var linePt = FirstRowTopLocal() / _board.Frame.WorldPerPt;
            var leftPt = HexGridMath.BoardOriginXOffset(_board.Cols, _board.CellWidth) / _board.Frame.WorldPerPt - WidthPt * 0.5f;
            Place(_band, new Vector2(leftPt, linePt + BandOverlapPt + BandHeightPt), BandHeightPt);
            Place(_edge, new Vector2(leftPt, linePt + BandOverlapPt), EdgeHeightPt);
            Place(_warnGlow, new Vector2(leftPt, linePt - BandOverlapPt), WarnGlowHeightPt);
        }

        /// <summary>Flashes the glow under the ceiling edge (ceiling_warn): alpha 0.5 + 0.5·sin(2πt / 0.233 − π/2), so it starts dark.</summary>
        public void ShowWarning(bool warning, float time)
        {
            _warnGlow.enabled = warning;
            if (warning) _warnGlow.color = new Color(1f, 1f, 1f, 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * time / WarnFlashSeconds - Mathf.PI * 0.5f));
        }

        private float FirstRowTopLocal()
        {
            return _board.CellWidth * 0.5f - _board.Grid.RowsPushed * HexGridMath.RowHeight(_board.CellWidth);
        }

        private static void Place(SpriteRenderer spriteRenderer, Vector2 topLeftPt, float heightPt)
        {
            spriteRenderer.transform.localPosition = topLeftPt;
            spriteRenderer.size = new Vector2(WidthPt, heightPt);
        }
    }
}
