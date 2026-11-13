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
    /// Existing sprite ids are kept so references survive a re-export. The
    /// legend is flipped against the PNG's real size, since some exports
    /// round their stated height differently.
    /// </summary>
    public static class SheetSlicer
    {
        public static void Slice(string texturePath, SheetLegend legend, SheetImportProfile profile)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
            MatchLegendToSource(importer, legend);
            ApplySettings(importer, profile);
            ApplySheetLayout(importer, legend);
            var provider = CreateProvider(importer);
            var rects = SheetSpriteRects.Build(legend, ExistingIds(provider));
            provider.SetSpriteRects(rects.ToArray());
            WriteNameFileIds(provider, rects);
            provider.Apply();
            importer.SaveAndReimport();
        }

        private static void MatchLegendToSource(TextureImporter importer, SheetLegend legend)
        {
            importer.GetSourceTextureWidthAndHeight(out var width, out var height);
            legend.width = width;
            legend.height = height;
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
        }

        private static void ApplySheetLayout(TextureImporter importer, SheetLegend legend)
        {
            importer.maxTextureSize = LegendGeometry.MaxTextureSize(legend);
            UseFullRectMeshes(importer);
        }

        private static void UseFullRectMeshes(TextureImporter importer)
        {
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
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
