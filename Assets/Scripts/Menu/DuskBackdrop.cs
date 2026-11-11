using Game.UI;
using UnityEngine;

namespace Game.Menu
{
    /// <summary>
    /// The dusk backdrop of the customization and retry screens: the banded
    /// dusk sky with its moon and hills, covering the whole screen, under the
    /// drifting star field.
    /// </summary>
    public static class DuskBackdrop
    {
        public static void Build(MenuUiFactory ui, RectTransform background)
        {
            ui.Cover(background, "bg_dusk");
            StarField.Create(ui, NativeStage.CreateStageAligned(background, "DuskStage"));
        }
    }
}
