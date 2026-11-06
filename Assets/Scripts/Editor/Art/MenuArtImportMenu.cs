using System.IO;
using System.Linq;
using Game.Art;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools.Art
{
    /// <summary>
    /// Tools > Art > Import Menu Sheets: slices every Claude Design sheet in
    /// Assets/Art/Screens/Sheets from its legend JSON, writes the sheet
    /// indices and the MenuArtLibrary, and copies the colorways into the pet
    /// catalog. Safe to re-run after every re-export.
    /// </summary>
    public static class MenuArtImportMenu
    {
        private const string SheetsFolder = "Assets/Art/Screens/Sheets";
        private const string IndexFolder = "Assets/Art/Screens/Index";
        private const string ColorwaysPath = "Assets/Art/Screens/colorways.json";

        [MenuItem("Tools/Art/Import Menu Sheets")]
        public static void ImportAll()
        {
            EnsureFolder(IndexFolder);
            var sheets = Directory.GetFiles(SheetsFolder, "*.json").Select(ImportSheet).ToArray();
            MenuArtLibraryWriter.Write(sheets);
            PetCatalogArtWriter.Write(ReadJson<ColorwaysFile>(ColorwaysPath));
            AssetDatabase.SaveAssets();
            Debug.Log($"Imported {sheets.Length} menu sheets, {sheets.Sum(s => s.Entries.Count)} sprite ids.");
        }

        private static SpriteSheetIndex ImportSheet(string legendPath)
        {
            var legend = ReadJson<SheetLegend>(legendPath);
            var texturePath = Path.ChangeExtension(legendPath, ".png").Replace(Path.DirectorySeparatorChar, '/');
            SheetSlicer.Slice(texturePath, legend);
            return SheetIndexWriter.Write(texturePath, legend, IndexFolder);
        }

        private static T ReadJson<T>(string path) => JsonUtility.FromJson<T>(File.ReadAllText(path));

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            AssetDatabase.CreateFolder(Path.GetDirectoryName(folder).Replace(Path.DirectorySeparatorChar, '/'), Path.GetFileName(folder));
        }
    }
}
