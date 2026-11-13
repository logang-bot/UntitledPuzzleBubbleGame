using Game.Superpowers;
using UnityEngine;

namespace Game.BoardArt
{
    /// <summary>
    /// The armed superpower bubbles (gm_bubbles, gm_fx): the bomb with its
    /// fuse spark, the row-clear bubble, and the rainbow swirl under its
    /// gloss. Overlay offsets are in pt, y up, from the bubble's center.
    /// </summary>
    public static class SuperpowerArtIds
    {
        public static string Body(SuperpowerId id) => id switch
        {
            SuperpowerId.Bomb => "bubble_bomb",
            SuperpowerId.RowClear => "bubble_rowclear",
            _ => "rainbow_swirl"
        };

        public static string Overlay(SuperpowerId id) => id switch
        {
            SuperpowerId.Bomb => "bomb_spark",
            SuperpowerId.Rainbow => "rainbow_gloss",
            _ => null
        };

        public static Vector2 OverlayOffset(SuperpowerId id) => id == SuperpowerId.Bomb ? new Vector2(15f, 25f) : Vector2.zero;
    }
}
