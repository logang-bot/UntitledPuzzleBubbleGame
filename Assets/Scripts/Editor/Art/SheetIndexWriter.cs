using System.Collections.Generic;
using System.Linq;
using Game.Art;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools.Art
{
    /// <summary>
    /// Creates or refreshes the SpriteSheetIndex asset for one sliced sheet,
    /// pairing each legend id with its frame sprites in order.
    /// </summary>
    public static class SheetIndexWriter
    {
        public static SpriteSheetIndex Write(string texturePath, SheetLegend legend, string indexFolder)
        {
            var sprites = AssetDatabase.LoadAllAssetsAtPath(texturePath).OfType<Sprite>().ToDictionary(s => s.name);
            var index = LoadOrCreate($"{indexFolder}/{legend.sheet}.asset");
            index.ReplaceEntries(legend.sprites.Select(sprite => EntryFor(sprite, sprites)).ToList());
            EditorUtility.SetDirty(index);
            return index;
        }

        private static SpriteAnimEntry EntryFor(SheetLegendSprite sprite, IReadOnlyDictionary<string, Sprite> sprites)
        {
            var frames = Enumerable.Range(0, sprite.frames)
                .Select(i => sprites.TryGetValue(SheetSpriteRects.FrameName(sprite.id, i), out var s) ? s : null)
                .ToArray();
            return new SpriteAnimEntry { Id = sprite.id, Frames = frames, Fps = sprite.fps };
        }

        private static SpriteSheetIndex LoadOrCreate(string assetPath)
        {
            var existing = AssetDatabase.LoadAssetAtPath<SpriteSheetIndex>(assetPath);
            if (existing != null) return existing;
            var created = ScriptableObject.CreateInstance<SpriteSheetIndex>();
            AssetDatabase.CreateAsset(created, assetPath);
            return created;
        }
    }
}
