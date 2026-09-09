using Game.Settings;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Gameplay
{
    /// <summary>
    /// Settings screen, lives in its own scene (SettingsMenu.unity), reached
    /// from and returning to MainMenu. Built at runtime under the Canvas,
    /// same recipe as MainMenuController/LevelResultScreen.
    /// </summary>
    public class SettingsMenuController : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRect;

        private const float ButtonWidth = 280f;
        private const float ButtonHeight = 90f;

        private static readonly Color SelectedColor = new Color(0.6f, 0.85f, 1f);

        private struct StyleButtonSpec
        {
            public string Name;
            public string Label;
            public float AnchorY;
            public LandingAnimationStyle Style;
        }

        private Image _bounceButtonImage;
        private Image _squashButtonImage;

        private void Start()
        {
            SpawnBackButton();
            _bounceButtonImage = SpawnStyleButton(new StyleButtonSpec { Name = "BounceButton", Label = "Bounce", AnchorY = 0.55f, Style = LandingAnimationStyle.OvershootBounce });
            _squashButtonImage = SpawnStyleButton(new StyleButtonSpec { Name = "SquashButton", Label = "Squash", AnchorY = 0.4f, Style = LandingAnimationStyle.SquashPop });
            RefreshSelection();
        }

        private void SpawnBackButton()
        {
            var buttonObj = SpawnButton("BackButton", "Back", 0.7f);
            buttonObj.GetComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
        }

        private Image SpawnStyleButton(StyleButtonSpec spec)
        {
            var buttonObj = SpawnButton(spec.Name, spec.Label, spec.AnchorY);
            buttonObj.GetComponent<Button>().onClick.AddListener(() => HandleStyleClicked(spec.Style));
            return buttonObj.GetComponent<Image>();
        }

        private void HandleStyleClicked(LandingAnimationStyle style)
        {
            GameSettings.LandingAnimationStyle = style;
            RefreshSelection();
        }

        private void RefreshSelection()
        {
            var style = GameSettings.LandingAnimationStyle;
            _bounceButtonImage.color = style == LandingAnimationStyle.OvershootBounce ? SelectedColor : Color.white;
            _squashButtonImage.color = style == LandingAnimationStyle.SquashPop ? SelectedColor : Color.white;
        }

        private GameObject SpawnButton(string name, string label, float anchorY)
        {
            var buttonObj = new GameObject(name, typeof(RectTransform));
            ConfigureButtonRect((RectTransform)buttonObj.transform, anchorY);
            buttonObj.AddComponent<Image>().color = Color.white;
            buttonObj.AddComponent<Button>();
            SpawnButtonLabel(buttonObj.transform, label);
            return buttonObj;
        }

        private void ConfigureButtonRect(RectTransform rect, float anchorY)
        {
            rect.SetParent(canvasRect, worldPositionStays: false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, anchorY);
            rect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);
        }

        private void SpawnButtonLabel(Transform buttonTransform, string text)
        {
            var labelObj = new GameObject("Label", typeof(RectTransform));
            var rect = (RectTransform)labelObj.transform;
            rect.SetParent(buttonTransform, worldPositionStays: false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            ConfigureLegacyText(labelObj.AddComponent<Text>(), text);
        }

        private static void ConfigureLegacyText(Text text, string content)
        {
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 30;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
        }
    }
}
