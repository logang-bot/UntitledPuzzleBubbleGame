using UnityEngine;

namespace Game.Settings
{
    public static class GameSettings
    {
        private const string LandingAnimationStyleKey = "LandingAnimationStyle";
        private const string HandednessKey = "Handedness";

        public static LandingAnimationStyle LandingAnimationStyle
        {
            get => (LandingAnimationStyle)PlayerPrefs.GetInt(LandingAnimationStyleKey, (int)LandingAnimationStyle.OvershootBounce);
            set => Save(LandingAnimationStyleKey, (int)value);
        }

        public static Handedness Handedness
        {
            get => (Handedness)PlayerPrefs.GetInt(HandednessKey, (int)Handedness.Right);
            set => Save(HandednessKey, (int)value);
        }

        private static void Save(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
        }
    }
}
