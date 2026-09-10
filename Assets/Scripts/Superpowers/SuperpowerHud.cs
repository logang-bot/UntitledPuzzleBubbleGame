using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Superpowers
{
    public class SuperpowerHud : MonoBehaviour
    {
        [SerializeField] private SuperpowerController superpowerController;
        [SerializeField] private RectTransform anchorRect;
        [SerializeField] private RectTransform fireZoneRect;

        private const float ButtonSize = 96f;
        private const float ButtonSpacing = 12f;

        private readonly Dictionary<SuperpowerId, Button> _buttons = new();
        private readonly Dictionary<SuperpowerId, Text> _chargeLabels = new();

        private void Start()
        {
            superpowerController.OnAbilitiesChanged += RebuildButtons;
            RebuildButtons();
        }

        private void RebuildButtons()
        {
            ClearButtons();
            foreach (var ability in superpowerController.UnlockedAbilities)
                SpawnButton(ability);
        }

        private void ClearButtons()
        {
            foreach (var button in _buttons.Values)
                Destroy(button.gameObject);
            _buttons.Clear();
            _chargeLabels.Clear();
        }

        private void OnDestroy()
        {
            superpowerController.OnAbilitiesChanged -= RebuildButtons;
        }

        private void Update()
        {
            foreach (var entry in _buttons)
                RefreshButton(entry.Key, entry.Value);
        }

        private void SpawnButton(SuperpowerId ability)
        {
            var buttonObj = new GameObject($"SuperpowerButton_{ability}", typeof(RectTransform));
            ConfigureButtonRect((RectTransform)buttonObj.transform, _buttons.Count);
            buttonObj.AddComponent<Image>().color = Color.white;
            var button = buttonObj.AddComponent<Button>();
            button.onClick.AddListener(() => superpowerController.TryActivate(ability));
            _buttons[ability] = button;
            _chargeLabels[ability] = SpawnChargeLabel(buttonObj.transform);
        }

        private void ConfigureButtonRect(RectTransform rect, int index)
        {
            rect.SetParent(anchorRect.parent, worldPositionStays: false);
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.sizeDelta = new Vector2(ButtonSize, ButtonSize);
            rect.anchoredPosition = new Vector2(index * (ButtonSize + ButtonSpacing), TopOffset());
        }

        private Text SpawnChargeLabel(Transform parent)
        {
            var obj = new GameObject("ChargeLabel", typeof(RectTransform));
            obj.transform.SetParent(parent, worldPositionStays: false);
            var text = obj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            return text;
        }

        private void RefreshButton(SuperpowerId id, Button button)
        {
            var remaining = superpowerController.RemainingCharges(id);
            button.interactable = remaining > 0;
            _chargeLabels[id].text = remaining.ToString();
        }

        private float TopOffset()
        {
            return fireZoneRect.anchoredPosition.y + fireZoneRect.sizeDelta.y + ButtonSpacing;
        }
    }
}
