using System;

namespace Game.EditorTools.Art
{
    /// <summary>
    /// JsonUtility mirror of a Claude Design legend file (Sheets/*.json).
    /// Coordinates are in pixels from the sheet's top-left corner.
    /// </summary>
    [Serializable]
    public class SheetLegend
    {
        public string sheet;
        public int width;
        public int height;
        public SheetLegendSprite[] sprites;
    }

    [Serializable]
    public class SheetLegendSprite
    {
        public string id;
        public int x;
        public int y;
        public int frameW;
        public int frameH;
        public int frames;
        public int stepX;
        public float fps;
        public float pivotX;
        public float pivotY;
        public SheetLegendBorder border;
    }

    [Serializable]
    public class SheetLegendBorder
    {
        public int l;
        public int t;
        public int r;
        public int b;
    }
}
