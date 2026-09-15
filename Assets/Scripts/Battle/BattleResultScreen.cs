using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Battle
{
    /// <summary>
    /// Same runtime-built recipe as LevelResultScreen (full-screen dim
    /// panel + message + button), with a third draw state and Rematch
    /// instead of Continue/Retry. See
    /// docs/features/battle-mode/specs/2026-09-15-simple-attack-battle-mode-design.md.
    /// </summary>
    public class BattleResultScreen : MonoBehaviour
    {
        [SerializeField] private BattleMatchController matchController;
        [SerializeField] private RectTransform canvasRect;

        private const float ButtonWidth = 200f;
        private const float ButtonHeight = 80f;

        private GameObject _panel;
        private Text _messageText;

        private void Start()
        {
            BuildPanel();
            matchController.OnMatchEnded += HandleMatchEnded;
        }

        private void OnDestroy()
        {
            matchController.OnMatchEnded -= HandleMatchEnded;
        }

        private void HandleMatchEnded(BattleMatchResult result)
        {
            _messageText.text = MessageFor(result);
            _panel.SetActive(true);
        }

        private static string MessageFor(BattleMatchResult result) => result switch
        {
            BattleMatchResult.Player1Wins => "Player 1 Wins!",
            BattleMatchResult.Player2Wins => "Player 2 Wins!",
            _ => "Draw!",
        };

        private void HandleRematchClicked()
        {
            _panel.SetActive(false);
            matchController.Rematch();
        }

        private void HandleMenuClicked()
        {
            _panel.SetActive(false);
            SceneManager.LoadScene("MainMenu");
        }

        private void BuildPanel()
        {
            _panel = new GameObject("BattleResultPanel", typeof(RectTransform));
            ConfigureStretchRect((RectTransform)_panel.transform, canvasRect, Vector2.zero, Vector2.one);
            _panel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
            _messageText = SpawnMessageText();
            SpawnRematchButton().onClick.AddListener(HandleRematchClicked);
            SpawnMenuButton().onClick.AddListener(HandleMenuClicked);
            _panel.SetActive(false);
        }

        private Button SpawnRematchButton()
        {
            var buttonObj = new GameObject("RematchButton", typeof(RectTransform));
            var rect = (RectTransform)buttonObj.transform;
            rect.SetParent(_panel.transform, worldPositionStays: false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.4f);
            rect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);
            buttonObj.AddComponent<Image>().color = Color.white;
            SpawnButtonLabel(buttonObj.transform, "Rematch");
            return buttonObj.AddComponent<Button>();
        }

        private Button SpawnMenuButton()
        {
            var buttonObj = new GameObject("MenuButton", typeof(RectTransform));
            var rect = (RectTransform)buttonObj.transform;
            rect.SetParent(_panel.transform, worldPositionStays: false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.22f);
            rect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight * 0.75f);
            buttonObj.AddComponent<Image>().color = Color.white;
            SpawnButtonLabel(buttonObj.transform, "Menu");
            return buttonObj.AddComponent<Button>();
        }

        private Text SpawnMessageText()
        {
            var textObj = new GameObject("ResultMessage", typeof(RectTransform));
            ConfigureStretchRect((RectTransform)textObj.transform, (RectTransform)_panel.transform, new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.75f));
            return ConfigureLegacyText(textObj.AddComponent<Text>(), fontSize: 48, color: Color.white);
        }

        private static void SpawnButtonLabel(Transform buttonTransform, string text)
        {
            var labelObj = new GameObject("Label", typeof(RectTransform));
            ConfigureStretchRect((RectTransform)labelObj.transform, (RectTransform)buttonTransform, Vector2.zero, Vector2.one);
            ConfigureLegacyText(labelObj.AddComponent<Text>(), fontSize: 28, color: Color.black).text = text;
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
