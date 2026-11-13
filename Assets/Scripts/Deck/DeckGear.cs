using UnityEngine;

namespace Game.Deck
{
    /// <summary>One back gear of the launcher mechanism: its sprite, center (left-hand pt), tooth count and turning sense.</summary>
    public readonly struct DeckGear
    {
        public readonly string SpriteId;
        public readonly Vector2 Center;
        public readonly int Teeth;
        public readonly int Turn;

        public DeckGear(string spriteId, Vector2 center, int teeth, int turn)
        {
            SpriteId = spriteId;
            Center = center;
            Teeth = teeth;
            Turn = turn;
        }
    }
}
