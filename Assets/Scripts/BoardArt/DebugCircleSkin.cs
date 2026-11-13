using Game.Art;
using Game.Grid;
using Game.Superpowers;
using UnityEngine;

namespace Game.BoardArt
{
    /// <summary>The Milestone-1 plain tinted circles, kept for chasing grid bugs (BoardView's debug toggle).</summary>
    public class DebugCircleSkin : IBubbleSkin
    {
        private static readonly SpriteAnimEntry NoFrames = new();

        private readonly Sprite _circle = CircleSpriteFactory.CreateWhiteCircle();
        private readonly float _cellWidth;

        public DebugCircleSkin(float cellWidth)
        {
            _cellWidth = cellWidth;
        }

        public GameObject Create(BubbleColor color, Transform parent)
        {
            var bubble = SpriteChild.Create(parent, "Bubble", _circle, BoardSorting.Bubble);
            bubble.transform.localScale = new Vector3(_cellWidth, _cellWidth, 1f);
            bubble.GetComponent<SpriteRenderer>().color = BubbleColorPalette.ToColor(color);
            return bubble;
        }

        public GameObject CreateSuperpower(SuperpowerId id, Transform parent)
        {
            var bubble = SpriteChild.Create(parent, "Superpower_" + id, _circle, BoardSorting.Front);
            bubble.transform.localScale = new Vector3(_cellWidth, _cellWidth, 1f);
            return bubble;
        }

        public void MarkScared(GameObject bubble)
        {
            bubble.GetComponent<SpriteRenderer>().sortingOrder = BoardSorting.Front;
        }

        public SpriteAnimEntry PopFrames(BubbleColor color) => NoFrames;
    }
}
