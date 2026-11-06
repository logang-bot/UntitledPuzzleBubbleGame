using Game.Art;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Builds a skinned button: sliced (or native-size) face, sprite-swap
    /// pressed and disabled states and a centered caption that sits on the
    /// face above the 4 px lip.
    /// </summary>
    public class MenuButtonBuilder
    {
        private const float LipHeight = 4f;

        private readonly MenuArtLibrary _art;
        private readonly MenuTextStyles _styles;

        public MenuButtonBuilder(MenuArtLibrary art, MenuTextStyles styles)
        {
            _art = art;
            _styles = styles;
        }

        public SkinnedButton Build(RectTransform parent, UiSkinButtonSpec spec)
        {
            var skin = ButtonSkin.For(spec.Kind);
            var image = CreateFace(parent, spec, skin);
            var button = image.gameObject.AddComponent<Button>();
            ApplySpriteSwap(button, skin);
            if (spec.OnClick != null) button.onClick.AddListener(() => spec.OnClick());
            var skinned = image.gameObject.AddComponent<SkinnedButton>();
            skinned.Bind(CreateCaption(image.rectTransform, spec.Label, skin), skin.CaptionColor);
            return skinned;
        }

        private Image CreateFace(RectTransform parent, UiSkinButtonSpec spec, ButtonSkin skin)
        {
            var image = RuntimeUi.CreateImage(parent, spec.Name, UiPlacement.TopLeft(spec.Rect));
            image.sprite = _art.Frame(skin.SpriteId, skin.NormalFrame);
            image.type = image.sprite != null && image.sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
            return image;
        }

        private void ApplySpriteSwap(Button button, ButtonSkin skin)
        {
            button.transition = Selectable.Transition.SpriteSwap;
            var state = new SpriteState { pressedSprite = _art.Frame(skin.SpriteId, skin.PressedFrame) };
            state.selectedSprite = button.image.sprite;
            state.highlightedSprite = button.image.sprite;
            if (skin.HasDisabledFrame) state.disabledSprite = _art.Frame(skin.SpriteId, skin.DisabledFrame);
            button.spriteState = state;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
        }

        private Text CreateCaption(RectTransform face, string label, ButtonSkin skin)
        {
            if (string.IsNullOrEmpty(label)) return null;
            var caption = RuntimeUi.CreateLabel(face, "Caption", UiPlacement.Fill());
            _styles.Caption.WithColor(skin.CaptionColor).ApplyTo(caption);
            caption.rectTransform.offsetMin = new Vector2(0f, LipHeight);
            caption.text = label;
            caption.raycastTarget = false;
            return caption;
        }
    }
}
