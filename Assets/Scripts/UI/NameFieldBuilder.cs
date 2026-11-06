using Game.Art;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// The pet name field: sliced name_field face, Cream typed text, Stone
    /// placeholder and a 2 px Cream caret, inset 9 px like the mockup.
    /// </summary>
    public static class NameFieldBuilder
    {
        private const int TextInset = 9;

        public static InputField Build(MenuUiFactory ui, RectTransform parent, UiInputSpec spec)
        {
            var face = ui.Sprite(parent, "name_field", spec.Rect);
            face.raycastTarget = true;
            var field = face.gameObject.AddComponent<InputField>();
            field.textComponent = CreateText(ui, face.rectTransform, MenuPalette.Cream);
            field.placeholder = CreatePlaceholder(ui, face.rectTransform, spec.Placeholder);
            ConfigureField(field, spec);
            face.gameObject.AddComponent<NameFieldSkin>().Bind(field, ui.Art.Find("name_field"));
            return field;
        }

        private static Text CreateText(MenuUiFactory ui, RectTransform face, Color color)
        {
            var text = ui.Label(face, new UiLabelSpec
            {
                Name = "Text",
                Style = ui.Styles.Small.WithColor(color).WithAlignment(TextAnchor.MiddleLeft),
                Placement = UiPlacement.Fill()
            });
            text.rectTransform.offsetMin = new Vector2(TextInset, 0f);
            text.rectTransform.offsetMax = new Vector2(-TextInset, 0f);
            text.supportRichText = false;
            return text;
        }

        private static Text CreatePlaceholder(MenuUiFactory ui, RectTransform face, string placeholder)
        {
            var text = CreateText(ui, face, MenuPalette.Stone);
            text.gameObject.name = "Placeholder";
            text.text = placeholder;
            return text;
        }

        private static void ConfigureField(InputField field, UiInputSpec spec)
        {
            field.characterLimit = spec.CharacterLimit;
            field.text = spec.InitialText;
            field.caretColor = MenuPalette.Cream;
            field.customCaretColor = true;
            field.caretWidth = 2;
            field.transition = Selectable.Transition.None;
        }
    }
}
