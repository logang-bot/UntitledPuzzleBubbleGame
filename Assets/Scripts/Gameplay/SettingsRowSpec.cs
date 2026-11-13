using System;

namespace Game.Gameplay
{
    /// <summary>One settings row: a label pair, the toggle's options, which one is selected and what picking one does.</summary>
    public struct SettingsRowSpec
    {
        public string Name;
        public string Label;
        public string SubLabel;
        public string[] Options;
        public int Selected;
        public Action<int> OnSelected;
    }
}
