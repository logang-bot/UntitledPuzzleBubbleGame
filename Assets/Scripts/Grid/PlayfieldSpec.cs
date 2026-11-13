using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    /// The cartoon game guide's playfield numbers (Assets/Art/Game/Spec/layout.json),
    /// in pt on its grid: the board's first row top, the danger line where the
    /// deck starts, and the launcher pointer. Distances below the danger line
    /// are anchored to the bottom of the layout, the first row to its top.
    /// </summary>
    public struct PlayfieldSpec
    {
        public const float CellPt = 352f / 8.5f;
        public const float RailPt = 4f;
        public const float GridWidthPt = 360f;

        public static readonly PlayfieldSpec Solo = new() { HeightPt = 780f, FirstRowTopPt = 40f, DangerYPt = 611f, PointerPt = new Vector2(180f, 716.67f), RaisedAboveGestureBar = true };
        public static readonly PlayfieldSpec BattleHalf = new() { HeightPt = 390f, FirstRowTopPt = 13f, DangerYPt = 277f, PointerPt = new Vector2(180f, 326.67f) };

        public float HeightPt { get; private set; }
        public float FirstRowTopPt { get; private set; }
        public float DangerYPt { get; private set; }
        public Vector2 PointerPt { get; private set; }
        public bool RaisedAboveGestureBar { get; private set; }
        public float DangerAboveBottomPt => HeightPt - DangerYPt;
        public float PointerAboveBottomPt => HeightPt - PointerPt.y;

        public static PlayfieldSpec For(PlayfieldLayout layout) => layout == PlayfieldLayout.Solo ? Solo : BattleHalf;
    }
}
