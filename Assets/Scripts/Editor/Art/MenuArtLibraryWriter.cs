using Game.Art;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools.Art
{
    /// <summary>
    /// Builds MenuArtLibrary.asset from the sheet indices, the pixel fonts
    /// (forced to Hinted Raster so captions stay crisp) and the fur material.
    /// </summary>
    public static class MenuArtLibraryWriter
    {
        private const string LibraryPath = "Assets/Art/Screens/MenuArtLibrary.asset";
        private const string MaterialPath = "Assets/Art/Shaders/UIFurRecolor.mat";
        private const string CaptionFontPath = "Assets/Art/Fonts/PixelifySans-Regular.ttf";
        private const string SmallFontPath = "Assets/Art/Fonts/Silkscreen-Regular.ttf";

        public static void Write(SpriteSheetIndex[] sheets)
        {
            var library = LoadOrCreate<MenuArtLibrary>(LibraryPath, ScriptableObject.CreateInstance<MenuArtLibrary>);
            var fonts = new MenuArtFonts { Caption = LoadPixelFont(CaptionFontPath), Small = LoadPixelFont(SmallFontPath) };
            library.Configure(sheets, fonts, LoadOrCreateFurMaterial());
            EditorUtility.SetDirty(library);
        }

        private static Font LoadPixelFont(string path)
        {
            var importer = (TrueTypeFontImporter)AssetImporter.GetAtPath(path);
            if (importer.fontRenderingMode != FontRenderingMode.HintedRaster)
            {
                importer.fontRenderingMode = FontRenderingMode.HintedRaster;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Font>(path);
        }

        private static Material LoadOrCreateFurMaterial()
        {
            return LoadOrCreate(MaterialPath, () => new Material(Shader.Find("Game/UI/FurRecolor")));
        }

        private static T LoadOrCreate<T>(string path, System.Func<T> create) where T : Object
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            var created = create();
            AssetDatabase.CreateAsset(created, path);
            return created;
        }
    }
}
