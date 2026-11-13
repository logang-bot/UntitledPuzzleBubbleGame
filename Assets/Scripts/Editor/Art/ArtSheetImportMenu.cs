using System.IO;
using System.Linq;
using Game.Art;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools.Art
{
    /// <summary>
    /// Tools > Art > Import Art Sheets: slices every Claude Design sheet from
    /// its legend JSON (the menu pixel pets in Assets/Art/Screens, the cartoon UI
    /// in Assets/Art/Cartoon, the in-game cartoon art and pixel pets in
    /// Assets/Art/Game), writes the sheet indices, the TMP fonts and the
    /// MenuArtLibrary, and copies the colorways into the pet catalog. Safe to
    /// re-run after every re-export.
    /// </summary>
    public static class ArtSheetImportMenu
    {
        private const string ColorwaysPath = "Assets/Art/Screens/colorways.json";
        private const string GamePetIdPrefix = "game_";

        private static readonly SheetFolder[] Folders =
        {
            new SheetFolder(("Assets/Art/Screens/Sheets", "Assets/Art/Screens/Index"), SheetImportProfile.Pixel),
            new SheetFolder(("Assets/Art/Cartoon/Sheets", "Assets/Art/Cartoon/Index"), SheetImportProfile.Cartoon),
            new SheetFolder(("Assets/Art/Game/PixelSheets", "Assets/Art/Game/Index"), SheetImportProfile.Pixel, GamePetIdPrefix),
            new SheetFolder(("Assets/Art/Game/Sheets", "Assets/Art/Game/Index"), SheetImportProfile.Cartoon)
        };

        [MenuItem("Tools/Art/Import Art Sheets")]
        public static void ImportAll()
        {
            var sheets = Folders.SelectMany(ImportFolder).ToArray();
            MenuArtLibraryWriter.Write(sheets);
            PetCatalogArtWriter.Write(ReadJson<ColorwaysFile>(ColorwaysPath));
            AssetDatabase.SaveAssets();
            Debug.Log($"Imported {sheets.Length} art sheets, {sheets.Sum(s => s.Entries.Count)} sprite ids.");
        }

        private static SpriteSheetIndex[] ImportFolder(SheetFolder folder)
        {
            EnsureFolder(folder.Index);
            return Directory.GetFiles(folder.Sheets, "*.json").Select(path => ImportSheet(path, folder)).ToArray();
        }

        private static SpriteSheetIndex ImportSheet(string legendPath, SheetFolder folder)
        {
            var legend = ReadJson<SheetLegend>(legendPath);
            foreach (var sprite in legend.sprites) sprite.id = folder.IdPrefix + sprite.id;
            var texturePath = Path.ChangeExtension(legendPath, ".png").Replace(Path.DirectorySeparatorChar, '/');
            SheetSlicer.Slice(texturePath, legend, folder.Profile);
            return SheetIndexWriter.Write(texturePath, legend, folder.Index);
        }

        private static T ReadJson<T>(string path) => JsonUtility.FromJson<T>(File.ReadAllText(path));

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            AssetDatabase.CreateFolder(Path.GetDirectoryName(folder).Replace(Path.DirectorySeparatorChar, '/'), Path.GetFileName(folder));
        }

        /// <summary>
        /// One folder of legends and sheets, where its indices go, how it is
        /// imported, and a prefix for its sprite ids. The in-game pixel pets
        /// share strip names with the menu portraits (cat_round_idle), so
        /// theirs are imported as game_cat_round_idle.
        /// </summary>
        private readonly struct SheetFolder
        {
            public readonly string Sheets;
            public readonly string Index;
            public readonly SheetImportProfile Profile;
            public readonly string IdPrefix;

            public SheetFolder((string Sheets, string Index) paths, SheetImportProfile profile, string idPrefix = "")
            {
                Sheets = paths.Sheets;
                Index = paths.Index;
                Profile = profile;
                IdPrefix = idPrefix;
            }
        }
    }
}
