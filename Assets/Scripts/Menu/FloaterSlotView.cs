using Game.Art;
using Game.Motion;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Menu
{
    /// <summary>
    /// One floating pet on the main menu, following its FloaterSchedule: the
    /// pixel floater recolored with the flight's colorway, turned in 90° steps
    /// and mirrored, kept on whole screen pixels (PixelSnap), bobbing ±3 pt,
    /// optionally inside a wobbling cartoon bubble.
    /// </summary>
    public class FloaterSlotView
    {
        private readonly FloaterContext _context;
        private readonly FloaterSchedule _schedule;
        private readonly RectTransform _root;
        private readonly Image _pet;
        private readonly PixelSnap _snap;
        private readonly Image _bubble;
        private readonly Material _fur;
        private FloaterFlight _flight;
        private SpriteAnimEntry _entry;
        private int _shownFlight = -1;

        public FloaterSlotView(FloaterContext context, FloaterSchedule schedule)
        {
            _context = context;
            _schedule = schedule;
            _root = RuntimeUi.CreateRect(context.SizeLayers[0], "Floater", UiPlacement.CenteredAt(Vector2.zero, Vector2.zero));
            _pet = RuntimeUi.CreateImage(_root, "Pet", UiPlacement.At(new Vector2(0.5f, 0.5f), Vector2.zero));
            _pet.raycastTarget = false;
            _fur = FurRecolor.CreateInstance(context.Ui.Art.FurRecolorMaterial);
            _pet.material = _fur;
            _snap = PixelSnap.Attach(_pet.rectTransform, Vector2.zero, FloaterSpec.DesignDevicePixels);
            _bubble = context.Ui.PivotSprite(_root, "bubble", Vector2.zero);
        }

        public void Tick(float time)
        {
            var flying = _schedule.TryGetFlight(time, out var flight, out var progress);
            _root.gameObject.SetActive(flying);
            if (!flying) return;
            if (flight.Number != _shownFlight) Show(flight);
            _root.anchoredPosition = UiPlacement.SmoothPosition(flight.Path.At(progress) + Bob(time));
            _pet.sprite = _entry.Frames[FrameClock.LoopingFrame(time + _flight.Pick.Phase * 3f, _entry.Fps, _entry.Frames.Length)];
            if (_flight.Pick.InBubble) Wobble(time);
        }

        private Vector2 Bob(float time)
        {
            return new Vector2(0f, FloaterSpec.BobAmplitude * Mathf.Sin(2f * Mathf.PI * (time / FloaterSpec.BobPeriod + _flight.Pick.Phase)));
        }

        private void Show(FloaterFlight flight)
        {
            _flight = flight;
            _shownFlight = flight.Number;
            var appearance = _context.Catalog.For(flight.Pick.Species);
            _entry = _context.Ui.Art.Find(flight.Pick.SpriteId(appearance.Shapes[flight.Pick.ShapeIndex].ArtId));
            FurRecolor.Apply(_fur, flight.Pick.Species, appearance.Colors[flight.Pick.ColorwayIndex % appearance.Colors.Count].ToRamp());
            _root.SetParent(_context.SizeLayers[(int)flight.Pick.Size], worldPositionStays: false);
            ShowPet(flight.Pick);
            _bubble.gameObject.SetActive(flight.Pick.InBubble);
        }

        private void ShowPet(FloaterPick pick)
        {
            _snap.SetArtSize(_entry.First.rect.size);
            _pet.rectTransform.localEulerAngles = new Vector3(0f, 0f, -90f * pick.QuarterTurns);
            _pet.rectTransform.localScale = new Vector3(pick.Mirror ? -1f : 1f, 1f, 1f);
        }

        private void Wobble(float time)
        {
            var pose = MenuMotions.BubbleWobble.Evaluate(time + _shownFlight);
            var scale = FloaterSpec.BubbleRadius(_flight.Pick.Size) / FloaterSpec.BubbleSpriteRadius;
            _bubble.rectTransform.localScale = new Vector3(pose.Scale.x * scale, pose.Scale.y * scale, 1f);
        }
    }
}
