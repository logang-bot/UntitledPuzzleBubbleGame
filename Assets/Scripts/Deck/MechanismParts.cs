using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Deck
{
    /// <summary>
    /// The launcher mechanism's built pieces, in the guide's draw order, plus
    /// the empty layers the pets, controls, timer pips and bubbles fill in.
    /// </summary>
    public class MechanismParts
    {
        public MechanismKind Kind;
        public bool HasFirer;
        public DeckGeometry Geometry;
        public readonly List<(RectTransform Rect, DeckGear Gear)> Gears = new();
        public RectTransform Crank;
        public RectTransform CrankKnob;
        public RectTransform Wheel;
        public RectTransform LeverArm;
        public Image FireButton;
        public RectTransform PointerRing;
        public RectTransform PointerArrow;
        public RectTransform AimerLayer;
        public RectTransform FirerLayer;
        public RectTransform ControlsLayer;
        public RectTransform PipsLayer;
        public RectTransform BubblesLayer;
    }
}
