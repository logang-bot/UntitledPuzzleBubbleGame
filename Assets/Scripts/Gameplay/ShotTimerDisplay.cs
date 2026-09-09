using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay
{
    /// <summary>
    /// Numeric countdown shown only in the final seconds before auto-fire,
    /// so the player isn't surprised by it. Built at runtime and anchored
    /// off the fire zone, mirroring FiredBubbleController's next-bubble
    /// indicator. See docs/features/core-gameplay/shot-timer-and-ceiling-descent.md.
    /// </summary>
    public class ShotTimerDisplay : MonoBehaviour
    {
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private RectTransform fireZoneRect;
        [SerializeField] private RectTransform rotateRightZoneRect;
        [SerializeField] private float warningThresholdSeconds = 4f;

        private const float DisplaySize = 60f;
        private const float DisplayMargin = 15f;

        private Text _countdownText;

        private void Start()
        {
            _countdownText = SpawnCountdownText();
        }

        private Text SpawnCountdownText()
        {
            var display = new GameObject("ShotTimerCountdown", typeof(RectTransform));
            var rect = (RectTransform)display.transform;
            ConfigureDisplayRect(rect);
            var text = display.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 36;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.red;
            return text;
        }

        private void ConfigureDisplayRect(RectTransform rect)
        {
            rect.SetParent(fireZoneRect.parent, worldPositionStays: false);
            rect.anchorMin = fireZoneRect.anchorMin;
            rect.anchorMax = fireZoneRect.anchorMax;
            rect.pivot = fireZoneRect.pivot;
            rect.sizeDelta = new Vector2(DisplaySize, DisplaySize);
            rect.anchoredPosition = fireZoneRect.anchoredPosition + new Vector2(RightOffset(), 0f);
        }

        // A bigger offset moves the display closer to the rotate-right zone, so
        // it's capped at whatever clears that zone, not floored by it - Constant
        // Pixel Size means a fixed margin tuned for one screen width isn't safe
        // on a narrower one once either zone's size changes.
        private float RightOffset()
        {
            var fireZoneOffset = fireZoneRect.sizeDelta.x * 0.5f + DisplaySize * 0.5f + DisplayMargin;
            var canvasHalfWidth = ((RectTransform)fireZoneRect.parent).rect.width * 0.5f;
            var rotateZoneInnerEdge = canvasHalfWidth - rotateRightZoneRect.sizeDelta.x - Mathf.Abs(rotateRightZoneRect.anchoredPosition.x);
            var maxSafeOffset = rotateZoneInnerEdge - DisplaySize * 0.5f - DisplayMargin;
            return Mathf.Min(fireZoneOffset, maxSafeOffset);
        }

        private void Update()
        {
            var remaining = gameStateManager.ShotTimeRemaining;
            var showing = remaining <= warningThresholdSeconds;
            _countdownText.gameObject.SetActive(showing);
            if (showing) _countdownText.text = Mathf.CeilToInt(remaining).ToString();
        }
    }
}
