using System;
using System.Collections.Generic;
using Game.Art;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// A two-or-more segment toggle on a sliced toggle_track: segments sit
    /// inset 3 px from the sides and 2 px from the top with a 2 px gap; the
    /// selected one uses toggle_seg frame 0 with an Ink caption, the others
    /// frame 1 with a Mist caption.
    /// </summary>
    public class SegmentedToggle
    {
        public event Action<int> OnSelected;
        public int Selected { get; private set; }

        private const int SideInset = 3;
        private const int TopInset = 2;
        private const int Gap = 2;
        private const int SelectedFrame = 0;
        private const int UnselectedFrame = 1;

        private readonly MenuUiFactory _ui;
        private readonly List<Image> _segments = new();
        private readonly List<Text> _captions = new();

        public SegmentedToggle(MenuUiFactory ui, RectTransform parent, SegmentedToggleSpec spec)
        {
            _ui = ui;
            var track = ui.Sprite(parent, "toggle_track", spec.Track);
            track.gameObject.name = spec.Name;
            for (var i = 0; i < spec.Labels.Length; i++)
                SpawnSegment(track.rectTransform, spec, i);
        }

        public void Select(int index)
        {
            Selected = index;
            for (var i = 0; i < _segments.Count; i++)
                ShowSegment(i, i == index);
        }

        private void SpawnSegment(RectTransform track, SegmentedToggleSpec spec, int index)
        {
            var rect = new RectInt(SideInset + index * (spec.SegmentWidth + Gap), TopInset, spec.SegmentWidth, spec.Track.height - 2 * TopInset);
            var segment = _ui.Sprite(track, "toggle_seg", rect);
            segment.raycastTarget = true;
            segment.gameObject.AddComponent<Button>().onClick.AddListener(() => HandleClicked(index));
            _segments.Add(segment);
            _captions.Add(SpawnCaption(segment.rectTransform, spec.Labels[index]));
        }

        private Text SpawnCaption(RectTransform segment, string label)
        {
            var caption = _ui.Label(segment, new UiLabelSpec { Name = "Caption", Text = label, Style = _ui.Styles.Small, Placement = UiPlacement.Fill() });
            caption.rectTransform.offsetMin = new Vector2(0f, 2f);
            return caption;
        }

        private void HandleClicked(int index)
        {
            Select(index);
            OnSelected?.Invoke(index);
        }

        private void ShowSegment(int index, bool selected)
        {
            _segments[index].sprite = _ui.Art.Frame("toggle_seg", selected ? SelectedFrame : UnselectedFrame);
            _captions[index].color = selected ? MenuPalette.Ink : MenuPalette.Mist;
        }
    }
}
