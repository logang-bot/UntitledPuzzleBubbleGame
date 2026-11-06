using System;
using Game.UI;
using UnityEngine;

namespace Game.Results
{
    /// <summary>The primary/secondary 180×36 button pair under a result panel.</summary>
    public static class ResultButtons
    {
        private static readonly RectInt PrimaryRect = new RectInt(45, 406, 180, 36);
        private static readonly RectInt SecondaryRect = new RectInt(45, 450, 180, 36);

        public static void Spawn(MenuUiFactory ui, RectTransform stage, ResultButtonsSpec spec)
        {
            ui.Button(stage, Spec(ButtonKind.Primary, (spec.PrimaryLabel, spec.OnPrimary), PrimaryRect));
            ui.Button(stage, Spec(ButtonKind.Secondary, (spec.SecondaryLabel, spec.OnSecondary), SecondaryRect));
        }

        private static UiSkinButtonSpec Spec(ButtonKind kind, (string Label, Action OnClick) action, RectInt rect) => new UiSkinButtonSpec
        {
            Name = action.Label + "Button",
            Kind = kind,
            Label = action.Label,
            Rect = rect,
            OnClick = action.OnClick
        };
    }
}
