using System;
using System.Collections.Generic;
using Game.Motion;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Pets
{
    /// <summary>
    /// The fur picker: one 40×40 pt swatch per colorway, 40 pt apart. Each is
    /// the colorway's pixel fill (5 device px per art px, centered) under a
    /// cartoon ring, gold when selected; a newly picked swatch pops (swatch_pick).
    /// </summary>
    public class SwatchRowView
    {
        public event Action<int> OnPicked;

        private const int Size = 40;
        private const int FillDevicePixels = 5;

        private readonly MenuUiFactory _ui;
        private readonly List<Image> _rings = new();
        private readonly List<MotionPlayer> _pops = new();
        private int _selected = -1;

        public SwatchRowView(MenuUiFactory ui, RectTransform parent, SwatchRowSpec spec)
        {
            _ui = ui;
            for (var i = 0; i < spec.FillIds.Count; i++)
                SpawnSwatch(parent, new RectInt(spec.Origin.x + i * Size, spec.Origin.y, Size, Size), spec.FillIds[i]);
        }

        public void Select(int index)
        {
            var changed = _selected >= 0 && index != _selected;
            _selected = index;
            for (var i = 0; i < _rings.Count; i++)
                _rings[i].sprite = _ui.Art.Frame(i == index ? "swatch_frame_selected" : "swatch_frame_normal", 0);
            if (changed) _pops[index].Play();
        }

        private void SpawnSwatch(RectTransform parent, RectInt rect, string fillId)
        {
            var index = _rings.Count;
            var holder = RuntimeUi.CreateRect(parent, "Swatch" + index, UiPlacement.TopLeft(rect));
            UiPlacement.MovePivot(holder, new Vector2(0.5f, 0.5f));
            _ui.PixelSprite(holder, fillId, new PixelPlacement(new Vector2(Size / 2f, Size / 2f), FillDevicePixels));
            var ring = _ui.Sprite(holder, "swatch_frame_normal", new RectInt(0, 0, Size, Size));
            ring.raycastTarget = true;
            ring.gameObject.AddComponent<Button>().onClick.AddListener(() => OnPicked?.Invoke(index));
            _rings.Add(ring);
            _pops.Add(MotionPlayer.Attach(holder, MenuMotions.SwatchPick));
        }
    }
}
