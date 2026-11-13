using Game.Art;
using Game.Motion;
using Game.Shooter;
using UnityEngine;

namespace Game.Deck
{
    /// <summary>
    /// Runs the launcher mechanism (mech_gears, mech_aim, mech_fire,
    /// pointer_tap): while a rotate slab is held the back gears step at 12
    /// fps (90/n° per step, neighbours counter-rotating) and the crank or
    /// wheel turns with them; the arrow follows the aim; a fire touch pops
    /// the pointer, and every shot works the fire lever or button.
    /// </summary>
    public class MechanismAnimator : MonoBehaviour
    {
        private const float StepFps = 12f;
        private const float FireFrameFps = 12f;
        private const int FireFrameCount = 4;
        private const float IdleSeconds = 10f;

        public int Step { get; private set; }

        private MechanismParts _parts;
        private ShooterController _shooter;
        private (Sprite Up, Sprite Down) _button;
        private Vector2 _knobRest;
        private float _gearSeconds;
        private float _sinceTap = IdleSeconds;
        private float _sinceFire = IdleSeconds;
        private bool _firePressedLastFrame;

        public static MechanismAnimator Create(MechanismParts parts, ShooterController shooter, MenuArtLibrary art)
        {
            var animator = parts.PointerRing.parent.gameObject.AddComponent<MechanismAnimator>();
            animator._parts = parts;
            animator._shooter = shooter;
            animator._button = (art.Frame("fire_button_up", 0), art.Frame("fire_button_down", 0));
            if (parts.CrankKnob != null) animator._knobRest = parts.CrankKnob.anchoredPosition;
            shooter.OnFireRequested += animator.HandleFire;
            return animator;
        }

        private void OnDestroy() => _shooter.OnFireRequested -= HandleFire;

        private void HandleFire(Vector2 origin, float angleDegrees) => _sinceFire = 0f;

        private void Update()
        {
            _gearSeconds += Time.deltaTime * _shooter.RotateDirection;
            Step = Mathf.FloorToInt(_gearSeconds * StepFps);
            TurnGears(Step);
            TurnAimControl(Step);
            _parts.PointerArrow.localRotation = Quaternion.Euler(0f, 0f, -_shooter.AimAngleDegrees);
            AnimateTap();
            AnimateFire();
        }

        private void TurnGears(int step)
        {
            foreach (var (rect, gear) in _parts.Gears)
                rect.localRotation = Spin(gear.Turn * step * 90f / gear.Teeth);
        }

        private void TurnAimControl(int step)
        {
            if (_parts.Wheel != null) _parts.Wheel.localRotation = Spin(step * 15f);
            if (_parts.Crank == null) return;
            var crankStep = ((step % 8) + 8) % 8;
            _parts.Crank.localRotation = Spin(crankStep * 45f);
            var angle = crankStep * Mathf.PI / 4f;
            var orbit = new Vector2(_parts.Geometry.TurnSign * Mathf.Round(DeckLayout.CrankKnobArtPx * Mathf.Cos(angle)), -Mathf.Round(DeckLayout.CrankKnobArtPx * Mathf.Sin(angle)));
            _parts.CrankKnob.anchoredPosition = _knobRest + orbit * DeckLayout.PtPerArtPx;
        }

        private Quaternion Spin(float degreesClockwise) => Quaternion.Euler(0f, 0f, -_parts.Geometry.TurnSign * degreesClockwise);

        private void AnimateTap()
        {
            var pressed = _shooter.IsFirePressed;
            if (pressed && !_firePressedLastFrame && !_shooter.IsFireLocked) _sinceTap = 0f;
            _firePressedLastFrame = pressed;
            _sinceTap = Mathf.Min(_sinceTap + Time.deltaTime, IdleSeconds);
            var scale = GameMotions.PointerTap.Evaluate(_sinceTap).Scale.x;
            _parts.PointerRing.localScale = new Vector3(scale, scale, 1f);
        }

        private void AnimateFire()
        {
            _sinceFire = Mathf.Min(_sinceFire + Time.deltaTime, IdleSeconds);
            var frame = Mathf.Min(Mathf.FloorToInt(_sinceFire * FireFrameFps), FireFrameCount - 1);
            if (_parts.LeverArm != null)
                _parts.LeverArm.localRotation = Spin(DeckLayout.LeverRadians[DeckLayout.LeverFrames[frame]] * Mathf.Rad2Deg);
            if (_parts.FireButton != null)
                _parts.FireButton.sprite = DeckLayout.ButtonDownFrames[frame] == 1 ? _button.Down : _button.Up;
        }
    }
}
