using System.Collections.Generic;
using Game.Pets;
using Game.UI;
using UnityEngine;

namespace Game.Menu
{
    /// <summary>
    /// The 15 space-pet slots of the main menu (34–56 s cycles, on screen
    /// for 78 % of each), drawn small to large through three size layers.
    /// </summary>
    public class FloaterSpawner : MonoBehaviour
    {
        private const int SlotCount = 15;
        private const int FirstSlotId = 20;

        private readonly List<FloaterSlotView> _slots = new();

        public static FloaterSpawner Create(MenuUiFactory ui, RectTransform parent, PetAppearanceCatalog catalog)
        {
            var layer = RuntimeUi.CreateRect(parent, "Floaters", UiPlacement.Fill());
            var spawner = layer.gameObject.AddComponent<FloaterSpawner>();
            var context = new FloaterContext { Ui = ui, SizeLayers = CreateSizeLayers(layer), Catalog = catalog };
            for (var i = 0; i < SlotCount; i++)
                spawner._slots.Add(new FloaterSlotView(context, SlotFor(i)));
            spawner.Update();
            return spawner;
        }

        private static RectTransform[] CreateSizeLayers(RectTransform layer)
        {
            return new[]
            {
                RuntimeUi.CreateRect(layer, "Small", UiPlacement.Fill()),
                RuntimeUi.CreateRect(layer, "Medium", UiPlacement.Fill()),
                RuntimeUi.CreateRect(layer, "Large", UiPlacement.Fill())
            };
        }

        private static SpawnSlot SlotFor(int index) => new SpawnSlot
        {
            Id = FirstSlotId + index,
            Period = 34f + index * 7 % 23,
            Offset = index * 0.37f % 1f,
            VisibleShare = 0.78f
        };

        private void Update()
        {
            var time = Time.realtimeSinceStartup;
            foreach (var slot in _slots)
                slot.Tick(time);
        }
    }
}
