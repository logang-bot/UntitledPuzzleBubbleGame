using System.Collections.Generic;
using UnityEngine;

namespace Game.Art
{
    /// <summary>
    /// Single entry point the screens use to reach the menu art: sprite
    /// animations by legend id, the two pixel fonts and the fur-recolor
    /// material. See docs/features/art/overview.md.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Menu Art Library")]
    public class MenuArtLibrary : ScriptableObject
    {
        public Font CaptionFont => captionFont;
        public Font SmallFont => smallFont;
        public Material FurRecolorMaterial => furRecolorMaterial;

        [SerializeField] private SpriteSheetIndex[] sheets = new SpriteSheetIndex[0];
        [SerializeField] private Font captionFont;
        [SerializeField] private Font smallFont;
        [SerializeField] private Material furRecolorMaterial;

        private Dictionary<string, SpriteAnimEntry> _byId;

        public void Configure(SpriteSheetIndex[] newSheets, MenuArtFonts fonts, Material furMaterial)
        {
            sheets = newSheets;
            captionFont = fonts.Caption;
            smallFont = fonts.Small;
            furRecolorMaterial = furMaterial;
            _byId = null;
        }

        public SpriteAnimEntry Find(string id)
        {
            _byId ??= BuildLookup();
            if (_byId.TryGetValue(id, out var entry)) return entry;
            Debug.LogError($"MenuArtLibrary: no sprite '{id}'. Re-run Tools > Art > Import Menu Sheets.");
            return new SpriteAnimEntry { Id = id };
        }

        public Sprite Frame(string id, int frameIndex)
        {
            var frames = Find(id).Frames;
            return frames.Length == 0 ? null : frames[Mathf.Clamp(frameIndex, 0, frames.Length - 1)];
        }

        private Dictionary<string, SpriteAnimEntry> BuildLookup()
        {
            var lookup = new Dictionary<string, SpriteAnimEntry>();
            foreach (var sheet in sheets)
                foreach (var entry in sheet.Entries)
                    lookup[entry.Id] = entry;
            return lookup;
        }
    }
}
