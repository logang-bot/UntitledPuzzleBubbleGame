using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Game.EditorTools.Art
{
    /// <summary>
    /// Turns a legend into Sprite Editor rects, one per frame, named
    /// "{id}_{frame}". Geometry (bottom-up flip, rounding, pivots) comes
    /// from LegendGeometry.
    /// </summary>
    public static class SheetSpriteRects
    {
        private struct LegendFrame
        {
            public SheetLegendSprite Sprite;
            public int Index;
            public float SheetHeight;
        }

        public static List<SpriteRect> Build(SheetLegend legend, IReadOnlyDictionary<string, GUID> existingIds)
        {
            var rects = new List<SpriteRect>();
            foreach (var sprite in legend.sprites)
                for (var frame = 0; frame < sprite.frames; frame++)
                    rects.Add(BuildFrame(new LegendFrame { Sprite = sprite, Index = frame, SheetHeight = legend.height }, existingIds));
            return rects;
        }

        public static string FrameName(string id, int frame) => $"{id}_{frame}";

        private static SpriteRect BuildFrame(LegendFrame frame, IReadOnlyDictionary<string, GUID> existingIds)
        {
            var name = FrameName(frame.Sprite.id, frame.Index);
            return new SpriteRect
            {
                name = name,
                spriteID = existingIds.TryGetValue(name, out var id) ? id : GUID.Generate(),
                rect = LegendGeometry.FrameRect(frame.Sprite, frame.Index, frame.SheetHeight),
                alignment = SpriteAlignment.Custom,
                pivot = LegendGeometry.NormalizedPivot(frame.Sprite),
                border = BorderOf(frame.Sprite.border)
            };
        }

        private static Vector4 BorderOf(SheetLegendBorder border)
        {
            return border == null ? Vector4.zero : new Vector4(border.l, border.b, border.r, border.t);
        }
    }
}
