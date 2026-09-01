using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    /// Listens to GameBoard.OnBubblePlaced so matching/dropping applies to any
    /// future placement source (superpowers, garbage rows), not just fired
    /// bubbles. GameBoard.Awake's initial random fill calls Grid.PlaceBubble
    /// directly (bypassing this event), so the debug board never auto-pops on
    /// load. See docs/features/core-gameplay/matching-and-popping.md.
    /// </summary>
    public class MatchProcessor : MonoBehaviour
    {
        [SerializeField] private GameBoard gameBoard;

        private void Start()
        {
            gameBoard.OnBubblePlaced += HandleBubblePlaced;
        }

        private void OnDestroy()
        {
            gameBoard.OnBubblePlaced -= HandleBubblePlaced;
        }

        private void HandleBubblePlaced(int row, int col)
        {
            var color = gameBoard.Grid.GetColor(row, col);
            var group = MatchResolver.FindMatchGroup(gameBoard.Grid, (row, col));
            if (group.Count == 0) return;
            gameBoard.PopCells(group, color);
            DropFloatingCells();
        }

        private void DropFloatingCells()
        {
            var floaters = MatchResolver.FindFloatingCells(gameBoard.Grid);
            if (floaters.Count > 0) gameBoard.DropCells(floaters);
        }
    }
}
