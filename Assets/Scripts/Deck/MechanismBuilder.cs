using Game.UI;
using UnityEngine;

namespace Game.Deck
{
    /// <summary>
    /// Builds the launcher mechanism on a deck stage in the guide's draw
    /// order: pipes, back gears, crank or wheel with the aimer, the pointer,
    /// the next ring, the lever or button with the firer, then the layers for
    /// the controls, timer pips and bubbles, and the NEXT sign on top.
    /// </summary>
    public static class MechanismBuilder
    {
        public static MechanismParts Build(DeckUi ui, RectTransform stage, MechanismParts parts)
        {
            var pipes = ui.Sprite(stage, "mech_pipes", parts.Geometry.Rect(DeckLayout.Pipes));
            if (parts.Geometry.RightHanded) DeckUi.FlipAroundCenter(pipes.rectTransform);
            foreach (var gear in DeckLayout.Gears) parts.Gears.Add((At(ui, stage, (gear.SpriteId, gear.Center), parts), gear));
            AddAimerSide(ui, stage, parts);
            parts.PointerRing = At(ui, stage, ("pointer_ring", DeckLayout.Pointer), parts);
            parts.PointerArrow = At(ui, stage, ("pointer_arrow", DeckLayout.Pointer), parts);
            At(ui, stage, ("next_ring", DeckLayout.NextRing), parts);
            AddFirerSide(ui, stage, parts);
            AddTopLayers(ui, stage, parts);
            return parts;
        }

        private static void AddAimerSide(DeckUi ui, RectTransform stage, MechanismParts parts)
        {
            if (parts.Kind == MechanismKind.Crank) parts.Crank = At(ui, stage, ("crank_gear", DeckLayout.Crank), parts);
            else parts.Wheel = At(ui, stage, ("steer_wheel", DeckLayout.Wheel), parts);
            parts.AimerLayer = ui.Group(stage, "Aimer");
            if (parts.Kind == MechanismKind.Crank) parts.CrankKnob = At(ui, stage, ("crank_knob", DeckLayout.Crank), parts);
        }

        private static void AddFirerSide(DeckUi ui, RectTransform stage, MechanismParts parts)
        {
            if (parts.HasFirer && parts.Kind == MechanismKind.Crank) AddLever(ui, stage, parts);
            parts.FirerLayer = ui.Group(stage, "Firer");
            if (parts.HasFirer && parts.Kind == MechanismKind.Wheel) parts.FireButton = ui.Sprite(stage, "fire_button_up", parts.Geometry.Rect(DeckLayout.FireButton));
        }

        private static void AddLever(DeckUi ui, RectTransform stage, MechanismParts parts)
        {
            parts.LeverArm = At(ui, stage, ("fire_lever_arm", DeckLayout.LeverPivot), parts);
            At(ui, stage, ("fire_lever_base", DeckLayout.LeverPivot), parts);
        }

        private static void AddTopLayers(DeckUi ui, RectTransform stage, MechanismParts parts)
        {
            parts.ControlsLayer = ui.Group(stage, "Controls");
            parts.PipsLayer = ui.Group(stage, "TimerPips");
            parts.BubblesLayer = ui.Group(stage, "Bubbles");
            ui.Sprite(stage, "next_sign", parts.Geometry.Rect(DeckLayout.NextSign));
            ui.Label(stage, parts.Geometry.Rect(DeckLayout.NextLabel), ("NEXT", GameTextStyles.SignLabel));
        }

        private static RectTransform At(DeckUi ui, RectTransform stage, (string SpriteId, Vector2 LeftHandPt) part, MechanismParts parts)
        {
            return ui.At(stage, part.SpriteId, parts.Geometry.Point(part.LeftHandPt)).rectTransform;
        }
    }
}
