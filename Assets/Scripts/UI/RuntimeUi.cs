using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Builds plain uGUI elements at runtime: the rect/image/label primitives
    /// MenuUiFactory skins on top of, plus unskinned debug buttons. Uses the
    /// legacy built-in font.
    /// </summary>
    public static class RuntimeUi
    {
        private const int DefaultFontSize = 30;

        public static RectTransform CreateRect(RectTransform parent, string name, UiPlacement placement)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, worldPositionStays: false);
            placement.ApplyTo(rect);
            return rect;
        }

        public static Image CreateImage(RectTransform parent, string name, UiPlacement placement)
        {
            return CreateRect(parent, name, placement).gameObject.AddComponent<Image>();
        }

        public static Text CreateLabel(RectTransform parent, string name, UiPlacement placement)
        {
            var text = CreateRect(parent, name, placement).gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = DefaultFontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            return text;
        }

        public static Button CreateButton(RectTransform parent, UiButtonSpec spec)
        {
            var image = CreateImage(parent, spec.Name, spec.Placement);
            CreateLabel(image.rectTransform, "Label", UiPlacement.Fill()).text = spec.Label;
            var button = image.gameObject.AddComponent<Button>();
            if (spec.OnClick != null) button.onClick.AddListener(() => spec.OnClick());
            return button;
        }
    }
}
