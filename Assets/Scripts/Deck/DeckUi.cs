using Game.Art;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Deck
{
    /// <summary>
    /// The menu UI kit (MenuUiFactory) with the deck's fractional pt rects:
    /// the guide's mechanism sits on 5/3 pt steps, so rects aren't whole pt.
    /// </summary>
    public class DeckUi
    {
        private static readonly Vector2 TopLeftPivot = new(0f, 1f);

        public MenuUiFactory Factory { get; }
        public MenuArtLibrary Art => Factory.Art;

        public DeckUi(MenuArtLibrary art)
        {
            Factory = new MenuUiFactory(art);
        }

        public Image Sprite(RectTransform parent, string spriteId, Rect rect)
        {
            var image = RuntimeUi.CreateImage(parent, spriteId, Placement(rect));
            image.sprite = Art.Frame(spriteId, 0);
            image.type = image.sprite != null && image.sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
            image.raycastTarget = false;
            return image;
        }

        public Image At(RectTransform parent, string spriteId, Vector2 point) => Factory.PivotSprite(parent, spriteId, point);

        public TMP_Text Label(RectTransform parent, Rect rect, (string Text, TextStyle Style) content)
        {
            return Factory.Label(parent, new UiLabelSpec { Name = content.Text, Text = content.Text, Style = content.Style, Placement = Placement(rect) });
        }

        public RectTransform Group(RectTransform parent, string name) => RuntimeUi.CreateRect(parent, name, UiPlacement.Fill());

        public static void FlipAroundCenter(RectTransform rect)
        {
            UiPlacement.MovePivot(rect, new Vector2(0.5f, 0.5f));
            rect.localScale = new Vector3(-rect.localScale.x, rect.localScale.y, 1f);
        }

        private static UiPlacement Placement(Rect rect) => UiPlacement.PivotAt(rect.position, rect.size, TopLeftPivot);
    }
}
