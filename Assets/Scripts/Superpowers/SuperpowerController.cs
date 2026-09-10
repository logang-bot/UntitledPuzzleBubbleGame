using System;
using System.Collections.Generic;
using Game.Gameplay;
using Game.Grid;
using Game.Shooter;
using UnityEngine;

namespace Game.Superpowers
{
    public class SuperpowerController : MonoBehaviour
    {
        [SerializeField] private SuperpowerCatalog catalog;
        [SerializeField] private GameBoard gameBoard;
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private FiredBubbleController firedBubbleController;
        [SerializeField] private float freezeDurationSeconds = 5f;

        public event Action OnAbilitiesChanged;

        public IEnumerable<SuperpowerId> UnlockedAbilities => _tracker.UnlockedIds;

        private readonly SuperpowerChargeTracker _tracker = new();

        private void Start()
        {
            gameBoard.OnLevelLoaded += HandleLevelLoaded;
            gameStateManager.OnLevelWon += HandleLevelWon;
            HandleLevelLoaded(gameBoard.LevelNumber);
        }

        private void OnDestroy()
        {
            gameBoard.OnLevelLoaded -= HandleLevelLoaded;
            gameStateManager.OnLevelWon -= HandleLevelWon;
        }

        public int RemainingCharges(SuperpowerId id) => _tracker.Remaining(id);

        public bool TryActivate(SuperpowerId id)
        {
            if (id != SuperpowerId.Freeze && firedBubbleController.HasArmedOrInFlightSuperpower) return false;
            if (!_tracker.TryConsume(id)) return false;
            ApplyActivation(id);
            return true;
        }

        private void ApplyActivation(SuperpowerId id)
        {
            switch (id)
            {
                case SuperpowerId.Freeze:
                    gameStateManager.Freeze(freezeDurationSeconds);
                    break;
                case SuperpowerId.Bomb:
                case SuperpowerId.RowClear:
                case SuperpowerId.Rainbow:
                    firedBubbleController.ArmSuperpower(id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(id), id, null);
            }
        }

        private void HandleLevelLoaded(int levelNumber)
        {
            _tracker.ResetForLevel(catalog.Definitions, SuperpowerProgress.HighestLevelReached);
            OnAbilitiesChanged?.Invoke();
        }

        private void HandleLevelWon()
        {
            if (gameBoard.LevelNumber > SuperpowerProgress.HighestLevelReached)
                SuperpowerProgress.HighestLevelReached = gameBoard.LevelNumber;
        }
    }
}
