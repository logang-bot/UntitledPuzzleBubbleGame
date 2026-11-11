using Game.Pets;
using Game.UI;
using UnityEngine;

namespace Game.Menu
{
    /// <summary>
    /// The main menu's deep-space background, back to front: the static deep
    /// space covering the whole screen, the drifting star field, the cosmos,
    /// then the floating pets.
    /// </summary>
    public static class SpaceBackdrop
    {
        public static void Build(MenuUiFactory ui, RectTransform background, PetAppearanceCatalog catalog)
        {
            ui.Cover(background, "space_deep");
            var stage = NativeStage.CreateStageAligned(background, "SpaceStage");
            StarField.Create(ui, stage);
            CosmosSpawner.Create(ui, stage);
            FloaterSpawner.Create(ui, stage, catalog);
        }
    }
}
