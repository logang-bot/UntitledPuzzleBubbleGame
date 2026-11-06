using Game.Art;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Flow
{
    /// <summary>
    /// Lays out the iris: the 318×633 paw frame at stage (−24, −24), plus
    /// Abyss bands beyond its edges for screens taller or wider than the
    /// art's bleed (shown only on the darkest frames).
    /// </summary>
    public static class TransitionIrisBuilder
    {
        private const float BandDepth = 2000f;
        private static readonly RectInt PawRect = new RectInt(-24, -24, 318, 633);

        public struct Parts
        {
            public Image Paw;
            public GameObject OuterFill;
        }

        public static Parts Build(RectTransform canvas)
        {
            var stage = NativeStage.CreateStageAligned(canvas, "TransitionStage");
            var paw = RuntimeUi.CreateImage(stage, "Paw", UiPlacement.TopLeft(PawRect));
            paw.raycastTarget = false;
            var outer = RuntimeUi.CreateRect(paw.rectTransform, "OuterFill", UiPlacement.Fill());
            SpawnBands(outer);
            return new Parts { Paw = paw, OuterFill = outer.gameObject };
        }

        private static void SpawnBands(RectTransform outer)
        {
            SpawnBand(outer, new Vector2(0.5f, 1f), new Vector2(0.5f, 0f));
            SpawnBand(outer, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f));
            SpawnBand(outer, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f));
            SpawnBand(outer, new Vector2(1f, 0.5f), new Vector2(0f, 0.5f));
        }

        private static void SpawnBand(RectTransform outer, Vector2 anchor, Vector2 pivot)
        {
            var band = RuntimeUi.CreateImage(outer, "Band", UiPlacement.At(anchor, new Vector2(BandDepth, BandDepth)));
            band.rectTransform.pivot = pivot;
            band.color = MenuPalette.Abyss;
            band.raycastTarget = false;
        }
    }
}
