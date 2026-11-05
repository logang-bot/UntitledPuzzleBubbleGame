using System;

namespace Game.UI
{
    public struct UiButtonSpec
    {
        public string Name;
        public string Label;
        public UiPlacement Placement;
        public Action OnClick;
    }
}
