using Game.Shooter;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>
    /// Owns the shot timer: auto-fires at the current aim angle on expiry,
    /// and resets only in response to ShooterController.OnFireRequested so
    /// manual and auto fire share one reset path. See
    /// docs/features/core-gameplay/shot-timer-and-ceiling-descent.md.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        [SerializeField] private ShooterController shooterController;
        [SerializeField] private float shotTimeSeconds = 8f;

        public float ShotTimeRemaining => _shotTimer.TimeRemaining;

        private ShotTimer _shotTimer;

        private void Awake()
        {
            _shotTimer = new ShotTimer(shotTimeSeconds);
        }

        private void Start()
        {
            shooterController.OnFireRequested += HandleFireRequested;
        }

        private void OnDestroy()
        {
            shooterController.OnFireRequested -= HandleFireRequested;
        }

        private void Update()
        {
            if (_shotTimer.Tick(Time.deltaTime)) shooterController.Fire();
        }

        private void HandleFireRequested(Vector2 origin, float angleDegrees)
        {
            _shotTimer.Reset();
        }
    }
}
