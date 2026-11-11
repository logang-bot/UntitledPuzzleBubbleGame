using Game.Art;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>Shows the field_empty / field_focused / field_filled face for the name field's state.</summary>
    [RequireComponent(typeof(Image))]
    public class NameFieldSkin : MonoBehaviour
    {
        private TMP_InputField _field;
        private Image _image;
        private Sprite _empty;
        private Sprite _focused;
        private Sprite _filled;

        public void Bind(TMP_InputField field, MenuArtLibrary art)
        {
            _field = field;
            _image = GetComponent<Image>();
            _empty = art.Frame("field_empty", 0);
            _focused = art.Frame("field_focused", 0);
            _filled = art.Frame("field_filled", 0);
        }

        private void LateUpdate()
        {
            if (_field == null) return;
            var face = CurrentFace();
            if (_image.sprite != face) _image.sprite = face;
        }

        private Sprite CurrentFace()
        {
            if (_field.isFocused) return _focused;
            return string.IsNullOrEmpty(_field.text) ? _empty : _filled;
        }
    }
}
