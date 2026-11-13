using Game.Art;
using Game.Grid;
using Game.Motion;
using Game.Superpowers;
using UnityEngine;

namespace Game.BoardArt
{
    /// <summary>
    /// The guide's glass bubbles: a body sprite with its colorblind glyph on
    /// top, scaled to one cell. Falling bubbles get the scared overlay and
    /// move in front of the board.
    /// </summary>
    public class CartoonBubbleSkin : IBubbleSkin
    {
        private readonly MenuArtLibrary _art;
        private readonly float _scale;

        public CartoonBubbleSkin(MenuArtLibrary art, float cellWidth)
        {
            _art = art;
            _scale = BoardArtScale.Bubble(cellWidth);
        }

        public GameObject Create(BubbleColor color, Transform parent)
        {
            var bubble = SpriteChild.Create(parent, "Bubble", _art.Frame(BubbleArtIds.Body(color), 0), BoardSorting.Bubble);
            bubble.transform.localScale = new Vector3(_scale, _scale, 1f);
            SpriteChild.Create(bubble.transform, "Glyph", _art.Frame(BubbleArtIds.Glyph(color), 0), BoardSorting.BubbleGlyph);
            return bubble;
        }

        public GameObject CreateSuperpower(SuperpowerId id, Transform parent)
        {
            var bubble = new GameObject("Superpower_" + id);
            bubble.transform.SetParent(parent, worldPositionStays: false);
            bubble.transform.localScale = new Vector3(_scale, _scale, 1f);
            var body = SpriteChild.Create(bubble.transform, "Body", _art.Frame(SuperpowerArtIds.Body(id), 0), BoardSorting.Front);
            if (id == SuperpowerId.Rainbow) TransformMotionPlayer.Play(body, GameMotions.RainbowSpin, 1f);
            AddOverlay(bubble.transform, id);
            return bubble;
        }

        public void MarkScared(GameObject bubble)
        {
            SpriteChild.Create(bubble.transform, "Scared", _art.Frame(BubbleArtIds.Scared, 0), BoardSorting.BubbleOverlay);
            foreach (var spriteRenderer in bubble.GetComponentsInChildren<SpriteRenderer>())
                spriteRenderer.sortingOrder += BoardSorting.Front;
        }

        public SpriteAnimEntry PopFrames(BubbleColor color) => _art.Find(BubbleArtIds.Pop(color));

        private void AddOverlay(Transform bubble, SuperpowerId id)
        {
            var overlayId = SuperpowerArtIds.Overlay(id);
            if (overlayId == null) return;
            var overlay = SpriteChild.Create(bubble, "Overlay", _art.Frame(overlayId, 0), BoardSorting.Front + 1);
            overlay.transform.localPosition = SuperpowerArtIds.OverlayOffset(id);
            if (id == SuperpowerId.Bomb) TransformMotionPlayer.Play(overlay, GameMotions.BombSpark, 1f);
        }
    }
}
