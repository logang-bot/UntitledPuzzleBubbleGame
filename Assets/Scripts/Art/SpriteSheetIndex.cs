using System.Collections.Generic;
using UnityEngine;

namespace Game.Art
{
    /// <summary>
    /// One exported Claude Design sheet: every sprite id with its ordered
    /// frames and fps. Written by Tools > Art > Import Art Sheets.
    /// </summary>
    public class SpriteSheetIndex : ScriptableObject
    {
        public IReadOnlyList<SpriteAnimEntry> Entries => entries;

        [SerializeField] private List<SpriteAnimEntry> entries = new();

        public void ReplaceEntries(List<SpriteAnimEntry> newEntries)
        {
            entries = newEntries;
        }
    }
}
