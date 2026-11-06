using Game.UI;
using UnityEngine;

namespace Game.Pets
{
    /// <summary>The "FUR" and "SHAPE" row labels on the pet card.</summary>
    public static class PetPanelLabels
    {
        private static readonly (string Text, RectInt Rect)[] Rows =
        {
            ("FUR", new RectInt(14, 161, 48, 11)),
            ("SHAPE", new RectInt(14, 194, 48, 11))
        };

        public static void Spawn(MenuUiFactory ui, RectTransform panel)
        {
            foreach (var row in Rows)
                ui.Label(panel, new UiLabelSpec
                {
                    Name = row.Text,
                    Text = row.Text,
                    Style = ui.Styles.Small.WithAlignment(TextAnchor.MiddleLeft),
                    Placement = UiPlacement.TopLeft(row.Rect)
                });
        }
    }
}
