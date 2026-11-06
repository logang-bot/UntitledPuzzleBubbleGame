using Game.Art;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Shows the name_field frame for the field's state: 0 empty,
    /// 1 focused, 2 filled.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class NameFieldSkin : MonoBehaviour
    {
        private const int EmptyFrame = 0;
        private const int FocusedFrame = 1;
        private const int FilledFrame = 2;

        private InputField _field;
        private SpriteAnimEntry _frames;
        private Image _image;

        public void Bind(InputField field, SpriteAnimEntry frames)
        {
            _field = field;
            _frames = frames;
            _image = GetComponent<Image>();
        }

        private void LateUpdate()
        {
            if (_field == null || _frames.Frames.Length <= FilledFrame) return;
            _image.sprite = _frames.Frames[CurrentFrame()];
        }

        private int CurrentFrame()
        {
            if (_field.isFocused) return FocusedFrame;
            return string.IsNullOrEmpty(_field.text) ? EmptyFrame : FilledFrame;
        }
    }
}
