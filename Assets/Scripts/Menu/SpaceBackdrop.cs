using Game.Art;
using Game.Pets;
using Game.UI;
using UnityEngine;

namespace Game.Menu
{
    /// <summary>
    /// The main menu's deep-space background, drawn back to front: Abyss
    /// fill, static deep space, near stars scrolling 4 px/s, drifting
    /// cosmos, then the floating pets.
    /// </summary>
    public static class SpaceBackdrop
    {
        private const float StarsSpeed = 4f;

        public static void Build(MenuUiFactory ui, RectTransform background, PetAppearanceCatalog catalog)
        {
            ui.Fill(background, "AbyssFill", MenuPalette.Abyss).raycastTarget = false;
            var stage = NativeStage.CreateStageAligned(background, "SpaceStage");
            ui.Sprite(stage, "space_deep", LoopingLayer.LayerRect);
            LoopingLayer.Create(ui, stage, new LoopingLayerSpec { SpriteId = "space_stars", Speed = StarsSpeed });
            CosmosSpawner.Create(ui, stage);
            FloaterSpawner.Create(ui, stage, catalog);
        }
    }
}
