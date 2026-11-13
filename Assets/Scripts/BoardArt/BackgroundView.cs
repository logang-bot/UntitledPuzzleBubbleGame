using Game.Art;
using Game.Grid;
using UnityEngine;

namespace Game.BoardArt
{
    /// <summary>
    /// The theme backdrop behind the whole view: 424 × 844 pt with a 32 pt
    /// bleed. Solo hangs it from the top of the view, a Battle half stands
    /// it on the bottom (the guide draws the half over the backdrop's lower
    /// part). Views taller than the art scale it up to cover.
    /// </summary>
    public class BackgroundView
    {
        public const float BleedPt = 32f;
        private const float ArtWidthPt = 424f;
        private const float ArtHeightPt = 844f;

        private readonly GameBoard _board;
        private readonly MenuArtLibrary _art;
        private readonly ViewAnchor _anchor;
        private Transform _holder;

        private float HalfHeightPt => _anchor.OrthographicSize / _board.Frame.WorldPerPt;

        public BackgroundView(GameBoard board, MenuArtLibrary art, ViewAnchor anchor)
        {
            _board = board;
            _art = art;
            _anchor = anchor;
        }

        public void Refresh()
        {
            if (_holder != null) Object.Destroy(_holder.gameObject);
            _holder = new GameObject("Background").transform;
            _holder.SetParent(_anchor.transform, worldPositionStays: false);
            var scale = CoverScale();
            _holder.localScale = Vector3.one * (_board.Frame.WorldPerPt * scale);
            _holder.localPosition = TopLeft(scale);
            SpriteChild.Create(_holder, "Backdrop", _art.Frame("bg_" + ThemeForLevel.Key(_board.Theme), 0), BoardSorting.Background);
            BackgroundAmbience.Populate(_holder, _board.Theme, _art);
        }

        private float CoverScale() => Mathf.Max(1f, 2f * HalfHeightPt / (ArtHeightPt - BleedPt));

        private Vector3 TopLeft(float scale)
        {
            var bleed = BleedPt * scale;
            var topPt = _board.Layout == PlayfieldLayout.Solo ? HalfHeightPt + bleed : -HalfHeightPt - bleed + ArtHeightPt * scale;
            return new Vector3(-ArtWidthPt * 0.5f * scale, topPt, 0f) * _board.Frame.WorldPerPt;
        }
    }
}
