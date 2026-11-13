using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    /// Places a PlayfieldSpec in world space for one camera view: the board's
    /// top edge, the danger line (the deck's top) and the launcher pointer.
    /// The deck is anchored to the bottom of the view and lifted by
    /// raisePt when a tall gesture bar would cover it. Pure math, no
    /// Camera/Screen access.
    /// </summary>
    public readonly struct PlayfieldFrame
    {
        public const float GestureBarAllowancePt = 22f;

        public float WorldPerPt { get; }
        public float FirstRowTop => _spec.FirstRowTopPt * WorldPerPt;
        public float RailWidth => PlayfieldSpec.RailPt * WorldPerPt;

        private readonly PlayfieldSpec _spec;
        private readonly float _raisePt;

        public PlayfieldFrame(PlayfieldSpec spec, float worldPerPt, float raisePt)
        {
            _spec = spec;
            WorldPerPt = worldPerPt;
            _raisePt = spec.RaisedAboveGestureBar ? raisePt : 0f;
        }

        public static float WorldPerPtFor(float cellWidth) => cellWidth / PlayfieldSpec.CellPt;

        public static float RaisePt(float safeBottomPx, float screenWidthPx)
        {
            var safeBottomPt = safeBottomPx * PlayfieldSpec.GridWidthPt / screenWidthPx;
            return Mathf.Max(0f, safeBottomPt - GestureBarAllowancePt);
        }

        public float BoardTopY(Vector2 cameraPosition, float orthographicSize)
        {
            return cameraPosition.y + orthographicSize - FirstRowTop;
        }

        public float DangerY(Vector2 cameraPosition, float orthographicSize)
        {
            return ViewBottom(cameraPosition, orthographicSize) + (_spec.DangerAboveBottomPt + _raisePt) * WorldPerPt;
        }

        public Vector2 Pointer(Vector2 cameraPosition, float orthographicSize)
        {
            var x = cameraPosition.x + (_spec.PointerPt.x - PlayfieldSpec.GridWidthPt * 0.5f) * WorldPerPt;
            var y = ViewBottom(cameraPosition, orthographicSize) + (_spec.PointerAboveBottomPt + _raisePt) * WorldPerPt;
            return new Vector2(x, y);
        }

        private static float ViewBottom(Vector2 cameraPosition, float orthographicSize) => cameraPosition.y - orthographicSize;
    }
}
