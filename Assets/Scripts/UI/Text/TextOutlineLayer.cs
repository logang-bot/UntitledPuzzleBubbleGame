using Game.Art;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// The outline and drop shadow of a styled text, drawn as a second TMP
    /// text just behind it, like the guide strokes the whole word before it
    /// fills it. TMP's own outline is drawn per letter, so a thick outline
    /// paints over the previous letter's face. Lives on the face text, keeps
    /// the copy's text and rect in sync every frame, and follows its
    /// enabled state and lifetime.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class TextOutlineLayer : MonoBehaviour
    {
        private TMP_Text _face;
        private TextMeshProUGUI _back;

        public static TextOutlineLayer For(TMP_Text face)
        {
            if (face.TryGetComponent<TextOutlineLayer>(out var layer)) return layer;
            layer = face.gameObject.AddComponent<TextOutlineLayer>();
            layer.CreateBack(face);
            return layer;
        }

        public void Show(TextStyle style, MenuArtFonts fonts)
        {
            _back.gameObject.SetActive(enabled);
            TextStyleApplier.ApplyLayout(_back, style, fonts);
            _back.enableVertexGradient = false;
            _back.color = style.Outline > 0f ? style.OutlineColor : style.ShadowColor;
            _back.fontSharedMaterial = TextStyleMaterials.ForOutline(_back.font, style);
            Sync();
        }

        public void Hide() => _back.gameObject.SetActive(false);

        private void CreateBack(TMP_Text face)
        {
            _face = face;
            var back = new GameObject(face.name + " Outline", typeof(RectTransform));
            back.transform.SetParent(face.transform.parent, worldPositionStays: false);
            back.transform.SetSiblingIndex(face.transform.GetSiblingIndex());
            _back = back.AddComponent<TextMeshProUGUI>();
            _back.raycastTarget = false;
        }

        private void LateUpdate() => Sync();

        private void OnEnable()
        {
            if (_back != null) _back.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            if (_back != null) _back.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_back != null) Destroy(_back.gameObject);
        }

        private void Sync()
        {
            if (_back.text != _face.text) _back.text = _face.text;
            CopyRect(_face.rectTransform, _back.rectTransform);
        }

        private static void CopyRect(RectTransform from, RectTransform to)
        {
            to.anchorMin = from.anchorMin;
            to.anchorMax = from.anchorMax;
            to.pivot = from.pivot;
            to.sizeDelta = from.sizeDelta;
            to.anchoredPosition = from.anchoredPosition;
            to.localScale = from.localScale;
            to.localRotation = from.localRotation;
        }
    }
}
