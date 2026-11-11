using System.Collections.Generic;
using UnityEngine;

namespace Game.Pets
{
    public struct ShapePickerSpec
    {
        public Vector2Int Origin;
        public IReadOnlyList<string> SilhouetteIds;
    }
}
