using UnityEngine;

namespace Game.Deck
{
    /// <summary>
    /// Maps the deck's left-hand, mechanism-relative pt (DeckLayout) onto a
    /// stage: shifts down to the mechanism's top and mirrors x for the right
    /// hand, where everything that turns also turns the other way.
    /// </summary>
    public readonly struct DeckGeometry
    {
        public const float StageWidth = 360f;

        public bool RightHanded { get; }
        public float MechanismTop { get; }
        public float TurnSign => RightHanded ? -1f : 1f;

        public DeckGeometry(bool rightHanded, float mechanismTop)
        {
            RightHanded = rightHanded;
            MechanismTop = mechanismTop;
        }

        public Vector2 Point(Vector2 leftHandPt)
        {
            return new Vector2(RightHanded ? StageWidth - leftHandPt.x : leftHandPt.x, MechanismTop + leftHandPt.y);
        }

        public Rect Rect(Rect leftHandPt)
        {
            var x = RightHanded ? StageWidth - leftHandPt.x - leftHandPt.width : leftHandPt.x;
            return new Rect(x, MechanismTop + leftHandPt.y, leftHandPt.width, leftHandPt.height);
        }

        /// <summary>The left edge of a pixel pet frame: art px at 5/3 pt, right-aligned to the mirrored spot for the right hand.</summary>
        public float PetLeft(float artX, float artWidth)
        {
            return RightHanded ? StageWidth - (artX + artWidth) * DeckLayout.PtPerArtPx : artX * DeckLayout.PtPerArtPx;
        }
    }
}
