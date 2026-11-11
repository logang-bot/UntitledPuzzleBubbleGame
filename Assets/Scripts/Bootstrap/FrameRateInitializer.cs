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
        private const int DefaultFrameRate = 60;

        [SerializeField] private int targetFrameRate = DefaultFrameRate;

        private void Awake()
        {
            Application.targetFrameRate = targetFrameRate;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplyDefaultBeforeFirstScene()
        {
            Application.targetFrameRate = DefaultFrameRate;
        }
    }
}
