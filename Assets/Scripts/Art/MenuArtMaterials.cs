using UnityEngine;

namespace Game.Art
{
    /// <summary>
    /// The menu's shared materials: the pixel pets' fur recolor, and the three
    /// stencil materials of the paw-print iris (the hole, the dark fill around
    /// it and the gold rim).
    /// </summary>
    [System.Serializable]
    public struct MenuArtMaterials
    {
        public Material FurRecolor;
        public Material IrisHole;
        public Material IrisFill;
        public Material IrisRim;
    }
}
