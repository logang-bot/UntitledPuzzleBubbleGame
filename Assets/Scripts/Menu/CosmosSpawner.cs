using System.Collections.Generic;
using Game.UI;
using UnityEngine;

namespace Game.Menu
{
    /// <summary>
    /// Drives the main menu's drifting cosmos (CosmosSprites.Slots) and the
    /// shooting star. Time is realtime since startup, so the sky carries on
    /// between menu visits.
    /// </summary>
    public class CosmosSpawner : MonoBehaviour
    {
        private readonly List<CosmosSlotView> _slots = new();
        private ShootingStarView _shootingStar;

        public static CosmosSpawner Create(MenuUiFactory ui, RectTransform parent)
        {
            var layer = RuntimeUi.CreateRect(parent, "Cosmos", UiPlacement.Fill());
            var spawner = layer.gameObject.AddComponent<CosmosSpawner>();
            foreach (var spec in CosmosSprites.Slots)
                spawner._slots.Add(new CosmosSlotView(ui, layer, spec));
            spawner._shootingStar = new ShootingStarView(ui, layer);
            spawner.Update();
            return spawner;
        }

        private void Update()
        {
            var time = Time.realtimeSinceStartup;
            foreach (var slot in _slots)
                slot.Tick(time);
            _shootingStar.Tick(time);
        }
    }
}
