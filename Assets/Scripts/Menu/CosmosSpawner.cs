using System.Collections.Generic;
using Game.UI;
using UnityEngine;

namespace Game.Menu
{
    /// <summary>
    /// Drives the main menu's drifting cosmos: one big hero-galaxy slot
    /// (240 s), three small slots (130–170 s) and the shooting star. Time is
    /// realtime since startup, so the sky carries on between menu visits.
    /// </summary>
    public class CosmosSpawner : MonoBehaviour
    {
        private static readonly CosmosSlotSpec[] Slots =
        {
            new CosmosSlotSpec { Slot = new SpawnSlot { Id = 0, Period = 240f, Offset = 0.42f, VisibleShare = 0.94f }, Big = true },
            new CosmosSlotSpec { Slot = new SpawnSlot { Id = 1, Period = 150f, Offset = 0.12f, VisibleShare = 0.7f } },
            new CosmosSlotSpec { Slot = new SpawnSlot { Id = 2, Period = 170f, Offset = 0.6f, VisibleShare = 0.7f } },
            new CosmosSlotSpec { Slot = new SpawnSlot { Id = 3, Period = 130f, Offset = 0.86f, VisibleShare = 0.7f } }
        };

        private readonly List<CosmosSlotView> _slots = new();
        private ShootingStarView _shootingStar;

        public static CosmosSpawner Create(MenuUiFactory ui, RectTransform parent)
        {
            var layer = RuntimeUi.CreateRect(parent, "Cosmos", UiPlacement.Fill());
            var spawner = layer.gameObject.AddComponent<CosmosSpawner>();
            foreach (var spec in Slots)
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
