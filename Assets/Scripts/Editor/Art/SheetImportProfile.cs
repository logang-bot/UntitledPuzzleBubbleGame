using UnityEditor;
using UnityEngine;

namespace Game.EditorTools.Art
{
    /// <summary>
    /// How a family of Claude Design sheets is imported: the pixel pets stay
    /// point-filtered at one pixel per unit, the cartoon art is 3× vector
    /// renders that scale smoothly.
    /// </summary>
    public readonly struct SheetImportProfile
    {
        public static readonly SheetImportProfile Pixel = new(1f, FilterMode.Point, TextureImporterCompression.Uncompressed);
        public static readonly SheetImportProfile Cartoon = new(3f, FilterMode.Bilinear, TextureImporterCompression.CompressedHQ);

        public float PixelsPerUnit { get; }
        public FilterMode Filter { get; }
        public TextureImporterCompression Compression { get; }

        private SheetImportProfile(float pixelsPerUnit, FilterMode filter, TextureImporterCompression compression)
        {
            PixelsPerUnit = pixelsPerUnit;
            Filter = filter;
            Compression = compression;
        }
    }
}
