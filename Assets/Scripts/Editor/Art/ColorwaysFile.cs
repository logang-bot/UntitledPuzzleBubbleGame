using System;

namespace Game.EditorTools.Art
{
    /// <summary>JsonUtility mirror of Assets/Art/Screens/colorways.json.</summary>
    [Serializable]
    public class ColorwaysFile
    {
        public Colorway[] cat;
        public Colorway[] dog;
    }

    [Serializable]
    public class Colorway
    {
        public string name;
        public string[] ramp;
    }
}
