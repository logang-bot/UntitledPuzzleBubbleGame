using System.Linq;
using Game.Art;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools.Art
{
    /// <summary>
    /// Builds MenuArtLibrary.asset from the sheet indices, the TMP menu fonts
    /// and the fur material. Sheets are listed pixel first, cartoon second, so
    /// a cartoon sprite wins when an old pixel sheet still has the same id;
    /// those overlaps are reported as warnings.
    /// </summary>
    public static class MenuArtLibraryWriter
    {
        private const string LibraryPath = "Assets/Art/Screens/MenuArtLibrary.asset";
        private const string ShaderFolder = "Assets/Art/Shaders";
        private const float IrisEdgeCutoff = 0.5f;

        public static void Write(SpriteSheetIndex[] sheets)
        {
            WarnAboutDuplicateIds(sheets);
            var library = LoadOrCreate(LibraryPath, ScriptableObject.CreateInstance<MenuArtLibrary>);
            library.Configure(sheets, FontAssetWriter.Write(), WriteMaterials());
            EditorUtility.SetDirty(library);
        }

        private static void WarnAboutDuplicateIds(SpriteSheetIndex[] sheets)
        {
            var duplicates = sheets.SelectMany(sheet => sheet.Entries).GroupBy(entry => entry.Id).Where(group => group.Count() > 1).Select(group => group.Key).ToArray();
            if (duplicates.Length > 0) Debug.LogWarning($"Menu sprite ids defined twice (the cartoon sheet wins): {string.Join(", ", duplicates)}");
        }

        private static T LoadOrCreate<T>(string path, System.Func<T> create) where T : Object
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            var created = create();
            AssetDatabase.CreateAsset(created, path);
            return created;
        }

        private static MenuArtMaterials WriteMaterials() => new MenuArtMaterials
        {
            FurRecolor = LoadOrCreateMaterial("UIFurRecolor", ("Game/UI/FurRecolor", 0f)),
            IrisHole = LoadOrCreateMaterial("UIIrisHole", ("Game/UI/StencilWrite", IrisEdgeCutoff)),
            IrisFill = LoadOrCreateMaterial("UIIrisFill", ("Game/UI/StencilOutside", 0f)),
            IrisRim = LoadOrCreateMaterial("UIIrisRim", ("Game/UI/StencilOutside", IrisEdgeCutoff))
        };

        private static Material LoadOrCreateMaterial(string name, (string Shader, float Cutoff) setup)
        {
            var material = LoadOrCreate($"{ShaderFolder}/{name}.mat", () => new Material(Shader.Find(setup.Shader)));
            if (material.HasProperty("_Cutoff")) material.SetFloat("_Cutoff", setup.Cutoff);
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
