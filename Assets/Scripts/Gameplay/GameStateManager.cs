using System;
using System.Collections.Generic;
using Game.Grid;
using Game.Shooter;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>
    /// Owns the shot timer and the ceiling descent timer — two independent
    /// countdowns. The shot timer auto-fires at the current aim angle on
    /// expiry and resets only in response to ShooterController.OnFireRequested
    /// so manual and auto fire share one reset path. The ceiling timer pushes
    /// the board down one row on a fixed interval and resets itself. Also the
    /// "referee" for win/loss: clearing the board wins, the ceiling reaching
    /// the shooter line loses. See
    /// docs/features/core-gameplay/shot-timer-and-ceiling-descent.md and
    /// docs/features/core-gameplay/win-loss-conditions.md.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        private enum CeilingState { Countdown, Warning }

        [SerializeField] private ShooterController shooterController;
        [SerializeField] private GameBoard gameBoard;
        [SerializeField] private ScoreTracker scoreTracker;
        [SerializeField] private ShotsFiredCounter shotsFiredCounter;
        [SerializeField] private CameraShake cameraShake;
        [SerializeField] private float shotTimeSeconds = 8f;

        public event Action OnLevelWon;
        public event Action OnLevelLost;

        public float ShotTimeRemaining => _shotTimer.TimeRemaining;

        private ShotTimer _shotTimer;
        private ShotTimer _ceilingTimer;
        private CeilingState _ceilingState = CeilingState.Countdown;
        private bool _landingOccurredDuringWarning;
        private bool _isGameOver;

        private void Awake()
        {
            _shotTimer = new ShotTimer(shotTimeSeconds);
        }

        private void Start()
        {
            _ceilingTimer = new ShotTimer(gameBoard.CurrentDifficulty.CeilingDropIntervalSeconds);
            shooterController.OnFireRequested += HandleFireRequested;
            gameBoard.OnRowPushedDown += HandleRowPushedDown;
            gameBoard.OnBubblesPopped += HandleBoardCellsCleared;
            gameBoard.OnClusterDropped += HandleBoardCellsCleared;
            gameBoard.OnBubblePlaced += HandleBubblePlaced;
        }

        private void OnDestroy()
        {
            shooterController.OnFireRequested -= HandleFireRequested;
            gameBoard.OnRowPushedDown -= HandleRowPushedDown;
            gameBoard.OnBubblesPopped -= HandleBoardCellsCleared;
            gameBoard.OnClusterDropped -= HandleBoardCellsCleared;
            gameBoard.OnBubblePlaced -= HandleBubblePlaced;
        }

        private void Update()
        {
            if (_isGameOver) return;

            if (_shotTimer.Tick(Time.deltaTime)) shooterController.Fire();

            TickCeiling();
        }

        private void TickCeiling()
        {
            if (_ceilingState == CeilingState.Countdown) TickCeilingCountdown();
            else TickCeilingWarning();
        }

        private void TickCeilingCountdown()
        {
            if (!_ceilingTimer.Tick(Time.deltaTime)) return;
            _ceilingState = CeilingState.Warning;
            cameraShake.StartShaking();
        }

        // The board only advances once the player has landed a bubble after the
        // warning starts, not the instant the timer expires - HandleBubblePlaced
        // just flags that a landing happened; the actual push runs from here, one
        // frame later, so it never races MatchProcessor's own OnBubblePlaced handling.
        private void TickCeilingWarning()
        {
            if (!_landingOccurredDuringWarning) return;
            _landingOccurredDuringWarning = false;
            cameraShake.StopShaking();
            gameBoard.PushRowDown();
            _ceilingTimer.Reset();
            _ceilingState = CeilingState.Countdown;
        }

        private void StopCeilingWarning()
        {
            if (_ceilingState != CeilingState.Warning) return;
            cameraShake.StopShaking();
            _ceilingState = CeilingState.Countdown;
            _landingOccurredDuringWarning = false;
        }

        private void HandleFireRequested(Vector2 origin, float angleDegrees)
        {
            _shotTimer.Reset();
        }

        private void HandleBubblePlaced(int row, int col)
        {
            if (_ceilingState == CeilingState.Warning) _landingOccurredDuringWarning = true;
        }

        private void HandleRowPushedDown(bool wasLastRowOccupied)
        {
            if (wasLastRowOccupied) EndGame(OnLevelLost, "Ceiling reached the shooter line.");
        }

        private void HandleBoardCellsCleared(IReadOnlyCollection<(int Row, int Col)> cells, BubbleColor color)
        {
            HandleBoardCellsCleared(cells);
        }

        private void HandleBoardCellsCleared(IReadOnlyCollection<(int Row, int Col)> cells)
        {
            if (gameBoard.Grid.IsEmpty) EndGame(OnLevelWon, "Board cleared.");
        }

        private void EndGame(Action raiseEvent, string logMessage)
        {
            if (_isGameOver) return;
            _isGameOver = true;
            shooterController.enabled = false;
            StopCeilingWarning();
            Debug.Log(logMessage);
            raiseEvent?.Invoke();
        }

        public void RetryLevel()
        {
            scoreTracker.ResetScore();
            shotsFiredCounter.ResetCount();
            ResumeWithLevel(gameBoard.LevelNumber);
        }

        public void AdvanceToNextLevel()
        {
            ResumeWithLevel(gameBoard.LevelNumber + 1);
        }

        private void ResumeWithLevel(int levelNumber)
        {
            gameBoard.LoadLevel(levelNumber);
            _shotTimer.Reset();
            StopCeilingWarning();
            _ceilingTimer = new ShotTimer(gameBoard.CurrentDifficulty.CeilingDropIntervalSeconds);
            shooterController.enabled = true;
            _isGameOver = false;
        }
    }
}
