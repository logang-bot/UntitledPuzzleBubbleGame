using Game.Art;
using Game.Grid;
using Game.Superpowers;
using UnityEngine;

namespace Game.BoardArt
{
    /// <summary>How a bubble looks: the cartoon sprites, or the plain debug circles.</summary>
    public interface IBubbleSkin
    {
        GameObject Create(BubbleColor color, Transform parent);
        GameObject CreateSuperpower(SuperpowerId id, Transform parent);
        void MarkScared(GameObject bubble);
        SpriteAnimEntry PopFrames(BubbleColor color);
    }
}
