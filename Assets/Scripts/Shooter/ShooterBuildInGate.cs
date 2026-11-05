using Game.Grid;
using UnityEngine;

namespace Game.Shooter
{
    /// <summary>
    /// Keeps the shooter from firing while a freshly loaded level is still
    /// playing its build-in animation, so shots can't land among bubbles that
    /// are still scaling in. Polls rather than subscribing so the very first
    /// level's build-in (started in GridDebugRenderer.Start) is never missed
    /// regardless of script execution order. See
    /// docs/features/level-content/specs/2026-09-11-curated-levels-and-transitions-design.md.
    /// </summary>
    public class ShooterBuildInGate : MonoBehaviour
    {
        [SerializeField] private GridDebugRenderer gridRenderer;
        [SerializeField] private ShooterController shooterController;

        private void Update()
        {
            shooterController.IsFireLocked = gridRenderer.IsBuildingIn;
        }
    }
}
