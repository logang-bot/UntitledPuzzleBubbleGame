using System;
using System.Collections.Generic;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Pets
{
    /// <summary>
    /// The fur picker: one 26×26 swatch frame per colorway, 29 px apart,
    /// with the colorway's fill sprite in its hole at (4, 4).
    /// </summary>
    public class SwatchRowView
    {
        public event Action<int> OnPicked;

        private const int Pitch = 29;
        private const int FrameSize = 26;
        private const int FillInset = 4;
        private const int FillSize = 18;

        private readonly MenuUiFactory _ui;
        private readonly List<Image> _frames = new();

        public SwatchRowView(MenuUiFactory ui, RectTransform parent, SwatchRowSpec spec)
        {
            _ui = ui;
            for (var i = 0; i < spec.FillIds.Count; i++)
                SpawnSwatch(parent, spec, i);
        }

        public void Select(int index)
        {
            for (var i = 0; i < _frames.Count; i++)
                _frames[i].sprite = _ui.Art.Frame("swatch_frame", i == index ? 1 : 0);
        }

        private void SpawnSwatch(RectTransform parent, SwatchRowSpec spec, int index)
        {
            var frame = _ui.Sprite(parent, "swatch_frame", new RectInt(spec.Origin.x + index * Pitch, spec.Origin.y, FrameSize, FrameSize));
            frame.raycastTarget = true;
            frame.gameObject.AddComponent<Button>().onClick.AddListener(() => OnPicked?.Invoke(index));
            _ui.Sprite(frame.rectTransform, spec.FillIds[index], new RectInt(FillInset, FillInset, FillSize, FillSize));
            _frames.Add(frame);
        }
    }
}
