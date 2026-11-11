using Game.Motion;
using Game.UI;
using TMPro;
using UnityEngine;

namespace Game.Results
{
    /// <summary>
    /// A result banner (win, lose, or the draw plate) with its caption, which
    /// drops in from above with an overshoot (banner_drop) every time it is shown.
    /// </summary>
    public class BannerView
    {
        public GameObject Root => _drop.gameObject;
        public TMP_Text Caption { get; }

        private readonly MotionPlayer _drop;

        public BannerView(MenuUiFactory ui, RectTransform parent, BannerSpec spec)
        {
            var group = RuntimeUi.CreateRect(parent, spec.Text, UiPlacement.TopLeft(spec.Rect));
            UiPlacement.MovePivot(group, new Vector2(0.5f, 0.5f));
            ui.Sprite(group, spec.SpriteId, new RectInt(0, 0, spec.Rect.width, spec.Rect.height));
            Caption = ui.LabelIn(group, spec.TextRect, (spec.Text, spec.Style));
            _drop = MotionPlayer.Attach(group, MenuMotions.BannerDrop);
        }

        public void Show(bool visible)
        {
            Root.SetActive(visible);
            if (visible) _drop.Play();
        }
    }
}
