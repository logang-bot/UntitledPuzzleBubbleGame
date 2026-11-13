using UnityEngine;

namespace Game.BoardArt
{
    /// <summary>Creates a named world-space sprite under a parent, at its origin.</summary>
    public static class SpriteChild
    {
        public static GameObject Create(Transform parent, string name, Sprite sprite, int sortingOrder)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, worldPositionStays: false);
            var spriteRenderer = child.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.sortingOrder = sortingOrder;
            return child;
        }

        public static SpriteRenderer Tiled(Transform parent, string name, Sprite sprite, int sortingOrder)
        {
            var spriteRenderer = Create(parent, name, sprite, sortingOrder).GetComponent<SpriteRenderer>();
            spriteRenderer.drawMode = SpriteDrawMode.Tiled;
            spriteRenderer.tileMode = SpriteTileMode.Continuous;
            return spriteRenderer;
        }
    }
}
