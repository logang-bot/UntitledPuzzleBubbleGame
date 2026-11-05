using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Builds placeholder uGUI elements at runtime, shared by the menu screens
    /// until the art pass replaces them with skinned prefabs. Uses the legacy
    /// built-in font, same as the original menus.
    /// </summary>
    public static class RuntimeUi
    {
        private const int DefaultFontSize = 30;
        private static readonly Color PlaceholderTextColor = new Color(0f, 0f, 0f, 0.4f);

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

        public static InputField CreateInputField(RectTransform parent, UiInputSpec spec)
        {
            var image = CreateImage(parent, spec.Name, spec.Placement);
            var field = image.gameObject.AddComponent<InputField>();
            field.textComponent = CreateInputText(image.rectTransform);
            field.placeholder = CreatePlaceholder(image.rectTransform, spec.Placeholder);
            field.characterLimit = spec.CharacterLimit;
            field.text = spec.InitialText;
            return field;
        }

        private static Text CreateInputText(RectTransform fieldRect)
        {
            var text = CreateLabel(fieldRect, "Text", UiPlacement.Fill());
            text.supportRichText = false;
            return text;
        }

        private static Text CreatePlaceholder(RectTransform fieldRect, string placeholder)
        {
            var text = CreateLabel(fieldRect, "Placeholder", UiPlacement.Fill());
            text.text = placeholder;
            text.fontStyle = FontStyle.Italic;
            text.color = PlaceholderTextColor;
            return text;
        }
    }
}
