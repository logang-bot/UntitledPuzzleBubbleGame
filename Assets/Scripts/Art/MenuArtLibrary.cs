using System.Collections.Generic;
using UnityEngine;

namespace Game.Art
{
    /// <summary>
    /// Single entry point the screens use to reach the menu art: sprite
    /// animations by legend id (pixel pets and cartoon UI), the TMP menu
    /// fonts and the fur-recolor material. See docs/features/art/overview.md.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Menu Art Library")]
    public class MenuArtLibrary : ScriptableObject
    {
        public MenuArtFonts Fonts => fonts;
        public MenuArtMaterials Materials => materials;
        public Material FurRecolorMaterial => materials.FurRecolor;

        [SerializeField] private SpriteSheetIndex[] sheets = new SpriteSheetIndex[0];
        [SerializeField] private MenuArtFonts fonts;
        [SerializeField] private MenuArtMaterials materials;

        private Dictionary<string, SpriteAnimEntry> _byId;

        public void Configure(SpriteSheetIndex[] newSheets, MenuArtFonts newFonts, MenuArtMaterials newMaterials)
        {
            sheets = newSheets;
            fonts = newFonts;
            materials = newMaterials;
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
