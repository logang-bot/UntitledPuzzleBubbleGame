using System.Linq;
using Game.Art;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Game.EditorTools.Art
{
    /// <summary>
    /// Creates the TMP SDF font assets for the menu fonts (Lilita One and the
    /// three Baloo 2 weights) next to their TTFs, pre-filled with printable
    /// ASCII. They stay dynamic so typed pet names can add glyphs. The 20 px
    /// padding leaves room for the design's 3–3.5 pt outlines plus the
    /// outlined drop shadow drawn as the TMP underlay.
    /// </summary>
    public static class FontAssetWriter
    {
        private const string FontFolder = "Assets/Art/Fonts";
        private const int SamplingSize = 72;
        private const int Padding = 20;
        private const int AtlasWidth = 1024;
        private const int AtlasHeight = 2048;

        public static MenuArtFonts Write() => new MenuArtFonts
        {
            Display = LoadOrCreate("LilitaOne-Regular"),
            UiSemiBold = LoadOrCreate("Baloo2-SemiBold"),
            UiBold = LoadOrCreate("Baloo2-Bold"),
            UiExtraBold = LoadOrCreate("Baloo2-ExtraBold")
        };

        private static TMP_FontAsset LoadOrCreate(string fontName)
        {
            var path = $"{FontFolder}/{fontName} SDF.asset";
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            return existing != null ? existing : Create(fontName, path);
        }

        private static TMP_FontAsset Create(string fontName, string path)
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>($"{FontFolder}/{fontName}.ttf");
            var asset = TMP_FontAsset.CreateFontAsset(font, SamplingSize, Padding, GlyphRenderMode.SDFAA, AtlasWidth, AtlasHeight,AtlasPopulationMode.Dynamic, true);
            asset.name = fontName + " SDF";
            AssetDatabase.CreateAsset(asset, path);
            SaveSubAssets(asset);
            asset.TryAddCharacters(PrintableAscii());
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void SaveSubAssets(TMP_FontAsset asset)
        {
            asset.material.name = asset.name + " Material";
            asset.atlasTextures[0].name = asset.name + " Atlas";
            AssetDatabase.AddObjectToAsset(asset.material, asset);
            AssetDatabase.AddObjectToAsset(asset.atlasTextures[0], asset);
        }

        private static string PrintableAscii() => new string(Enumerable.Range(32, 95).Select(code => (char)code).ToArray());
    }
}
