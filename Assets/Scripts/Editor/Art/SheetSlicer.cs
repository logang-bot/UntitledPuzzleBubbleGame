using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Game.EditorTools.Art
{
    /// <summary>
    /// Applies a sheet family's import settings (see SheetImportProfile; no
    /// mipmaps either way) and the legend's sprite rects to one sheet texture.
    /// Existing sprite ids are kept so references survive a re-export.
    /// </summary>
    public static class SheetSlicer
    {
        public static void Slice(string texturePath, SheetLegend legend, SheetImportProfile profile)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
            ApplySettings(importer, profile);
            var provider = CreateProvider(importer);
            var rects = SheetSpriteRects.Build(legend, ExistingIds(provider));
            provider.SetSpriteRects(rects.ToArray());
            WriteNameFileIds(provider, rects);
            provider.Apply();
            importer.SaveAndReimport();
        }

        private static void ApplySettings(TextureImporter importer, SheetImportProfile profile)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = profile.PixelsPerUnit;
            importer.filterMode = profile.Filter;
            importer.textureCompression = profile.Compression;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = 4096;
        }

        private static ISpriteEditorDataProvider CreateProvider(TextureImporter importer)
        {
            var factories = new SpriteDataProviderFactories();
            factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            return provider;
        }

        private static Dictionary<string, GUID> ExistingIds(ISpriteEditorDataProvider provider)
        {
            return provider.GetSpriteRects()
                .GroupBy(rect => rect.name)
                .ToDictionary(group => group.Key, group => group.First().spriteID);
        }

        private static void WriteNameFileIds(ISpriteEditorDataProvider provider, List<SpriteRect> rects)
        {
            var nameIds = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            nameIds?.SetNameFileIdPairs(rects.Select(rect => new SpriteNameFileIdPair(rect.name, rect.spriteID)));
        }
    }
}
