using System.Collections.Generic;
using Game.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Deck
{
    /// <summary>The 12 shot timer pips around the pointer, clockwise from the top, tinted each frame from the shot clock.</summary>
    public class TimerPipsView : MonoBehaviour
    {
        private readonly List<Image> _pips = new();
        private IShotClock _clock;

        public static void Create(DeckUi ui, MechanismParts parts, IShotClock clock)
        {
            var view = parts.PipsLayer.gameObject.AddComponent<TimerPipsView>();
            view._clock = clock;
            var center = parts.Geometry.Point(DeckLayout.Pointer);
            for (var i = 0; i < ShotClockDisplay.PipCount; i++)
                view._pips.Add(ui.At(parts.PipsLayer, "timer_pip", center + PipOffset(i)));
        }

        private static Vector2 PipOffset(int index)
        {
            var radians = (-90f + index * 360f / ShotClockDisplay.PipCount) * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * DeckLayout.PipRadius;
        }

        private void Update()
        {
            var remaining = _clock.ShotTimeRemaining;
            for (var i = 0; i < _pips.Count; i++)
                _pips[i].color = ShotClockDisplay.PipColor(i, remaining, Time.time);
        }
    }
}
