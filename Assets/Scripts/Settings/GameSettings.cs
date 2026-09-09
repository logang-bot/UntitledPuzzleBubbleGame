using UnityEngine;

namespace Game.Settings
{
    public static class GameSettings
    {
        private const string LandingAnimationStyleKey = "LandingAnimationStyle";

        public static LandingAnimationStyle LandingAnimationStyle
        {
            get => (LandingAnimationStyle)PlayerPrefs.GetInt(LandingAnimationStyleKey, (int)LandingAnimationStyle.OvershootBounce);
            set
            {
                PlayerPrefs.SetInt(LandingAnimationStyleKey, (int)value);
                PlayerPrefs.Save();
            }
        }
    }
}
