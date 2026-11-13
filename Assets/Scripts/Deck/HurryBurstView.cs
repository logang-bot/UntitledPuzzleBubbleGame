using Game.Art;
using Game.Gameplay;
using Game.Motion;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Deck
{
    /// <summary>
    /// The last-seconds countdown (countdown_pop): a shout bubble with the
    /// digit 4 → 1, re-popping each second and swelling with urgency, with a
    /// wobble on 1. Its two spike frames alternate at 6 fps while it bobs.
    /// </summary>
    public class HurryBurstView : MonoBehaviour
    {
        private const float BobPt = 1.67f;
        private const float SpikeFps = 6f;
        private const float RampSeconds = 0.18f;

        private IShotClock _clock;
        private SpriteAnimEntry _spikes;
        private Image _burst;
        private TMP_Text _digit;
        private CanvasGroup _group;
        private Vector2 _rest;
        private int _shownDigit;
        private float _popStart;

        public static void Create(DeckUi ui, RectTransform layer, (Vector2 Point, IShotClock Clock) setup)
        {
            var root = RuntimeUi.CreateRect(layer, "HurryBurst", UiPlacement.CenteredAt(setup.Point, Vector2.zero));
            var view = root.gameObject.AddComponent<HurryBurstView>();
            view._clock = setup.Clock;
            view._spikes = ui.Art.Find("hurry_burst");
            view._burst = ui.At(root, "hurry_burst", Vector2.zero);
            view._digit = ui.Label(root, new Rect(-40f, -26f, 80f, 50f), ("4", GameTextStyles.CountdownDigit));
            view._group = root.gameObject.AddComponent<CanvasGroup>();
            view._rest = root.anchoredPosition;
        }

        private void Update()
        {
            var remaining = _clock.ShotTimeRemaining;
            _group.alpha = 0f;
            if (!ShotClockDisplay.IsHurrying(remaining)) { _shownDigit = 0; return; }
            ShowDigit(ShotClockDisplay.Digit(remaining));
            Animate(Time.time - _popStart);
        }

        private void ShowDigit(int digit)
        {
            if (digit == _shownDigit) return;
            _shownDigit = digit;
            _digit.text = digit.ToString();
            _popStart = Time.time;
        }

        private void Animate(float sincePop)
        {
            var pose = GameMotions.CountdownPop.Evaluate(sincePop);
            var urgency = ShotClockDisplay.Urgency(_shownDigit);
            var ramp = sincePop < RampSeconds ? 1f + (urgency - 1f) * sincePop / RampSeconds : urgency;
            transform.localScale = Vector3.one * (pose.Scale.x * ramp);
            transform.localRotation = Quaternion.Euler(0f, 0f, _shownDigit == 1 ? -GameMotions.CountdownWobble.Evaluate(sincePop) : 0f);
            _group.alpha = pose.Alpha;
            ((RectTransform)transform).anchoredPosition = _rest + new Vector2(0f, -Mathf.Round(Mathf.Sin(Time.time * Mathf.PI * 3f)) * BobPt);
            _burst.sprite = _spikes.Frames[Mathf.FloorToInt(Time.time * SpikeFps) % _spikes.Frames.Length];
        }
    }
}
