using Game.Art;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Companion to a sprite-swap Button: drops the caption 3 px with the
    /// pressed face and greys it out (Stone) while disabled, matching the
    /// Claude Design button kit.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SkinnedButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public const float PressDrop = 3f;

        public Button Button { get; private set; }
        public Text Caption { get; private set; }

        private Color _captionColor;
        private Vector2 _captionRest;

        public void Bind(Text caption, Color captionColor)
        {
            Button = GetComponent<Button>();
            Caption = caption;
            _captionColor = captionColor;
            if (caption != null) _captionRest = caption.rectTransform.anchoredPosition;
        }

        public void SetInteractable(bool interactable)
        {
            Button.interactable = interactable;
            if (Caption != null) Caption.color = interactable ? _captionColor : MenuPalette.Stone;
        }

        public void SetLabel(string label)
        {
            if (Caption != null) Caption.text = label;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Button.IsInteractable()) MoveCaption(-PressDrop);
        }

        public void OnPointerUp(PointerEventData eventData) => MoveCaption(0f);

        public void OnPointerExit(PointerEventData eventData) => MoveCaption(0f);

        private void MoveCaption(float drop)
        {
            if (Caption != null) Caption.rectTransform.anchoredPosition = _captionRest + new Vector2(0f, drop);
        }
    }
}
