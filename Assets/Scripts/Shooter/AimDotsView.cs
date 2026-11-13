using System.Collections.Generic;
using Game.Art;
using Game.BoardArt;
using UnityEngine;

namespace Game.Shooter
{
    /// <summary>
    /// Draws the aim line as cartoon dots (aim_dots): 23 pt past the pointer,
    /// one every 20 pt, scrolling forward at 30 pt/s and fading toward the
    /// landing point. Dot sprites are pooled and drawn behind the bubbles.
    /// </summary>
    public class AimDotsView : MonoBehaviour
    {
        private const float StartPt = 23f;
        private const float GapPt = 20f;
        private const float ScrollPtPerSecond = 30f;

        private readonly List<SpriteRenderer> _pool = new();
        private readonly List<(Vector2 Position, float Alpha)> _dots = new();
        private Sprite _sprite;
        private float _unitsPerPt;
        private float _scrollPt;

        public static AimDotsView Create(Transform parent, MenuArtLibrary art, float unitsPerPt)
        {
            var view = new GameObject("AimDots").AddComponent<AimDotsView>();
            view.transform.SetParent(parent, worldPositionStays: false);
            view._sprite = art.Frame("aim_dot", 0);
            view._unitsPerPt = unitsPerPt;
            return view;
        }

        public void Show(IReadOnlyList<Vector2> path)
        {
            _scrollPt += ScrollPtPerSecond * Time.deltaTime;
            AimDotPlacer.Place(path, new AimDotSpacing(StartPt * _unitsPerPt, GapPt * _unitsPerPt, _scrollPt * _unitsPerPt), _dots);
            for (var i = 0; i < _dots.Count; i++) Draw(DotAt(i), _dots[i]);
            for (var i = _dots.Count; i < _pool.Count; i++) _pool[i].enabled = false;
        }

        public void Hide()
        {
            foreach (var dot in _pool) dot.enabled = false;
        }

        private SpriteRenderer DotAt(int index)
        {
            while (_pool.Count <= index) _pool.Add(NewDot());
            return _pool[index];
        }

        private SpriteRenderer NewDot()
        {
            var dot = SpriteChild.Create(transform, "Dot", _sprite, BoardSorting.AimDots).GetComponent<SpriteRenderer>();
            dot.transform.localScale = Vector3.one * _unitsPerPt;
            return dot;
        }

        private static void Draw(SpriteRenderer dot, (Vector2 Position, float Alpha) placement)
        {
            dot.enabled = true;
            dot.transform.position = placement.Position;
            dot.color = new Color(1f, 1f, 1f, placement.Alpha);
        }
    }
}
