using Game.Art;
using Game.Motion;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Companion to a sprite-swap Button: drops the caption 4 pt with the
    /// pressed face, springs the button back (0.96 → 1, btn_press) on release,
    /// and switches the caption to the disabled style while not interactable.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SkinnedButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public const float PressDrop = 4f;

        public Button Button { get; private set; }
        public TMP_Text Caption { get; private set; }

        private MenuArtFonts _fonts;
        private TextStyle _captionStyle;
        private Vector2 _captionRest;
        private MotionPlayer _release;
        private bool _pressed;

        public void Bind(TMP_Text caption, (MenuArtFonts Fonts, TextStyle Style) captionLook)
        {
            Button = GetComponent<Button>();
            Caption = caption;
            _fonts = captionLook.Fonts;
            _captionStyle = captionLook.Style;
            _release = MotionPlayer.Attach((RectTransform)transform, MenuMotions.ButtonRelease);
            if (caption != null) _captionRest = caption.rectTransform.anchoredPosition;
        }

        public void SetInteractable(bool interactable)
        {
            Button.interactable = interactable;
            if (Caption != null) TextStyleApplier.Apply(Caption, interactable ? _captionStyle : TextStyles.ButtonDisabled, _fonts);
        }

        public void SetLabel(string label)
        {
            if (Caption != null) Caption.text = label;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Button.IsInteractable()) return;
            _pressed = true;
            MoveCaption(-PressDrop);
        }

        public void OnPointerUp(PointerEventData eventData) => Release();

        public void OnPointerExit(PointerEventData eventData) => Release();

        private void Release()
        {
            if (!_pressed) return;
            _pressed = false;
            MoveCaption(0f);
            _release.Play();
        }

        private void MoveCaption(float drop)
        {
            if (Caption != null) Caption.rectTransform.anchoredPosition = _captionRest + new Vector2(0f, drop);
        }
    }
}
