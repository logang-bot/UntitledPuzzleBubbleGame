using Game.UI;
using TMPro;
using UnityEngine;

namespace Game.Pets
{
    /// <summary>The "FUR" and "SHAPE" row labels on the pet card.</summary>
    public static class PetPanelLabels
    {
        private static readonly (string Text, RectInt Rect)[] Rows =
        {
            ("FUR", new RectInt(30, 462, 60, 32)),
            ("SHAPE", new RectInt(30, 520, 60, 32))
        };

        public static void Spawn(MenuUiFactory ui, RectTransform content)
        {
            foreach (var row in Rows)
                ui.LabelIn(content, row.Rect, (row.Text, TextStyles.Label.Aligned(TextAlignmentOptions.Left)));
        }
    }
}
