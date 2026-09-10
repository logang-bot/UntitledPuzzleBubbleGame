using System.Collections.Generic;
using UnityEngine;

namespace Game.Superpowers
{
    [CreateAssetMenu(menuName = "Game/Superpower Catalog")]
    public class SuperpowerCatalog : ScriptableObject
    {
        [SerializeField] private List<SuperpowerDefinition> definitions = new();

        public IReadOnlyList<SuperpowerDefinition> Definitions => definitions;
    }
}
