using System;
using Game.Art;
using Game.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Flow
{
    /// <summary>
    /// The paw-print iris between screens (TransitionTimeline, PawIris). Lives
    /// across scene loads on its own top-most canvas and swallows input while
    /// it plays: it closes, runs the covered action (e.g. a scene load) once
    /// the screen is dark, holds briefly, then opens. Created on first use from
    /// the MenuArtLibrary a screen hands it; without one (e.g. a scene opened
    /// directly in the editor) it falls back to an instant swap. See
    /// docs/features/game-flow/overview.md.
    /// </summary>
    public class SceneTransition : MonoBehaviour
    {
        private const int SortingOrder = 1000;
        private const float MaxFrameStep = 1f / 30f;
        private static SceneTransition _instance;

        private enum Phase { Idle, Closing, Holding, Opening }

        private GameObject _root;
        private PawIris _iris;
        private Action _onCovered;
        private Phase _phase;
        private float _elapsed;

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
            var canvas = OverlayCanvasFactory.Create("TransitionCanvas", SortingOrder);
            canvas.SetParent(transform, worldPositionStays: false);
            RuntimeUi.CreateImage(canvas, "InputBlocker", UiPlacement.Fill()).color = Color.clear;
            _iris = new PawIris(new MenuUiFactory(art), canvas);
            _root = canvas.gameObject;
            _root.SetActive(false);
        }

        private void Play(Action onCovered)
        {
            if (_phase != Phase.Idle) return;
            _onCovered = onCovered;
            Enter(Phase.Closing);
            _root.SetActive(true);
            _iris.SetScale(TransitionTimeline.OpenScale);
        }

        private void Update()
        {
            if (_phase == Phase.Idle) return;
            _elapsed += Mathf.Min(Time.unscaledDeltaTime, MaxFrameStep);
            if (_phase == Phase.Closing) Close();
            else if (_phase == Phase.Holding) Hold();
            else Open();
        }

        private void Close()
        {
            _iris.SetScale(TransitionTimeline.ClosingScale(_elapsed));
            if (_elapsed < TransitionTimeline.CloseSeconds) return;
            Enter(Phase.Holding);
            _onCovered?.Invoke();
        }

        private void Hold()
        {
            if (_elapsed >= TransitionTimeline.HoldSeconds) Enter(Phase.Opening);
        }

        private void Open()
        {
            _iris.SetScale(TransitionTimeline.OpeningScale(_elapsed));
            if (_elapsed < TransitionTimeline.OpenSeconds) return;
            Enter(Phase.Idle);
            _root.SetActive(false);
        }

        private void Enter(Phase phase)
        {
            _phase = phase;
            _elapsed = 0f;
        }
    }
}
