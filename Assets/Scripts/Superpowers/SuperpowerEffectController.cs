using System.Collections.Generic;
using System.Linq;
using Game.Grid;
using Game.Shooter;
using UnityEngine;

namespace Game.Superpowers
{
    public class SuperpowerEffectController : MonoBehaviour
    {
        [SerializeField] private GameBoard gameBoard;
        [SerializeField] private FiredBubbleController firedBubbleController;
        [SerializeField] private int bombRadius = 2;

        private void Start()
        {
            firedBubbleController.OnSuperpowerLanded += HandleSuperpowerLanded;
        }

        private void OnDestroy()
        {
            firedBubbleController.OnSuperpowerLanded -= HandleSuperpowerLanded;
        }

        private void HandleSuperpowerLanded(SuperpowerId id, (int Row, int Col) cell)
        {
            PopByColorGroup(ResolveAffectedCells(id, cell));
        }

        private HashSet<(int Row, int Col)> ResolveAffectedCells(SuperpowerId id, (int Row, int Col) cell)
        {
            return id switch
            {
                SuperpowerId.Bomb => SuperpowerEffectResolver.ResolveBomb(gameBoard.Grid, cell, bombRadius),
                SuperpowerId.RowClear => SuperpowerEffectResolver.ResolveRowClear(gameBoard.Grid, cell.Row),
                SuperpowerId.Rainbow => SuperpowerEffectResolver.ResolveRainbow(gameBoard.Grid, cell),
                _ => throw new System.ArgumentOutOfRangeException(nameof(id), id, null)
            };
        }

        private void PopByColorGroup(HashSet<(int Row, int Col)> cells)
        {
            foreach (var group in cells.GroupBy(c => gameBoard.Grid.GetColor(c.Row, c.Col)))
                gameBoard.PopCells(group.ToList(), group.Key);
        }
    }
}
