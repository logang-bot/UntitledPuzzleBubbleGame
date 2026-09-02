using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Grid
{
    public static class BubbleColorPalette
    {
        private static readonly Dictionary<BubbleColor, Color> Colors = new Dictionary<BubbleColor, Color>
        {
            { BubbleColor.Red, Color.red },
            { BubbleColor.Orange, new Color(1f, 0.5f, 0f) },
            { BubbleColor.Yellow, Color.yellow },
            { BubbleColor.Green, Color.green },
            { BubbleColor.Blue, Color.blue },
            { BubbleColor.Purple, new Color(0.6f, 0.2f, 0.8f) },
        };

        public static readonly BubbleColor[] AllColors = (BubbleColor[])Enum.GetValues(typeof(BubbleColor));

        public static Color ToColor(BubbleColor color) => Colors[color];

        public static BubbleColor Random() => AllColors[UnityEngine.Random.Range(0, AllColors.Length)];

        public static BubbleColor Random(int colorCount) => AllColors[UnityEngine.Random.Range(0, colorCount)];
    }
}
