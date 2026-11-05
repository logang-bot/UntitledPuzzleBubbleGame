using System.Collections.Generic;
using Game.UI;

namespace Game.Pets
{
    public struct OptionCyclerSpec
    {
        public string Name;
        public UiPlacement Placement;
        public IReadOnlyList<string> Labels;
        public int InitialIndex;
    }
}
