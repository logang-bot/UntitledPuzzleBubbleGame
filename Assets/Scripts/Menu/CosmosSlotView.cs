using Game.Motion;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Menu
{
    /// <summary>
    /// One drifting cosmos slot. Each cycle picks a sprite and a straight
    /// crossing (the comet always sideways, mirrored so its tail trails);
    /// galaxies spin slowly over a pulsing glow while they glide across.
    /// </summary>
    public class CosmosSlotView
    {
        private const float EdgeMargin = 10f;

        private readonly MenuUiFactory _ui;
        private readonly CosmosSlotSpec _spec;
        private readonly RectTransform _root;
        private readonly Image _glow;
        private readonly Image _body;
        private CrossingPath _path;
        private float _glowScale;
        private bool _isGalaxy;
        private int _cycle = int.MinValue;

        public CosmosSlotView(MenuUiFactory ui, RectTransform parent, CosmosSlotSpec spec)
        {
            _ui = ui;
            _spec = spec;
            _root = RuntimeUi.CreateRect(parent, "CosmosSlot" + spec.Slot.Id, UiPlacement.CenteredAt(Vector2.zero, Vector2.zero));
            _glow = ui.PivotSprite(_root, CosmosSprites.GalaxyGlow, Vector2.zero);
            _body = ui.PivotSprite(_root, spec.Pool[0], Vector2.zero);
        }

        public void Tick(float time)
        {
            var visible = _spec.Slot.TryGetState(time, out var state);
            _root.gameObject.SetActive(visible);
            if (!visible) return;
            if (state.Cycle != _cycle) Repick(state.Cycle);
            _root.anchoredPosition = UiPlacement.SmoothPosition(_path.At(state.Progress));
            if (_isGalaxy) Animate(time);
        }

        private void Repick(int cycle)
        {
            _cycle = cycle;
            var random = _spec.Slot.RandomFor(cycle);
            var id = _spec.SpriteFor(cycle, random);
            ShowSprite(id);
            _path = PickPath(random, id);
            _body.rectTransform.localScale = new Vector3(id == CosmosSprites.Comet && _path.MovesRight ? -1f : 1f, 1f, 1f);
        }

        private void ShowSprite(string id)
        {
            _body.sprite = _ui.Art.Frame(id, 0);
            _body.rectTransform.sizeDelta = _body.sprite.rect.size / _body.sprite.pixelsPerUnit;
            _body.rectTransform.pivot = _body.sprite.pivot / _body.sprite.rect.size;
            _body.rectTransform.localEulerAngles = Vector3.zero;
            _isGalaxy = CosmosSprites.IsGalaxy(id);
            _glow.enabled = _isGalaxy;
            _glowScale = _body.rectTransform.sizeDelta.x * CosmosSprites.GlowScalePerPt;
        }

        private CrossingPath PickPath(System.Random random, string id)
        {
            var half = _body.rectTransform.sizeDelta / 2f + Vector2.one * EdgeMargin;
            var radius = Vector2.one * Mathf.Max(half.x, half.y);
            if (id != CosmosSprites.Comet) return CrossingPath.Pick(random, radius, new CrossingRoute(CrossingPath.RandomEdge(random), _spec.Band));
            return CrossingPath.Pick(random, radius, new CrossingRoute(CrossingPath.RandomHorizontalEdge(random), _spec.Band)).Leveled();
        }

        private void Animate(float time)
        {
            _body.rectTransform.localEulerAngles = new Vector3(0f, 0f, -MenuMotions.GalaxySpin.Evaluate(time).Rotation);
            var glow = MenuMotions.GalaxyGlow.Evaluate(time);
            _glow.rectTransform.localScale = new Vector3(glow.Scale.x * _glowScale, glow.Scale.y * _glowScale * CosmosSprites.GlowFlattening, 1f);
            _glow.color = new Color(1f, 1f, 1f, glow.Alpha);
        }
    }
}
