using Game.Grid;
using UnityEngine;

namespace Game.Flow
{
    public class SoloRunRecorder : MonoBehaviour
    {
        [SerializeField] private GameBoard gameBoard;

        private void Start()
        {
            gameBoard.OnLevelLoaded += HandleLevelLoaded;
        }

        private void OnDestroy()
        {
            gameBoard.OnLevelLoaded -= HandleLevelLoaded;
        }

        private static void HandleLevelLoaded(int levelNumber) => SoloRunSave.CurrentLevel = levelNumber;
    }
}
