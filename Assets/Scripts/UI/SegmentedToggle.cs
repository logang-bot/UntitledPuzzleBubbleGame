using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// A segmented toggle on a sliced toggle_track: segments inset 4 pt, each
    /// (width − 8) / n wide. The selected one uses seg_on with its caption
    /// raised 3 pt above the lip, the others seg_off with the dimmed caption.
    /// </summary>
    public class SegmentedToggle
    {
        public event Action<int> OnSelected;
        public int Selected { get; private set; }

        private const int Inset = 4;
        private const float SelectedCaptionRaise = 3f;

        private readonly MenuUiFactory _ui;
        private readonly List<Image> _segments = new();
        private readonly List<TMP_Text> _captions = new();

        public SegmentedToggle(MenuUiFactory ui, RectTransform parent, SegmentedToggleSpec spec)
        {
            _ui = ui;
            var track = ui.Sprite(parent, "toggle_track", spec.Track);
            track.gameObject.name = spec.Name;
            var width = (spec.Track.width - 2 * Inset) / spec.Labels.Length;
            for (var i = 0; i < spec.Labels.Length; i++)
                SpawnSegment(track.rectTransform, new RectInt(Inset + i * width, Inset, width, spec.Track.height - 2 * Inset), spec.Labels[i]);
        }

        public void Select(int index)
        {
            Selected = index;
            for (var i = 0; i < _segments.Count; i++)
                ShowSegment(i, i == index);
        }

        private void SpawnSegment(RectTransform track, RectInt rect, string label)
        {
            var index = _segments.Count;
            var segment = _ui.Sprite(track, "seg_off", rect);
            segment.raycastTarget = true;
            segment.gameObject.AddComponent<Button>().onClick.AddListener(() => HandleClicked(index));
            _segments.Add(segment);
            _captions.Add(_ui.Label(segment.rectTransform, new UiLabelSpec { Name = "Caption", Text = label, Style = TextStyles.SegmentOff, Placement = UiPlacement.Fill() }));
        }

        private void HandleClicked(int index)
        {
            Select(index);
            OnSelected?.Invoke(index);
        }

        private void ShowSegment(int index, bool selected)
        {
            _segments[index].sprite = _ui.Art.Frame(selected ? "seg_on" : "seg_off", 0);
            TextStyleApplier.Apply(_captions[index], selected ? TextStyles.SegmentOn : TextStyles.SegmentOff, _ui.Fonts);
            _captions[index].rectTransform.offsetMin = new Vector2(0f, selected ? SelectedCaptionRaise : 0f);
        }
    }
}
