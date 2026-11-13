using System.Collections.Generic;
using Game.Superpowers;
using Game.UI;
using UnityEngine;

namespace Game.Deck
{
    /// <summary>
    /// The four superpower buttons in the solo deck, in a fixed order
    /// (Freeze, Bomb, Row Clear, Rainbow) opposite the rotate slabs. A
    /// button is pressable while its ability is unlocked with charges left;
    /// otherwise it shows its disabled face.
    /// </summary>
    public class SuperpowerButtonsView : MonoBehaviour
    {
        public const float LeftHandX = 168f;
        public const float RightHandX = 4f;
        private const float Top = 613f;
        private const float Pitch = 48f;
        private static readonly SuperpowerId[] Order = { SuperpowerId.Freeze, SuperpowerId.Bomb, SuperpowerId.RowClear, SuperpowerId.Rainbow };

        private readonly List<SuperpowerSlot> _slots = new();
        private readonly HashSet<SuperpowerId> _unlocked = new();
        private SuperpowerController _controller;

        public static SuperpowerButtonsView Create(DeckUi ui, RectTransform layer, float x)
        {
            var view = layer.gameObject.AddComponent<SuperpowerButtonsView>();
            for (var i = 0; i < Order.Length; i++)
                view._slots.Add(SuperpowerSlot.Build(ui, layer, (Order[i], new Vector2(x + i * Pitch, Top))));
            return view;
        }

        public void Bind(SuperpowerController controller)
        {
            _controller = controller;
            controller.OnAbilitiesChanged += RefreshUnlocked;
            RefreshUnlocked();
            foreach (var slot in _slots) slot.OnPressed(() => controller.TryActivate(slot.Id));
        }

        private void RefreshUnlocked()
        {
            _unlocked.Clear();
            _unlocked.UnionWith(_controller.UnlockedAbilities);
        }

        private void Update()
        {
            if (_controller == null) return;
            foreach (var slot in _slots)
                slot.ShowCharges(_unlocked.Contains(slot.Id) ? _controller.RemainingCharges(slot.Id) : 0);
        }

        private void OnDestroy()
        {
            if (_controller != null) _controller.OnAbilitiesChanged -= RefreshUnlocked;
        }
    }
}
