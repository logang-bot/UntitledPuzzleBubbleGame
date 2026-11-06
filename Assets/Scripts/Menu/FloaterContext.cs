using Game.Pets;
using Game.UI;
using UnityEngine;

namespace Game.Menu
{
    /// <summary>What every floater slot shares: art, the S/M/L draw layers and the pet catalog.</summary>
    public struct FloaterContext
    {
        public MenuUiFactory Ui;
        public RectTransform[] SizeLayers;
        public PetAppearanceCatalog Catalog;
    }
}
