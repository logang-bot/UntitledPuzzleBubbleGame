using Game.Art;
using Game.Grid;
using UnityEngine;

namespace Game.BoardArt
{
    /// <summary>
    /// The fixed edges of the playfield: the side rails (4 pt, the full view
    /// height) and the danger line on the deck's top edge, whose glow pulses
    /// (danger_pulse) while a bubble is within one row of it.
    /// </summary>
    public class PlayfieldEdges : MonoBehaviour
    {
        private const float PulsePeriodSeconds = 0.8f;
        private const float HalfWidthPt = PlayfieldSpec.GridWidthPt * 0.5f;

        private GameBoard _board;
        private SpriteRenderer _glow;

        public static void Create(GameBoard board, MenuArtLibrary art, ViewAnchor anchor)
        {
            var edges = new GameObject("PlayfieldEdges").AddComponent<PlayfieldEdges>();
            edges.transform.SetParent(anchor.transform, worldPositionStays: false);
            edges.transform.localScale = Vector3.one * board.Frame.WorldPerPt;
            edges._board = board;
            edges.Build(art, anchor.OrthographicSize / board.Frame.WorldPerPt);
        }

        private void Build(MenuArtLibrary art, float halfHeightPt)
        {
            AddRail(art, -1f, halfHeightPt);
            AddRail(art, 1f, halfHeightPt);
            var dangerPt = (_board.Frame.DangerY(Vector2.zero, halfHeightPt * _board.Frame.WorldPerPt)) / _board.Frame.WorldPerPt;
            Place(SpriteChild.Tiled(transform, "DangerLine", art.Frame("danger_line", 0), BoardSorting.Edges), new Vector2(-HalfWidthPt, dangerPt + 4f), new Vector2(PlayfieldSpec.GridWidthPt, 8f));
            _glow = SpriteChild.Tiled(transform, "DangerGlow", art.Frame("danger_glow", 0), BoardSorting.DangerGlow);
            Place(_glow, new Vector2(-HalfWidthPt, dangerPt + 28f), new Vector2(PlayfieldSpec.GridWidthPt, 28f));
        }

        private void AddRail(MenuArtLibrary art, float side, float halfHeightPt)
        {
            var rail = SpriteChild.Tiled(transform, "Rail", art.Frame("rail", 0), BoardSorting.Edges);
            rail.flipX = side > 0f;
            Place(rail, new Vector2(side * HalfWidthPt, halfHeightPt), new Vector2(PlayfieldSpec.RailPt, halfHeightPt * 2f));
        }

        private static void Place(SpriteRenderer spriteRenderer, Vector2 topLeftPt, Vector2 sizePt)
        {
            spriteRenderer.transform.localPosition = topLeftPt;
            spriteRenderer.size = sizePt;
        }

        private void Update()
        {
            _glow.enabled = BubbleNearDanger();
            if (_glow.enabled) _glow.color = new Color(1f, 1f, 1f, 0.55f + 0.35f * Mathf.Sin(2f * Mathf.PI * Time.time / PulsePeriodSeconds));
        }

        private bool BubbleNearDanger()
        {
            var rowHeight = HexGridMath.RowHeight(_board.CellWidth);
            var dangerY = transform.position.y + _board.Frame.DangerY(Vector2.zero, _board.ViewCamera.orthographicSize);
            foreach (var cell in _board.Grid.OccupiedCells())
                if (LowestEdge(cell) - dangerY < rowHeight) return true;
            return false;
        }

        private float LowestEdge((int Row, int Col) cell)
        {
            return _board.transform.position.y + _board.Grid.GetWorldPosition(cell.Row, cell.Col).y - _board.CellWidth * 0.5f;
        }
    }
}
