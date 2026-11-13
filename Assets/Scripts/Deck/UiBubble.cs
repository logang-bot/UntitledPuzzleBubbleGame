using Game.Art;
using Game.BoardArt;
using Game.Grid;
using Game.Motion;
using Game.Superpowers;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Deck
{
    /// <summary>
    /// A cartoon bubble on the deck canvas (loaded, next or feeding), drawn
    /// one cell (41.4 pt) wide: a colored bubble with its glyph, or an armed
    /// superpower (the bomb's spark flickers, the rainbow's swirl spins).
    /// </summary>
    public class UiBubble
    {
        public const float CellScale = PlayfieldSpec.CellPt / BoardArtScale.BubbleSpritePt;
        private const int SuperpowerLook = 100;

        public RectTransform Root { get; }

        private readonly MenuArtLibrary _art;
        private readonly Image _body;
        private readonly Image _overlay;
        private int _shownLook = -1;

        public UiBubble(DeckUi ui, RectTransform parent, Vector2 point)
        {
            _art = ui.Art;
            Root = RuntimeUi.CreateRect(parent, "Bubble", UiPlacement.CenteredAt(point, Vector2.zero));
            Root.localScale = Vector3.one * CellScale;
            _body = ui.At(Root, BubbleArtIds.Body(BubbleColor.Red), Vector2.zero);
            _overlay = ui.At(Root, BubbleArtIds.Glyph(BubbleColor.Red), Vector2.zero);
        }

        public void Show(BubbleColor color)
        {
            if (!ChangeLook((int)color)) return;
            Dress(BubbleArtIds.Body(color), (BubbleArtIds.Glyph(color), Vector2.zero));
        }

        public void ShowSuperpower(SuperpowerId id)
        {
            if (!ChangeLook(SuperpowerLook + (int)id)) return;
            Dress(SuperpowerArtIds.Body(id), (SuperpowerArtIds.Overlay(id), SuperpowerArtIds.OverlayOffset(id)));
            if (id == SuperpowerId.Rainbow) MotionPlayer.AttachAndPlay(_body.rectTransform, GameMotions.RainbowSpin);
            if (id == SuperpowerId.Bomb) MotionPlayer.AttachAndPlay(_overlay.rectTransform, GameMotions.BombSpark);
        }

        private bool ChangeLook(int look)
        {
            if (_shownLook == look) return false;
            _shownLook = look;
            StopMotion(_body);
            StopMotion(_overlay);
            return true;
        }

        private static void StopMotion(Image image)
        {
            if (image.TryGetComponent<MotionPlayer>(out var player)) { player.enabled = false; Object.Destroy(player); }
            image.rectTransform.localRotation = Quaternion.identity;
            image.rectTransform.localScale = Vector3.one;
        }

        private void Dress(string bodyId, (string Id, Vector2 Offset) overlay)
        {
            SetSprite(_body, bodyId);
            _overlay.enabled = overlay.Id != null;
            if (overlay.Id != null) SetSprite(_overlay, overlay.Id);
            _overlay.rectTransform.anchoredPosition = overlay.Offset;
        }

        private void SetSprite(Image image, string spriteId)
        {
            var sprite = _art.Frame(spriteId, 0);
            image.sprite = sprite;
            if (sprite != null) image.rectTransform.sizeDelta = sprite.rect.size / sprite.pixelsPerUnit;
        }
    }
}
