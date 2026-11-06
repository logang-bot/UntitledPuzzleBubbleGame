using System;
using UnityEngine;

namespace Game.UI
{
    public struct UiSkinButtonSpec
    {
        public string Name;
        public ButtonKind Kind;
        public string Label;
        public RectInt Rect;
        public Action OnClick;
    }
}
