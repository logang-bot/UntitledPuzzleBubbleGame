using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Shooter
{
    /// <summary>
    /// Tracks whether a UI zone is currently pressed. Attached to the
    /// rotate-left, rotate-right, and fire on-screen zones; the same
    /// press/release plumbing works for both mouse (Editor) and touch
    /// (device) via Unity's UI event system.
    /// </summary>
    public class HoldInputZone : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public bool IsPressed { get; private set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            IsPressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsPressed = false;
        }
    }
}
