using System;
using Game.Motion;
using Game.UI;
using UnityEngine;

namespace Game.Results
{
    /// <summary>
    /// The primary/secondary 240×56 pt button pair under a result panel, in
    /// one group that pops in with the panel (panel_pop).
    /// </summary>
    public static class ResultButtons
    {
        private static readonly RectInt GroupRect = new RectInt(60, 560, 240, 124);
        private static readonly RectInt PrimaryRect = new RectInt(0, 0, 240, 56);
        private static readonly RectInt SecondaryRect = new RectInt(0, 68, 240, 56);

        public static MotionPlayer Spawn(MenuUiFactory ui, RectTransform stage, ResultButtonsSpec spec)
        {
            var group = RuntimeUi.CreateRect(stage, "ResultButtons", UiPlacement.TopLeft(GroupRect));
            UiPlacement.MovePivot(group, new Vector2(0.5f, 0.5f));
            ui.Button(group, Spec(ButtonKind.Primary, (spec.PrimaryLabel, spec.OnPrimary), PrimaryRect));
            ui.Button(group, Spec(ButtonKind.Secondary, (spec.SecondaryLabel, spec.OnSecondary), SecondaryRect));
            return MotionPlayer.Attach(group, MenuMotions.PanelPop);
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
