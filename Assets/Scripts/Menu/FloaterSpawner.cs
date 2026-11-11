using System.Collections.Generic;
using Game.Pets;
using Game.UI;
using UnityEngine;

namespace Game.Menu
{
    /// <summary>
    /// The main menu's floating pets: FloaterSpec.SlotCount slots, each on its
    /// own pre-warmed FloaterSchedule, drawn small to large through three size
    /// layers so the nearer, faster pets pass in front.
    /// </summary>
    public class FloaterSpawner : MonoBehaviour
    {
        private const int FirstSlotId = 20;

        private readonly List<FloaterSlotView> _slots = new();

        public static FloaterSpawner Create(MenuUiFactory ui, RectTransform parent, PetAppearanceCatalog catalog)
        {
            var layer = RuntimeUi.CreateRect(parent, "Floaters", UiPlacement.Fill());
            var spawner = layer.gameObject.AddComponent<FloaterSpawner>();
            var context = new FloaterContext { Ui = ui, SizeLayers = CreateSizeLayers(layer), Catalog = catalog };
            var now = Time.realtimeSinceStartup;
            for (var i = 0; i < FloaterSpec.SlotCount; i++)
                spawner._slots.Add(new FloaterSlotView(context, new FloaterSchedule(FirstSlotId + i, now)));
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

        private void Update()
        {
            var time = Time.realtimeSinceStartup;
            foreach (var slot in _slots)
                slot.Tick(time);
        }
    }
}
