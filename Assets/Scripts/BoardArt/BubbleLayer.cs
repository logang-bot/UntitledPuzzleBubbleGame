using System.Collections.Generic;
using Game.Grid;
using Game.Motion;
using Game.Settings;
using UnityEngine;

namespace Game.BoardArt
{
    /// <summary>
    /// The board's bubble sprites, one per occupied cell, under a root that
    /// slides on ceiling pushes. Spawns them with the build-in, settles and
    /// nudges on landing, bursts popped clusters in order and tosses
    /// dropped bubbles out of the board.
    /// </summary>
    public class BubbleLayer
    {
        private const float DropStaggerSeconds = 0.02f;

        private readonly Dictionary<(int Row, int Col), GameObject> _bubbles = new();
        private readonly List<GameObject> _bursting = new();
        private readonly List<GameObject> _falling = new();
        private readonly GameBoard _board;
        private readonly IBubbleSkin _skin;
        private readonly Transform _root;
        private System.Func<(int Row, int Col), float> _plannedDelay;
        private int _plannedFrame = -1;

        private float UnitsPerPt => _board.Frame.WorldPerPt;

        public BubbleLayer(GameBoard board, IBubbleSkin skin, Transform root)
        {
            _board = board;
            _skin = skin;
            _root = root;
        }

        public float SpawnAllBuildingIn()
        {
            var cells = _board.Grid.OccupiedCells();
            foreach (var cell in cells)
                TransformMotionPlayer.Play(Spawn(cell), GameMotions.BuildIn(BuildInSchedule.DelayFor(cell)), UnitsPerPt);
            return BuildInSchedule.TotalSeconds(cells);
        }

        public void Place((int Row, int Col) cell)
        {
            TransformMotionPlayer.Play(Spawn(cell), GameMotions.Settle(GameSettings.LandingAnimationStyle), UnitsPerPt);
            foreach (var neighbor in _board.Grid.GetNeighbors(cell.Row, cell.Col))
                if (_bubbles.TryGetValue(neighbor, out var bubble)) Nudge(bubble, AwayFrom(cell, neighbor));
        }

        /// <summary>Overrides the burst timing of every pop raised this frame (a superpower bursting its own way).</summary>
        public void PlanBurstDelays(System.Func<(int Row, int Col), float> delay)
        {
            _plannedDelay = delay;
            _plannedFrame = Time.frameCount;
        }

        public void Pop(IReadOnlyCollection<(int Row, int Col)> cells, BubbleColor color)
        {
            var delayOf = BurstDelays(cells);
            foreach (var cell in cells)
                if (_bubbles.Remove(cell, out var bubble)) Burst(bubble, (color, delayOf(cell)));
        }

        public void Drop(IReadOnlyCollection<(int Row, int Col)> cells, float floorY)
        {
            _falling.RemoveAll(bubble => bubble == null);
            var order = 0;
            foreach (var cell in cells)
                if (_bubbles.Remove(cell, out var bubble)) Toss(bubble, floorY, order++ * DropStaggerSeconds);
        }

        public void RebuildAfterPush()
        {
            ClearBubbles();
            foreach (var cell in _board.Grid.OccupiedCells()) Spawn(cell);
            _bursting.RemoveAll(bubble => bubble == null);
            foreach (var bubble in _bursting) bubble.transform.localPosition += Vector3.down * HexGridMath.RowHeight(_board.CellWidth);
        }

        public void ClearAll()
        {
            ClearBubbles();
            DestroyAll(_bursting);
            DestroyAll(_falling);
        }

        private GameObject Spawn((int Row, int Col) cell)
        {
            var bubble = _skin.Create(_board.Grid.GetColor(cell.Row, cell.Col), _root);
            bubble.name = $"Bubble_{cell.Row}_{cell.Col}";
            bubble.transform.localPosition = _board.Grid.GetWorldPosition(cell.Row, cell.Col);
            _bubbles[cell] = bubble;
            return bubble;
        }

        private Vector2 AwayFrom((int Row, int Col) landed, (int Row, int Col) neighbor)
        {
            var away = (_board.Grid.GetWorldPosition(neighbor.Row, neighbor.Col) - _board.Grid.GetWorldPosition(landed.Row, landed.Col)).normalized;
            return new Vector2(away.x, -away.y);
        }

        private void Nudge(GameObject bubble, Vector2 awayPt)
        {
            TransformMotionPlayer.Play(bubble, GameMotions.NeighborNudge(awayPt * 2f), UnitsPerPt);
        }

        private System.Func<(int Row, int Col), float> BurstDelays(IReadOnlyCollection<(int Row, int Col)> cells)
        {
            if (_plannedFrame == Time.frameCount) return _plannedDelay;
            var delays = PopSchedule.Delays(cells, cell => _board.Grid.GetNeighbors(cell.Row, cell.Col));
            return cell => delays[cell];
        }

        private void Burst(GameObject bubble, (BubbleColor Color, float Delay) pop)
        {
            BubblePop.Begin(bubble, _skin.PopFrames(pop.Color), pop.Delay);
            _bursting.Add(bubble);
        }

        private void Toss(GameObject bubble, float floorY, float delay)
        {
            TransformMotionPlayer.Stop(bubble);
            bubble.transform.SetParent(null, worldPositionStays: true);
            _skin.MarkScared(bubble);
            bubble.AddComponent<FallingBubble>().Configure(UnitsPerPt, floorY, delay);
            _falling.Add(bubble);
        }

        private void ClearBubbles()
        {
            foreach (var bubble in _bubbles.Values) Object.Destroy(bubble);
            _bubbles.Clear();
        }

        private static void DestroyAll(List<GameObject> bubbles)
        {
            foreach (var bubble in bubbles)
                if (bubble != null) Object.Destroy(bubble);
            bubbles.Clear();
        }
    }
}
