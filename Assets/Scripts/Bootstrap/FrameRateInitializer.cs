using UnityEngine;

namespace Game.Bootstrap
{
    /// <summary>
    /// Pins the app to a fixed frame rate on startup. VSync is disabled
    /// project-wide (see ProjectSettings/QualitySettings.asset) so this target
    /// actually takes effect instead of being overridden by the display's sync
    /// interval, giving consistent frame pacing across Android devices.
    /// </summary>
    public class FrameRateInitializer : MonoBehaviour
    {
        [SerializeField] private int targetFrameRate = 60;

        private void Awake()
        {
            Application.targetFrameRate = targetFrameRate;
        }
    }
}
