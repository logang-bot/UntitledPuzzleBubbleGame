using Game.Art;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Builds the menu widgets from the Claude Design sheets: cartoon sprites
    /// and 9-slices, TMP labels in the guide's text styles, skinned buttons,
    /// the name field, and pixel-art sprites kept on whole screen pixels.
    /// Rects are native pt from the parent's top-left (UiPlacement.TopLeft).
    /// </summary>
    public class MenuUiFactory
    {
        public MenuArtLibrary Art { get; }
        public MenuArtFonts Fonts { get; }

        private readonly MenuButtonBuilder _buttons;

        public MenuUiFactory(MenuArtLibrary art)
        {
            Art = art;
            Fonts = art.Fonts;
            _buttons = new MenuButtonBuilder(art);
        }

        public Image Sprite(RectTransform parent, string spriteId, RectInt rect)
        {
            var image = RuntimeUi.CreateImage(parent, spriteId, UiPlacement.TopLeft(rect));
            image.sprite = Art.Frame(spriteId, 0);
            image.type = image.sprite != null && image.sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
            image.raycastTarget = false;
            return image;
        }

        public Image Tiled(RectTransform parent, string spriteId, RectInt rect)
        {
            var image = Sprite(parent, spriteId, rect);
            image.type = Image.Type.Tiled;
            return image;
        }

        /// <summary>A sprite at its own pt size whose legend pivot sits on <paramref name="nativePoint"/>.</summary>
        public Image PivotSprite(RectTransform parent, string spriteId, Vector2 nativePoint)
        {
            var sprite = Art.Frame(spriteId, 0);
            var size = sprite != null ? sprite.rect.size / sprite.pixelsPerUnit : Vector2.zero;
            var pivot = sprite != null ? sprite.pivot / sprite.rect.size : new Vector2(0.5f, 0.5f);
            var image = RuntimeUi.CreateImage(parent, spriteId, UiPlacement.PivotAt(nativePoint, size, pivot));
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        /// <summary>A pixel-art sprite whose pivot sits on a native point, scaled to whole screen pixels per art pixel.</summary>
        public Image PixelSprite(RectTransform parent, string spriteId, PixelPlacement placement)
        {
            var image = PivotSprite(parent, spriteId, placement.Pivot);
            var artSize = image.sprite != null ? image.sprite.rect.size : Vector2.zero;
            PixelSnap.Attach(image.rectTransform, artSize, placement.DesignDevicePixels);
            return image;
        }

        /// <summary>A static backdrop sprite scaled to cover its whole parent, keeping its aspect (the screen's bleed area included).</summary>
        public Image Cover(RectTransform parent, string spriteId)
        {
            var image = Fill(parent, spriteId, Color.white);
            image.sprite = Art.Frame(spriteId, 0);
            image.raycastTarget = false;
            var fitter = image.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = image.sprite != null ? image.sprite.rect.width / image.sprite.rect.height : 1f;
            return image;
        }

        public Image Fill(RectTransform parent, string name, Color color)
        {
            var image = RuntimeUi.CreateImage(parent, name, UiPlacement.Fill());
            image.color = color;
            return image;
        }

        public TMP_Text Label(RectTransform parent, UiLabelSpec spec)
        {
            var text = RuntimeUi.CreateRect(parent, spec.Name, spec.Placement).gameObject.AddComponent<TextMeshProUGUI>();
            TextStyleApplier.Apply(text, spec.Style, Fonts);
            text.text = spec.Text;
            text.raycastTarget = false;
            return text;
        }

        public TMP_Text LabelIn(RectTransform parent, RectInt rect, (string Text, TextStyle Style) content)
        {
            return Label(parent, new UiLabelSpec { Name = content.Text, Text = content.Text, Style = content.Style, Placement = UiPlacement.TopLeft(rect) });
        }

        public SkinnedButton Button(RectTransform parent, UiSkinButtonSpec spec) => _buttons.Build(parent, spec);

        public TMP_InputField NameField(RectTransform parent, UiInputSpec spec) => NameFieldBuilder.Build(this, parent, spec);

        /// <summary>The coral title plate with its caption, which sits 4 pt above the plate's lower edge as in the guide.</summary>
        public TMP_Text Plate(RectTransform parent, RectInt rect, string caption)
        {
            var plate = Sprite(parent, "title_plate", rect);
            return LabelIn(plate.rectTransform, new RectInt(0, 0, rect.width, rect.height - 4), (caption, TextStyles.TitlePlate));
        }
    }
}
