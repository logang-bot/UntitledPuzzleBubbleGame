using Game.Gameplay;
using Game.Shooter;
using UnityEngine;

namespace Game.Battle
{
    /// <summary>
    /// Per-side shot timer only — battle mode has no ceiling-descent timer
    /// (see GameStateManager, whose ceiling/freeze machinery this
    /// deliberately does not reuse). Auto-fires at the current aim on
    /// expiry, same 12s default as solo mode. See
    /// docs/features/battle-mode/specs/2026-09-15-simple-attack-battle-mode-design.md.
    /// </summary>
    public class BattleShotClock : MonoBehaviour
    {
        [SerializeField] private ShooterController shooterController;
        [SerializeField] private float shotTimeSeconds = 12f;

        private ShotTimer _shotTimer;

        private void Awake() => _shotTimer = new ShotTimer(shotTimeSeconds);

        private void Start() => shooterController.OnFireRequested += HandleFireRequested;

        private void OnDestroy() => shooterController.OnFireRequested -= HandleFireRequested;

        private void Update()
        {
            if (_shotTimer.Tick(Time.deltaTime)) shooterController.Fire();
        }

        public void ResetForRematch() => _shotTimer.Reset();

        private void HandleFireRequested(Vector2 origin, float angleDegrees) => _shotTimer.Reset();
    }
}
