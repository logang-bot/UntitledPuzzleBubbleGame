using System;
using Game.Shooter;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>
    /// Tallies every shot attempt (manual or auto-fire-on-timeout) via
    /// ShooterController.OnFireRequested. See
    /// docs/features/core-gameplay/hud-and-level-flow.md.
    /// </summary>
    public class ShotsFiredCounter : MonoBehaviour
    {
        [SerializeField] private ShooterController shooterController;

        public event Action<int> OnShotsFiredChanged;
        public int ShotsFired { get; private set; }

        private void Start()
        {
            shooterController.OnFireRequested += HandleFireRequested;
        }

        private void OnDestroy()
        {
            shooterController.OnFireRequested -= HandleFireRequested;
        }

        public void ResetCount()
        {
            ShotsFired = 0;
            OnShotsFiredChanged?.Invoke(ShotsFired);
        }

        private void HandleFireRequested(Vector2 origin, float angleDegrees)
        {
            ShotsFired++;
            OnShotsFiredChanged?.Invoke(ShotsFired);
        }
    }
}
