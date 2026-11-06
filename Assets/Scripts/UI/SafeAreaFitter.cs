using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Insets its RectTransform to Screen.safeArea (notches, gesture bars),
    /// re-checked whenever the safe area or resolution changes.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        private Rect _appliedArea;
        private Vector2Int _appliedScreen;

        private void Update()
        {
            var screen = new Vector2Int(Screen.width, Screen.height);
            if (Screen.safeArea == _appliedArea && screen == _appliedScreen) return;
            _appliedArea = Screen.safeArea;
            _appliedScreen = screen;
            Fit((RectTransform)transform);
        }

        private void Fit(RectTransform rect)
        {
            var area = _appliedArea;
            rect.anchorMin = new Vector2(area.xMin / _appliedScreen.x, area.yMin / _appliedScreen.y);
            rect.anchorMax = new Vector2(area.xMax / _appliedScreen.x, area.yMax / _appliedScreen.y);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
