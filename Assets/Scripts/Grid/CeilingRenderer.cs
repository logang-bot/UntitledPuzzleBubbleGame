using UnityEngine;

namespace Game.Grid
{
    /// <summary>
    /// Draws a solid band across the reserved space at the top of the screen
    /// (GameBoard.CeilingHeight), so the ceiling/wall boundary reads clearly
    /// and row 0's bubbles visibly touch it. Grows by one row height on every
    /// ceiling-descent push (GameBoard.OnRowPushedDown): the wall doesn't
    /// just push existing bubbles down, it physically advances into that
    /// space, so the band's footprint must grow to match - otherwise the
    /// band stays pinned to its original size while the wall has silently
    /// advanced past it, leaving a plain-background gap that reads as
    /// placeable space even though nothing can land there anymore (see
    /// GridModel.RowsPushed / BubbleLandingResolver). Sits directly above the
    /// current effective row 0, rendered behind bubbles (sortingOrder -1) so
    /// nothing overlaps it. Reuses Unity's built-in Texture2D.whiteTexture
    /// (no art asset needed), tinted and stretched via the transform,
    /// matching CircleSpriteFactory's no-import approach for bubbles.
    /// </summary>
    public class CeilingRenderer : MonoBehaviour
    {
        [SerializeField] private GameBoard gameBoard;
        [SerializeField] private Color color = new(0.2f, 0.19f, 0.22f);

        private Transform _ceilingTransform;

        private void Start()
        {
            SpawnCeiling();
            UpdateCeiling();
            gameBoard.OnRowPushedDown += HandleBoardChanged;
            gameBoard.OnLevelLoaded += HandleBoardChanged;
        }

        private void OnDestroy()
        {
            gameBoard.OnRowPushedDown -= HandleBoardChanged;
            gameBoard.OnLevelLoaded -= HandleBoardChanged;
        }

        private void HandleBoardChanged(bool wasLastRowOccupied) => UpdateCeiling();
        private void HandleBoardChanged(int levelNumber) => UpdateCeiling();

        private void SpawnCeiling()
        {
            var ceiling = new GameObject("Ceiling");
            ceiling.transform.SetParent(gameBoard.transform);
            _ceilingTransform = ceiling.transform;

            var spriteRenderer = ceiling.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = WhiteSquareSprite();
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = -1; // behind bubbles
        }

        private void UpdateCeiling()
        {
            var cellWidth = gameBoard.CellWidth;
            var rowsPushedOffset = gameBoard.Grid.RowsPushed * HexGridMath.RowHeight(cellWidth);
            var height = gameBoard.CeilingHeight + rowsPushedOffset;
            var width = HexGridMath.BoardWidthWithOffsetMargin(gameBoard.Cols, cellWidth);
            var centerX = HexGridMath.BoardOriginXOffset(gameBoard.Cols, cellWidth);
            // The TOP edge is the screen's fixed physical boundary and never
            // moves (cellWidth*0.5 + CeilingHeight, same as before any
            // pushes). The BOTTOM edge is what advances: it must land exactly
            // on the current effective row 0's top edge
            // (-rowsPushedOffset + cellWidth*0.5), or the band would grow
            // upward off-screen instead of downward toward the bubbles.
            var centerY = cellWidth * 0.5f + gameBoard.CeilingHeight * 0.5f - rowsPushedOffset * 0.5f;

            _ceilingTransform.localPosition = new Vector3(centerX, centerY, 0f);
            _ceilingTransform.localScale = new Vector3(width, height, 1f);
        }

        private static Sprite WhiteSquareSprite()
        {
            var texture = Texture2D.whiteTexture;
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width);
        }
    }
}
