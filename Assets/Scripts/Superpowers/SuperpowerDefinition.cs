using System;

namespace Game.Superpowers
{
    [Serializable]
    public class SuperpowerDefinition
    {
        public SuperpowerId Id;
        public int UnlockLevel;
        public int ChargesPerLevel;
    }
}
