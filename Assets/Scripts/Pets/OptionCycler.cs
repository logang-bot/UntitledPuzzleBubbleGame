using System;
using System.Collections.Generic;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Pets
{
    /// <summary>
    /// A "&lt; label &gt;" row that steps through a fixed list of options,
    /// wrapping at both ends. Placeholder until the art pass adds swatches
    /// and silhouette icons.
    /// </summary>
    public class OptionCycler
    {
        public event Action<int> OnChanged;

        public int Index { get; private set; }

        private static readonly Vector2 StepButtonSize = new Vector2(80f, 0f);

        private readonly IReadOnlyList<string> _labels;
        private readonly Text _label;

        public OptionCycler(RectTransform parent, OptionCyclerSpec spec)
        {
            _labels = spec.Labels;
            Index = CyclicIndex.Clamp(spec.InitialIndex, _labels.Count);
            var row = RuntimeUi.CreateRect(parent, spec.Name, spec.Placement);
            SpawnStepButton(row, -1);
            SpawnStepButton(row, 1);
            _label = SpawnValueLabel(row);
            RefreshLabel();
        }

        private void SpawnStepButton(RectTransform row, int delta)
        {
            var anchorX = delta < 0 ? 0f : 1f;
            var button = RuntimeUi.CreateButton(row, new UiButtonSpec
            {
                Name = delta < 0 ? "PreviousButton" : "NextButton",
                Label = delta < 0 ? "<" : ">",
                Placement = UiPlacement.At(new Vector2(anchorX, 0.5f), StepButtonSize),
                OnClick = () => StepBy(delta)
            });
            StretchVertically(button.GetComponent<RectTransform>(), anchorX);
        }

        private static void StretchVertically(RectTransform rect, float anchorX)
        {
            rect.anchorMin = new Vector2(anchorX, 0f);
            rect.anchorMax = new Vector2(anchorX, 1f);
            rect.pivot = new Vector2(anchorX, 0.5f);
        }

        private void StepBy(int delta)
        {
            Index = CyclicIndex.Step(Index, delta, _labels.Count);
            RefreshLabel();
            OnChanged?.Invoke(Index);
        }

        private static Text SpawnValueLabel(RectTransform row)
        {
            var placement = UiPlacement.Stretch(new Vector2(0f, 0f), new Vector2(1f, 1f));
            placement.SizeDelta = new Vector2(-2f * StepButtonSize.x, 0f);
            var label = RuntimeUi.CreateLabel(row, "Value", placement);
            label.color = Color.white;
            return label;
        }

        private void RefreshLabel() => _label.text = _labels.Count > 0 ? _labels[Index] : "-";
    }
}
