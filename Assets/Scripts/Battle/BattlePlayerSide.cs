using System;
using Game.Grid;
using Game.Shooter;
using UnityEngine;

namespace Game.Battle
{
    [Serializable]
    public sealed class BattlePlayerSide
    {
        public GameBoard GameBoard;
        public ShooterController ShooterController;
        public BattleShotClock ShotClock;
        public BattleAttackController AttackController;
        public BattleSideOutcome Outcome;
    }
}
