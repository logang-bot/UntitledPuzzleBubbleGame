using Game.Art;
using Game.Gameplay;
using Game.UI;
using UnityEngine;

namespace Game.Deck
{
    /// <summary>
    /// The Freeze superpower on screen (freeze_in, freeze_snow): while the
    /// timers are frozen an icy frame and a cool tint cover the screen,
    /// fading in over 0.25 s and out over 0.4 s, and 14 snowflakes burst from
    /// the launcher when the freeze starts.
    /// </summary>
    public class FreezeOverlayView : MonoBehaviour
    {
        private const float FadeInSeconds = 0.25f;
        private const float FadeOutSeconds = 0.4f;
        private const float FrameBleedPt = 8f;

        private GameStateManager _game;
        private CanvasGroup _group;
        private SnowBurst _snow;
        private bool _wasFrozen;
        private float _sinceChange = FadeOutSeconds;

        public static void Create(DeckUi ui, RectTransform stage, (GameStateManager Game, Vector2 Launcher) setup)
        {
            var screen = RuntimeUi.CreateRect((RectTransform)stage.parent, "Freeze", UiPlacement.Fill());
            var view = screen.gameObject.AddComponent<FreezeOverlayView>();
            ui.Factory.Fill(screen, "FreezeTint", GameColors.FreezeTint).raycastTarget = false;
            var frame = ui.Factory.Fill(screen, "FreezeFrame", Color.white);
            (frame.sprite, frame.type, frame.raycastTarget) = (ui.Art.Frame("freeze_frame", 0), UnityEngine.UI.Image.Type.Sliced, false);
            frame.rectTransform.offsetMin = -Vector2.one * FrameBleedPt;
            frame.rectTransform.offsetMax = Vector2.one * FrameBleedPt;
            (view._game, view._group) = (setup.Game, screen.gameObject.AddComponent<CanvasGroup>());
            view._group.blocksRaycasts = false;
            view._snow = new SnowBurst(ui, stage, setup.Launcher);
        }

        private void Update()
        {
            var frozen = _game.IsFrozen;
            if (frozen != _wasFrozen) StartChange(frozen);
            _sinceChange += Time.deltaTime;
            _group.alpha = frozen ? Mathf.Clamp01(_sinceChange / FadeInSeconds) : 1f - Mathf.Clamp01(_sinceChange / FadeOutSeconds);
            _snow.Advance(Time.deltaTime);
        }

        private void StartChange(bool frozen)
        {
            _wasFrozen = frozen;
            _sinceChange = 0f;
            if (frozen) _snow.Burst();
        }
    }
}
