using UnityEngine;

namespace Game.UI
{
    public struct SegmentedToggleSpec
    {
        public string Name;
        public RectInt Track;
        public int SegmentWidth;
        public string[] Labels;
    }
}
