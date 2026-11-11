using Game.Art;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Builds a cartoon button: sliced face with sprite-swap pressed and
    /// disabled states, and a caption that sits on the face, above the 6 pt
    /// lip (1.5 pt from the top, 7.5 pt from the bottom, as in the guide).
    /// </summary>
    public class MenuButtonBuilder
    {
        private const float CaptionTopInset = 1.5f;
        private const float CaptionBottomInset = 7.5f;

        private readonly MenuArtLibrary _art;

        public MenuButtonBuilder(MenuArtLibrary art)
        {
            _art = art;
        }

        public SkinnedButton Build(RectTransform parent, UiSkinButtonSpec spec)
        {
            var skin = ButtonSkin.For(spec.Kind);
            var image = CreateFace(parent, spec, skin);
            var button = image.gameObject.AddComponent<Button>();
            ApplySpriteSwap(button, skin);
            if (spec.OnClick != null) button.onClick.AddListener(() => spec.OnClick());
            var skinned = image.gameObject.AddComponent<SkinnedButton>();
            skinned.Bind(skin.HasCaption ? CreateCaption(image.rectTransform, spec.Label, skin.Caption) : null, (_art.Fonts, skin.Caption));
            return skinned;
        }

        private Image CreateFace(RectTransform parent, UiSkinButtonSpec spec, ButtonSkin skin)
        {
            var image = RuntimeUi.CreateImage(parent, spec.Name, UiPlacement.TopLeft(spec.Rect));
            image.sprite = _art.Frame(skin.NormalId, 0);
            image.type = image.sprite != null && image.sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
            UiPlacement.MovePivot(image.rectTransform, new Vector2(0.5f, 0.5f));
            return image;
        }

        private void ApplySpriteSwap(Button button, ButtonSkin skin)
        {
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState
            {
                pressedSprite = _art.Frame(skin.PressedId, 0),
                selectedSprite = button.image.sprite,
                highlightedSprite = button.image.sprite,
                disabledSprite = skin.HasCaption ? _art.Frame(ButtonSkin.DisabledFace, 0) : button.image.sprite
            };
            button.navigation = new Navigation { mode = Navigation.Mode.None };
        }

        private TMP_Text CreateCaption(RectTransform face, string label, TextStyle style)
        {
            var caption = RuntimeUi.CreateRect(face, "Caption", UiPlacement.Fill()).gameObject.AddComponent<TextMeshProUGUI>();
            caption.rectTransform.offsetMin = new Vector2(0f, CaptionBottomInset);
            caption.rectTransform.offsetMax = new Vector2(0f, -CaptionTopInset);
            TextStyleApplier.Apply(caption, style, _art.Fonts);
            caption.text = label;
            caption.raycastTarget = false;
            return caption;
        }
    }
}
