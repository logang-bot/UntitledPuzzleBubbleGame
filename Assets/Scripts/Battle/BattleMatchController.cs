using System;
using UnityEngine;

namespace Game.Battle
{
    /// <summary>
    /// Reconciles both sides' outcomes into one authoritative result rather
    /// than letting each side unilaterally declare a winner — needed since,
    /// unlike solo mode, two boards can both end in the same frame (see the
    /// spec's "simultaneous end" rule: a draw). DefaultExecutionOrder
    /// ensures StartMatch's real LoadLevel call runs after every other
    /// battle component has subscribed to OnLevelLoaded. See
    /// docs/features/battle-mode/specs/2026-09-15-simple-attack-battle-mode-design.md.
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public class BattleMatchController : MonoBehaviour
    {
        [SerializeField] private BattlePlayerSide player1;
        [SerializeField] private BattlePlayerSide player2;
        [SerializeField] private int minLevelNumber = 1;
        [SerializeField] private int maxLevelNumber = 5;

        public event Action<BattleMatchResult> OnMatchEnded;

        private BattleEndReason? _player1EndedThisFrame;
        private BattleEndReason? _player2EndedThisFrame;
        private bool _matchEnded;

        private void Start()
        {
            player1.Outcome.OnSideEnded += HandlePlayer1Ended;
            player2.Outcome.OnSideEnded += HandlePlayer2Ended;
            StartMatch();
        }

        private void OnDestroy()
        {
            player1.Outcome.OnSideEnded -= HandlePlayer1Ended;
            player2.Outcome.OnSideEnded -= HandlePlayer2Ended;
        }

        public void Rematch()
        {
            _matchEnded = false;
            ResetSideState(player1);
            ResetSideState(player2);
            EnableSide(player1);
            EnableSide(player2);
            StartMatch();
        }

        private void LateUpdate()
        {
            if (_matchEnded) return;
            if (_player1EndedThisFrame == null && _player2EndedThisFrame == null) return;
            Settle();
        }

        private void StartMatch()
        {
            _player1EndedThisFrame = null;
            _player2EndedThisFrame = null;
            var levelNumber = UnityEngine.Random.Range(minLevelNumber, maxLevelNumber + 1);
            player1.GameBoard.LoadLevel(levelNumber);
            player2.GameBoard.LoadLevel(levelNumber);
        }

        private void HandlePlayer1Ended(BattleEndReason reason) => _player1EndedThisFrame = reason;
        private void HandlePlayer2Ended(BattleEndReason reason) => _player2EndedThisFrame = reason;

        private void Settle()
        {
            _matchEnded = true;
            DisableSide(player1);
            DisableSide(player2);
            OnMatchEnded?.Invoke(ResolveResult());
        }

        private BattleMatchResult ResolveResult()
        {
            if (_player1EndedThisFrame.HasValue && _player2EndedThisFrame.HasValue) return BattleMatchResult.Draw;
            if (_player1EndedThisFrame.HasValue) return ResultFor(_player1EndedThisFrame.Value, BattleMatchResult.Player1Wins, BattleMatchResult.Player2Wins);
            return ResultFor(_player2EndedThisFrame.Value, BattleMatchResult.Player2Wins, BattleMatchResult.Player1Wins);
        }

        private static BattleMatchResult ResultFor(BattleEndReason reason, BattleMatchResult ifCleared, BattleMatchResult ifWallReached) =>
            reason == BattleEndReason.Cleared ? ifCleared : ifWallReached;

        private static void DisableSide(BattlePlayerSide side)
        {
            side.ShooterController.enabled = false;
            side.ShotClock.enabled = false;
            side.AttackController.enabled = false;
        }

        private static void EnableSide(BattlePlayerSide side)
        {
            side.ShooterController.enabled = true;
            side.ShotClock.enabled = true;
            side.AttackController.enabled = true;
        }

        private static void ResetSideState(BattlePlayerSide side)
        {
            side.Outcome.ResetForRematch();
            side.AttackController.ResetForRematch();
            side.ShotClock.ResetForRematch();
        }
    }
}
