using Game.Art;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Skinned counterpart of RuntimeUi: builds the menu widgets from the
    /// Claude Design sheets. All rects are native pixels from the parent's
    /// top-left (see UiPlacement.TopLeft).
    /// </summary>
    public class MenuUiFactory
    {
        public MenuArtLibrary Art { get; }
        public MenuTextStyles Styles { get; }

        private readonly MenuButtonBuilder _buttons;

        public MenuUiFactory(MenuArtLibrary art)
        {
            Art = art;
            Styles = new MenuTextStyles(art);
            _buttons = new MenuButtonBuilder(art, Styles);
        }

        public Image Sprite(RectTransform parent, string spriteId, RectInt rect)
        {
            var image = RuntimeUi.CreateImage(parent, spriteId, UiPlacement.TopLeft(rect));
            image.sprite = Art.Frame(spriteId, 0);
            image.type = image.sprite != null && image.sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
            image.raycastTarget = false;
            return image;
        }

        public SpriteFrameAnimator AnimatedSprite(RectTransform parent, string spriteId, RectInt rect)
        {
            var animator = Sprite(parent, spriteId, rect).gameObject.AddComponent<SpriteFrameAnimator>();
            animator.Play(Art.Find(spriteId));
            return animator;
        }

        public Image Fill(RectTransform parent, string name, Color color)
        {
            var image = RuntimeUi.CreateImage(parent, name, UiPlacement.Fill());
            image.color = color;
            return image;
        }

        public Text Label(RectTransform parent, UiLabelSpec spec)
        {
            var text = RuntimeUi.CreateLabel(parent, spec.Name, spec.Placement);
            spec.Style.ApplyTo(text);
            text.text = spec.Text;
            text.raycastTarget = false;
            return text;
        }

        public Text CenteredLabel(RectTransform parent, string text, LabelLine line)
        {
            return Label(parent, new UiLabelSpec
            {
                Name = "Label",
                Text = text,
                Style = line.Style,
                Placement = UiPlacement.TopBand(line.Y, line.Style.Size + 4)
            });
        }

        public SkinnedButton Button(RectTransform parent, UiSkinButtonSpec spec) => _buttons.Build(parent, spec);

        public InputField NameField(RectTransform parent, UiInputSpec spec) => NameFieldBuilder.Build(this, parent, spec);

        public Text Plate(RectTransform parent, RectInt rect, string caption)
        {
            var plate = Sprite(parent, "plate", rect);
            return Label(plate.rectTransform, new UiLabelSpec
            {
                Name = "Caption",
                Text = caption,
                Style = Styles.Caption,
                Placement = UiPlacement.Fill()
            });
        }
    }
}
