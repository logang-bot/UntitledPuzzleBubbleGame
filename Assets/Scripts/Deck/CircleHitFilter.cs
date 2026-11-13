using UnityEngine;

namespace Game.Deck
{
    /// <summary>Limits a centered UI element's touches to the circle inscribed in its rect.</summary>
    [RequireComponent(typeof(RectTransform))]
    public class CircleHitFilter : MonoBehaviour, ICanvasRaycastFilter
    {
        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            var rect = (RectTransform)transform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, eventCamera, out var local)) return false;
            return (local - rect.rect.center).magnitude <= rect.rect.width * 0.5f;
        }
    }
}
