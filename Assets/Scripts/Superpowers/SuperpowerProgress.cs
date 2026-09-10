using UnityEngine;

namespace Game.Superpowers
{
    public static class SuperpowerProgress
    {
        private const string HighestLevelReachedKey = "HighestLevelReached";

        public static int HighestLevelReached
        {
            get => PlayerPrefs.GetInt(HighestLevelReachedKey, 1);
            set
            {
                PlayerPrefs.SetInt(HighestLevelReachedKey, value);
                PlayerPrefs.Save();
            }
        }
    }
}
