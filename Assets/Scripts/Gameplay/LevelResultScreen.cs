using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay
{
    /// <summary>
    /// One panel handles both win and lose results — structurally identical
    /// (full-screen dim panel + message + one button), differing only in
    /// message/label/action. Built at runtime under the Canvas, using the
    /// project's first UnityEngine.UI.Button (existing input is
    /// HoldInputZone's custom pointer handlers) since a one-shot tap is
    /// standard here and the scene already has an EventSystem. See
    /// docs/features/core-gameplay/hud-and-level-flow.md.
    /// </summary>
    public class LevelResultScreen : MonoBehaviour
    {
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private RectTransform canvasRect;

        private const float ButtonWidth = 200f;
        private const float ButtonHeight = 80f;

        private GameObject _panel;
        private Text _messageText;
        private Text _buttonLabel;
        private Button _actionButton;

        private void Start()
        {
            BuildPanel();
            gameStateManager.OnLevelWon += HandleWon;
            gameStateManager.OnLevelLost += HandleLost;
        }

        private void OnDestroy()
        {
            gameStateManager.OnLevelWon -= HandleWon;
            gameStateManager.OnLevelLost -= HandleLost;
        }

        private void HandleWon() => ShowResult("Level Complete!", "Continue", gameStateManager.AdvanceToNextLevel);
        private void HandleLost() => ShowResult("Game Over", "Retry", gameStateManager.RetryLevel);

        private void ShowResult(string message, string buttonLabel, Action onAction)
        {
            _messageText.text = message;
            _buttonLabel.text = buttonLabel;
            _actionButton.onClick.RemoveAllListeners();
            _actionButton.onClick.AddListener(() => HandleActionClicked(onAction));
            _panel.SetActive(true);
        }

        private void HandleActionClicked(Action onAction)
        {
            _panel.SetActive(false);
            onAction();
        }

        private void BuildPanel()
        {
            _panel = new GameObject("LevelResultPanel", typeof(RectTransform));
            ConfigureStretchRect((RectTransform)_panel.transform, canvasRect, Vector2.zero, Vector2.one);
            _panel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
            _messageText = SpawnMessageText();
            _actionButton = SpawnActionButton(out _buttonLabel);
            _panel.SetActive(false);
        }

        private Text SpawnMessageText()
        {
            var textObj = new GameObject("ResultMessage", typeof(RectTransform));
            ConfigureStretchRect((RectTransform)textObj.transform, (RectTransform)_panel.transform, new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.75f));
            return ConfigureLegacyText(textObj.AddComponent<Text>(), fontSize: 48, color: Color.white);
        }

        private Button SpawnActionButton(out Text label)
        {
            var buttonObj = new GameObject("ResultActionButton", typeof(RectTransform));
            var rect = (RectTransform)buttonObj.transform;
            rect.SetParent(_panel.transform, worldPositionStays: false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.4f);
            rect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);
            buttonObj.AddComponent<Image>().color = Color.white;
            label = SpawnButtonLabel(buttonObj.transform);
            return buttonObj.AddComponent<Button>();
        }

        private Text SpawnButtonLabel(Transform buttonTransform)
        {
            var labelObj = new GameObject("Label", typeof(RectTransform));
            ConfigureStretchRect((RectTransform)labelObj.transform, (RectTransform)buttonTransform, Vector2.zero, Vector2.one);
            return ConfigureLegacyText(labelObj.AddComponent<Text>(), fontSize: 28, color: Color.black);
        }

        private static void ConfigureStretchRect(RectTransform rect, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            rect.SetParent(parent, worldPositionStays: false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Text ConfigureLegacyText(Text text, int fontSize, Color color)
        {
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            return text;
        }
    }
}
