using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Background bleeds edge to edge; Stage is the 270×585 native frame,
    /// centered in the safe area, that every layout coordinate refers to.
    /// </summary>
    public struct ScreenLayers
    {
        public RectTransform Background;
        public RectTransform Stage;
    }
}
