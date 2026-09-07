using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Gameplay
{
    /// <summary>
    /// Entry-point menu, lives in its own scene (MainMenu.unity) so that
    /// SampleScene/GameBoard only load once Play is pressed - no gating logic
    /// needed on GameBoard itself. Built at runtime under the Canvas, same
    /// recipe as LevelResultScreen. The second button is a non-interactive
    /// placeholder for the not-yet-designed local 2-player battle mode.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private string gameplaySceneName = "SampleScene";

        private const float ButtonWidth = 320f;
        private const float ButtonHeight = 90f;

        private static readonly Color DisabledButtonColor = new Color(1f, 1f, 1f, 0.4f);

        private struct MenuButtonSpec
        {
            public string Name;
            public string Label;
            public float AnchorY;
            public bool Interactable;
            public Action OnClick;
        }

        private void Start()
        {
            SpawnButton(new MenuButtonSpec { Name = "PlayButton", Label = "Play", AnchorY = 0.58f, Interactable = true, OnClick = HandlePlayClicked });
            SpawnButton(new MenuButtonSpec { Name = "TwoPlayerButton", Label = "2 Players (Coming Soon)", AnchorY = 0.42f, Interactable = false, OnClick = null });
        }

        private void HandlePlayClicked() => SceneManager.LoadScene(gameplaySceneName);

        private void SpawnButton(MenuButtonSpec spec)
        {
            var buttonObj = new GameObject(spec.Name, typeof(RectTransform));
            ConfigureButtonRect((RectTransform)buttonObj.transform, spec.AnchorY);
            buttonObj.AddComponent<Image>().color = spec.Interactable ? Color.white : DisabledButtonColor;
            var button = buttonObj.AddComponent<Button>();
            button.interactable = spec.Interactable;
            SpawnButtonLabel(buttonObj.transform, spec.Label, spec.Interactable);
            if (spec.OnClick != null) button.onClick.AddListener(() => spec.OnClick());
        }

        private void ConfigureButtonRect(RectTransform rect, float anchorY)
        {
            rect.SetParent(canvasRect, worldPositionStays: false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, anchorY);
            rect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);
        }

        private void SpawnButtonLabel(Transform buttonTransform, string text, bool interactable)
        {
            var labelObj = new GameObject("Label", typeof(RectTransform));
            var rect = (RectTransform)labelObj.transform;
            rect.SetParent(buttonTransform, worldPositionStays: false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            ConfigureLegacyText(labelObj.AddComponent<Text>(), text, interactable);
        }

        private static void ConfigureLegacyText(Text text, string content, bool interactable)
        {
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 30;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = interactable ? Color.black : new Color(0f, 0f, 0f, 0.5f);
        }
    }
}
