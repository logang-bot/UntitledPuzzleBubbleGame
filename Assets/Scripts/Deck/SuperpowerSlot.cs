using System;
using Game.Superpowers;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Deck
{
    /// <summary>
    /// One 44 pt superpower button: normal, pressed and disabled faces from
    /// gm_deck, and a gold charge badge with the count while any are left.
    /// </summary>
    public class SuperpowerSlot
    {
        private static readonly Vector2 Size = new(44f, 44f);

        public SuperpowerId Id { get; }

        private readonly Button _button;
        private readonly GameObject _badge;
        private readonly TMP_Text _count;

        private SuperpowerSlot(SuperpowerId id, Button button, (GameObject Badge, TMP_Text Count) charges)
        {
            Id = id;
            _button = button;
            _badge = charges.Badge;
            _count = charges.Count;
        }

        public static SuperpowerSlot Build(DeckUi ui, RectTransform layer, (SuperpowerId Id, Vector2 TopLeft) slot)
        {
            var key = "pow_" + slot.Id.ToString().ToLowerInvariant();
            var face = ui.Sprite(layer, key + "_normal", new Rect(slot.TopLeft, Size));
            var button = MakeButton(ui, face, key);
            return new SuperpowerSlot(slot.Id, button, AddBadge(ui, face, slot.TopLeft));
        }

        public void OnPressed(Action press) => _button.onClick.AddListener(() => press());

        public void ShowCharges(int remaining)
        {
            _button.interactable = remaining > 0;
            _badge.SetActive(remaining > 0);
            _count.text = remaining.ToString();
            _count.gameObject.SetActive(remaining > 0);
        }

        private static Button MakeButton(DeckUi ui, Image face, string key)
        {
            face.raycastTarget = true;
            var button = face.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState { pressedSprite = ui.Art.Frame(key + "_pressed", 0), disabledSprite = ui.Art.Frame(key + "_disabled", 0) };
            return button;
        }

        private static (GameObject Badge, TMP_Text Count) AddBadge(DeckUi ui, Image face, Vector2 topLeft)
        {
            var layer = (RectTransform)face.transform.parent;
            var badge = ui.At(layer, "charge_badge", topLeft + new Vector2(37f, 7f)).gameObject;
            var count = ui.Label(layer, new Rect(topLeft + new Vector2(27f, -3f), new Vector2(20f, 20f)), ("0", GameTextStyles.ChargeCount));
            badge.transform.SetParent(face.transform, worldPositionStays: true);
            count.transform.SetParent(face.transform, worldPositionStays: true);
            return (badge, count);
        }
    }
}
