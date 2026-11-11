using Game.UI;
using UnityEngine;

namespace Game.Results
{
    /// <summary>
    /// A result banner or plate: its sprite at <see cref="Rect"/> (pt), and
    /// its caption at <see cref="TextRect"/>, relative to the banner.
    /// </summary>
    public struct BannerSpec
    {
        public string SpriteId;
        public string Text;
        public TextStyle Style;
        public RectInt Rect;
        public RectInt TextRect;
    }
}
