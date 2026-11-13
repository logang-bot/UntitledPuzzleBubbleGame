using Game.Art;
using Game.Grid;
using Game.Shooter;
using Game.Superpowers;
using UnityEngine;

namespace Game.BoardArt
{
    /// <summary>
    /// The landed superpowers' own bursts (bomb_explode, row_sweep): the
    /// bomb plays its baked explosion and its cells pop outward from the
    /// center, 0.02 s per cell; the row clear sends its sweep head across
    /// the row at 900 pt/s and each bubble pops as the head reaches it.
    /// Subscribes in Awake so its timing is planned before the pops arrive.
    /// </summary>
    public class SuperpowerBursts : MonoBehaviour
    {
        private const float BombStepSeconds = 0.02f;
        private const float ExplosionScale = 1.25f;
        private const float SweepPtPerSecond = 900f;

        private GameBoard _board;
        private MenuArtLibrary _art;
        private BoardView _view;
        private FiredBubbleController _shots;

        public static void Attach(BoardView view, (GameBoard Board, MenuArtLibrary Art, FiredBubbleController Shots) sources)
        {
            var bursts = view.gameObject.AddComponent<SuperpowerBursts>();
            (bursts._board, bursts._art, bursts._shots, bursts._view) = (sources.Board, sources.Art, sources.Shots, view);
            sources.Shots.OnSuperpowerLanded += bursts.HandleLanded;
        }

        private void OnDestroy() => _shots.OnSuperpowerLanded -= HandleLanded;

        private void HandleLanded(SuperpowerId id, (int Row, int Col) cell)
        {
            var center = CellWorld(cell);
            if (id == SuperpowerId.Bomb) Explode(center);
            if (id == SuperpowerId.RowClear) Sweep(center);
        }

        private void Explode(Vector2 center)
        {
            var blast = SpriteChild.Create(null, "Explosion", _art.Frame("explosion", 0), BoardSorting.Effects);
            blast.transform.position = center;
            blast.transform.localScale = Vector3.one * (_board.Frame.WorldPerPt * ExplosionScale);
            WorldFrameAnimator.PlayOnce(blast, _art.Find("explosion"));
            _view.PlanBurstDelays(target => BombStepSeconds * Vector2.Distance(CellWorld(target), center) / _board.CellWidth);
        }

        private void Sweep(Vector2 rowCenter)
        {
            var startX = _board.Bounds.LeftWallX;
            var head = SpriteChild.Create(null, "RowSweep", _art.Frame("row_sweep", 0), BoardSorting.Effects);
            head.transform.position = new Vector3(startX, rowCenter.y, 0f);
            head.transform.localScale = Vector3.one * _board.Frame.WorldPerPt;
            var speed = SweepPtPerSecond * _board.Frame.WorldPerPt;
            SweepMover.Launch(head, speed, _board.Bounds.RightWallX + 200f * _board.Frame.WorldPerPt);
            _view.PlanBurstDelays(target => Mathf.Max(0f, CellWorld(target).x - startX) / speed);
        }

        private Vector2 CellWorld((int Row, int Col) cell) => (Vector2)_board.transform.position + _board.Grid.GetWorldPosition(cell.Row, cell.Col);
    }
}
