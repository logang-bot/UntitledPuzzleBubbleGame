using Game.Gameplay;
using Game.Grid;
using Game.Pets;
using Game.Shooter;

namespace Game.Deck
{
    /// <summary>
    /// What the launcher pets need: who they are (setup and saved looks) and
    /// what they react to. The game state is missing in Battle Mode, which
    /// has no ceiling warning or level result.
    /// </summary>
    public struct PetStageInputs
    {
        public DeckSetup Setup;
        public PetAppearanceCatalog Catalog;
        public ShooterController Shooter;
        public MechanismAnimator Mechanism;
        public IShotClock Clock;
        public GameBoard Board;
        public GameStateManager Game;
    }
}
