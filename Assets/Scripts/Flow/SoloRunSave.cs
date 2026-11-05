using Game.Superpowers;
using UnityEngine;

namespace Game.Flow
{
    /// <summary>
    /// The single-player run that Continue resumes. Starting a New Game resets
    /// superpower unlocks too, so they follow the run rather than the device.
    /// See docs/features/game-flow/overview.md.
    /// </summary>
    public static class SoloRunSave
    {
        private const string HasRunKey = "SoloRun.HasRun";
        private const string CurrentLevelKey = "SoloRun.CurrentLevel";

        public static bool HasRun => PlayerPrefs.GetInt(HasRunKey, 0) == 1;

        public static int CurrentLevel
        {
            get => PlayerPrefs.GetInt(CurrentLevelKey, 1);
            set
            {
                PlayerPrefs.SetInt(CurrentLevelKey, value);
                PlayerPrefs.Save();
            }
        }

        public static void BeginNewRun()
        {
            PlayerPrefs.SetInt(HasRunKey, 1);
            CurrentLevel = 1;
            SuperpowerProgress.HighestLevelReached = 1;
        }
    }
}
