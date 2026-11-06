using System;
using System.Collections.Generic;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Pets
{
    /// <summary>
    /// The shape picker: ‹ arrow, one silhouette tile per shape (27 px
    /// apart, frame 1 when selected), › arrow. Tiles pick directly; arrows
    /// step with wraparound.
    /// </summary>
    public class ShapePickerView
    {
        public event Action<int> OnPicked;

        private const int ArrowWidth = 16;
        private const int TileOffset = 20;
        private const int TilePitch = 27;
        private const int RightArrowOffset = 130;
        private static readonly Vector2Int TileSize = new Vector2Int(22, 26);

        private readonly MenuUiFactory _ui;
        private readonly List<Image> _tiles = new();
        private readonly IReadOnlyList<string> _tileIds;
        private int _selected;

        public ShapePickerView(MenuUiFactory ui, RectTransform parent, ShapePickerSpec spec)
        {
            _ui = ui;
            _tileIds = spec.TileIds;
            SpawnArrow(parent, ButtonKind.ArrowLeft, new Vector2Int(spec.Origin.x, spec.Origin.y));
            for (var i = 0; i < _tileIds.Count; i++)
                SpawnTile(parent, spec.Origin, i);
            SpawnArrow(parent, ButtonKind.ArrowRight, new Vector2Int(spec.Origin.x + RightArrowOffset, spec.Origin.y));
        }

        public void Select(int index)
        {
            _selected = index;
            for (var i = 0; i < _tiles.Count; i++)
                _tiles[i].sprite = _ui.Art.Frame(_tileIds[i], i == index ? 1 : 0);
        }

        private void SpawnArrow(RectTransform parent, ButtonKind kind, Vector2Int position)
        {
            var step = kind == ButtonKind.ArrowLeft ? -1 : 1;
            _ui.Button(parent, new UiSkinButtonSpec
            {
                Name = kind.ToString(),
                Kind = kind,
                Rect = new RectInt(position.x, position.y, ArrowWidth, TileSize.y),
                OnClick = () => OnPicked?.Invoke(CyclicIndex.Step(_selected, step, _tileIds.Count))
            });
        }

        private void SpawnTile(RectTransform parent, Vector2Int origin, int index)
        {
            var rect = new RectInt(origin.x + TileOffset + index * TilePitch, origin.y, TileSize.x, TileSize.y);
            var tile = _ui.Sprite(parent, _tileIds[index], rect);
            tile.raycastTarget = true;
            tile.gameObject.AddComponent<Button>().onClick.AddListener(() => OnPicked?.Invoke(index));
            _tiles.Add(tile);
        }
    }
}
