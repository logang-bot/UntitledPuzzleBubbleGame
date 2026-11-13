using Game.Motion;
using Game.Shooter;
using UnityEngine;

namespace Game.Deck
{
    /// <summary>
    /// The bubble loaded at the pointer and the next one in its ring. After
    /// each shot the next bubble rides the feed tube up to the pointer
    /// (next_feed, 0.32 s outCubic) and a new next bubble pops into the ring
    /// 0.12 s later (next_pop).
    /// </summary>
    public class LoadedBubbleView : MonoBehaviour
    {
        private const float FeedSeconds = 0.32f;
        private const float NextPopDelay = 0.17f;

        private FiredBubbleController _shots;
        private UiBubble _loaded;
        private UiBubble _next;
        private UiBubble _feeding;
        private (Vector2 From, Vector2 To) _feedPath;
        private float _sinceShot = FeedSeconds + 1f;

        public static void Create(DeckUi ui, MechanismParts parts, FiredBubbleController shots)
        {
            var view = parts.BubblesLayer.gameObject.AddComponent<LoadedBubbleView>();
            view._shots = shots;
            view._loaded = new UiBubble(ui, parts.BubblesLayer, parts.Geometry.Point(DeckLayout.Pointer));
            view._next = new UiBubble(ui, parts.BubblesLayer, parts.Geometry.Point(DeckLayout.NextRing));
            view._feeding = new UiBubble(ui, parts.BubblesLayer, parts.Geometry.Point(DeckLayout.NextRing));
            view._feedPath = (view._next.Root.anchoredPosition, view._loaded.Root.anchoredPosition);
            shots.OnQueueAdvanced += view.StartFeed;
        }

        private void OnDestroy() => _shots.OnQueueAdvanced -= StartFeed;

        private void StartFeed() => _sinceShot = 0f;

        private void Update()
        {
            _sinceShot = Mathf.Min(_sinceShot + Time.deltaTime, FeedSeconds + 1f);
            var feeding = _sinceShot < FeedSeconds;
            _loaded.Root.gameObject.SetActive(!feeding);
            _feeding.Root.gameObject.SetActive(feeding);
            ShowLoaded();
            _feeding.Show(_shots.LoadedColor);
            _feeding.Root.anchoredPosition = Vector2.Lerp(_feedPath.From, _feedPath.To, Ease.Apply(EaseKind.OutCubic, _sinceShot / FeedSeconds));
            ShowNext();
        }

        private void ShowLoaded()
        {
            if (_shots.ArmedAbility.HasValue) _loaded.ShowSuperpower(_shots.ArmedAbility.Value);
            else _loaded.Show(_shots.LoadedColor);
        }

        private void ShowNext()
        {
            _next.Show(_shots.QueuedColor);
            var scale = GameMotions.NextPop.Evaluate(_sinceShot - NextPopDelay).Scale.x;
            _next.Root.localScale = Vector3.one * (_sinceShot < NextPopDelay ? 0f : scale) * UiBubble.CellScale;
        }
    }
}
