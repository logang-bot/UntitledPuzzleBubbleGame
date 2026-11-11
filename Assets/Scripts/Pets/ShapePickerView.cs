using System;
using System.Collections.Generic;
using Game.Motion;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Pets
{
    /// <summary>
    /// The shape picker: ‹ arrow, one 40×46 pt slot per shape (44 pt apart)
    /// holding the shape's silhouette, › arrow. Slots pick directly and pop
    /// when newly selected; arrows step with wraparound.
    /// </summary>
    public class ShapePickerView
    {
        public event Action<int> OnPicked;

        private const int SlotOffset = 34;
        private const int SlotPitch = 44;
        private const int RightArrowOffset = 210;
        private static readonly Vector2Int SlotSize = new Vector2Int(40, 46);
        private static readonly Vector2Int ArrowSize = new Vector2Int(30, 44);
        private static readonly RectInt SilhouetteRect = new RectInt(4, 7, 32, 32);

        private readonly MenuUiFactory _ui;
        private readonly List<Image> _slots = new();
        private readonly List<MotionPlayer> _pops = new();
        private int _selected = -1;

        public ShapePickerView(MenuUiFactory ui, RectTransform parent, ShapePickerSpec spec)
        {
            _ui = ui;
            SpawnArrow(parent, ButtonKind.ArrowLeft, spec.Origin);
            for (var i = 0; i < spec.SilhouetteIds.Count; i++)
                SpawnSlot(parent, new Vector2Int(spec.Origin.x + SlotOffset + i * SlotPitch, spec.Origin.y), spec.SilhouetteIds[i]);
            SpawnArrow(parent, ButtonKind.ArrowRight, spec.Origin + new Vector2Int(RightArrowOffset, 0));
        }

        public void Select(int index)
        {
            var changed = _selected >= 0 && index != _selected;
            _selected = index;
            for (var i = 0; i < _slots.Count; i++)
                _slots[i].sprite = _ui.Art.Frame(i == index ? "shape_slot_selected" : "shape_slot_normal", 0);
            if (changed) _pops[index].Play();
        }

        private void SpawnArrow(RectTransform parent, ButtonKind kind, Vector2Int position)
        {
            var step = kind == ButtonKind.ArrowLeft ? -1 : 1;
            _ui.Button(parent, new UiSkinButtonSpec
            {
                Name = kind.ToString(),
                Kind = kind,
                Rect = new RectInt(position.x, position.y + 1, ArrowSize.x, ArrowSize.y),
                OnClick = () => OnPicked?.Invoke(CyclicIndex.Step(Math.Max(_selected, 0), step, _slots.Count))
            });
        }

        private void SpawnSlot(RectTransform parent, Vector2Int position, string silhouetteId)
        {
            var index = _slots.Count;
            var slot = _ui.Sprite(parent, "shape_slot_normal", new RectInt(position.x, position.y, SlotSize.x, SlotSize.y));
            UiPlacement.MovePivot(slot.rectTransform, new Vector2(0.5f, 0.5f));
            slot.raycastTarget = true;
            slot.gameObject.AddComponent<Button>().onClick.AddListener(() => OnPicked?.Invoke(index));
            _ui.Sprite(slot.rectTransform, silhouetteId, SilhouetteRect);
            _slots.Add(slot);
            _pops.Add(MotionPlayer.Attach(slot.rectTransform, MenuMotions.SwatchPick));
        }
    }
}
