using Game.Art;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// The pet name field: a sliced field_* face (state picked by
    /// NameFieldSkin), name_text / name_placeholder text inset 18 pt inside a
    /// masked text area, and an ink caret blinking once a second.
    /// </summary>
    public static class NameFieldBuilder
    {
        private const float TextInset = 18f;
        private const int CaretWidth = 3;
        private const float CaretBlinksPerSecond = 1f;

        public static TMP_InputField Build(MenuUiFactory ui, RectTransform parent, UiInputSpec spec)
        {
            var face = ui.Sprite(parent, "field_empty", spec.Rect);
            face.raycastTarget = true;
            var area = CreateTextArea(face.rectTransform);
            var field = face.gameObject.AddComponent<TMP_InputField>();
            field.textViewport = area;
            field.placeholder = CreateText(ui, area, ("Placeholder", TextStyles.NamePlaceholder));
            field.textComponent = CreateText(ui, area, ("Text", TextStyles.NameText));
            ((TMP_Text)field.placeholder).text = spec.Placeholder;
            ConfigureField(field, spec);
            face.gameObject.AddComponent<NameFieldSkin>().Bind(field, ui.Art);
            return field;
        }

        private static RectTransform CreateTextArea(RectTransform face)
        {
            var area = RuntimeUi.CreateRect(face, "TextArea", UiPlacement.Fill());
            area.offsetMin = new Vector2(TextInset, 0f);
            area.offsetMax = new Vector2(-TextInset, 0f);
            area.gameObject.AddComponent<RectMask2D>();
            return area;
        }

        private static TMP_Text CreateText(MenuUiFactory ui, RectTransform area, (string Name, TextStyle Style) text)
        {
            return ui.Label(area, new UiLabelSpec { Name = text.Name, Style = text.Style.Aligned(TextAlignmentOptions.Left), Placement = UiPlacement.Fill() });
        }

        private static void ConfigureField(TMP_InputField field, UiInputSpec spec)
        {
            field.characterLimit = spec.CharacterLimit;
            field.lineType = TMP_InputField.LineType.SingleLine;
            field.text = spec.InitialText;
            field.customCaretColor = true;
            field.caretColor = MenuColors.Ink;
            field.caretWidth = CaretWidth;
            field.caretBlinkRate = CaretBlinksPerSecond;
            field.transition = Selectable.Transition.None;
        }
    }
}
