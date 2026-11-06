using System;
using Game.Art;
using Game.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Flow
{
    /// <summary>
    /// The paw-print iris between screens. Lives across scene loads on its
    /// own top-most canvas and swallows input while it plays. Created on
    /// first use from the MenuArtLibrary a screen hands it; without one
    /// (e.g. a scene opened directly in the editor) it falls back to an
    /// instant swap. See docs/features/game-flow/overview.md.
    /// </summary>
    public class SceneTransition : MonoBehaviour
    {
        private const int SortingOrder = 1000;
        private static SceneTransition _instance;

        private SpriteAnimEntry _frames;
        private Image _paw;
        private GameObject _outerFill;
        private Image _blocker;
        private Action _onCovered;
        private float _elapsed;
        private bool _closing;
        private bool _playing;

        public static void Install(MenuArtLibrary art)
        {
            if (_instance != null || art == null) return;
            _instance = new GameObject("SceneTransition").AddComponent<SceneTransition>();
            DontDestroyOnLoad(_instance.gameObject);
            _instance.Build(art);
        }

        public static void LoadScene(string sceneName) => Cover(() => SceneManager.LoadScene(sceneName));

        public static void Cover(Action onCovered)
        {
            if (_instance == null) { onCovered(); return; }
            _instance.Play(onCovered);
        }

        private void Build(MenuArtLibrary art)
        {
            _frames = art.Find("transition_paw");
            var canvas = OverlayCanvasFactory.Create("TransitionCanvas", SortingOrder);
            canvas.SetParent(transform, worldPositionStays: false);
            _blocker = RuntimeUi.CreateImage(canvas, "InputBlocker", UiPlacement.Fill());
            _blocker.color = Color.clear;
            var parts = TransitionIrisBuilder.Build(canvas);
            _paw = parts.Paw;
            _outerFill = parts.OuterFill;
            Show(false);
        }

        private void Play(Action onCovered)
        {
            if (_playing) return;
            _onCovered = onCovered;
            _elapsed = 0f;
            _closing = true;
            _playing = true;
            Show(true);
        }

        private void Update()
        {
            if (!_playing) return;
            _elapsed += Time.unscaledDeltaTime;
            ShowFrame(_closing ? TransitionTimeline.ClosingFrame(_elapsed) : TransitionTimeline.OpeningFrame(_elapsed));
            if (TransitionTimeline.IsHalfDone(_elapsed)) AdvancePhase();
        }

        private void ShowFrame(int frame)
        {
            _paw.sprite = _frames.Frames[frame];
            _outerFill.SetActive(frame >= TransitionTimeline.FrameCount - 2);
        }

        private void AdvancePhase()
        {
            _elapsed = 0f;
            if (_closing) { _closing = false; _onCovered?.Invoke(); return; }
            _playing = false;
            Show(false);
        }

        private void Show(bool visible)
        {
            _blocker.gameObject.SetActive(visible);
            _paw.gameObject.SetActive(visible);
            if (!visible) _outerFill.SetActive(false);
        }
    }
}
