using System;
using UnityEngine;

namespace Game.Art
{
    [Serializable]
    public class SpriteAnimEntry
    {
        public string Id;
        public Sprite[] Frames = Array.Empty<Sprite>();
        public float Fps;

        public Sprite First => Frames.Length > 0 ? Frames[0] : null;
    }
}
