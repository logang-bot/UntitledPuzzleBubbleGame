using System.Collections.Generic;

namespace Game.Superpowers
{
    public class SuperpowerChargeTracker
    {
        public IEnumerable<SuperpowerId> UnlockedIds => _charges.Keys;

        private readonly Dictionary<SuperpowerId, int> _charges = new();

        public void ResetForLevel(IReadOnlyList<SuperpowerDefinition> definitions, int highestLevelReached)
        {
            _charges.Clear();
            foreach (var definition in definitions)
                if (highestLevelReached >= definition.UnlockLevel)
                    _charges[definition.Id] = definition.ChargesPerLevel;
        }

        public int Remaining(SuperpowerId id) => _charges.TryGetValue(id, out var count) ? count : 0;

        public bool TryConsume(SuperpowerId id)
        {
            if (Remaining(id) <= 0) return false;
            _charges[id]--;
            return true;
        }
    }
}
