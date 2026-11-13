using System;

namespace Game.EditorTools.Art
{
    /// <summary>
    /// JsonUtility mirror of a Claude Design legend file (Sheets/*.json).
    /// Coordinates are in pixels from the sheet's top-left corner. Some
    /// exports carry fractional pixels, so geometry is read as float and
    /// rounded by LegendGeometry.
    /// </summary>
    [Serializable]
    public class SheetLegend
    {
        public string sheet;
        public float width;
        public float height;
        public SheetLegendSprite[] sprites;
    }

    [Serializable]
    public class SheetLegendSprite
    {
        public string id;
        public float x;
        public float y;
        public float frameW;
        public float frameH;
        public int frames;
        public float stepX;
        public float fps;
        public string pivot;
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
