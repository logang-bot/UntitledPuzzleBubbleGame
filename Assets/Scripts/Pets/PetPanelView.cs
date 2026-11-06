using System;
using System.Linq;
using Game.UI;
using UnityEngine;

namespace Game.Pets
{
    /// <summary>
    /// The 254×226 pet card on the customization screen: stage and portrait,
    /// name field, fur swatches and shape picker. Rebuilt for whichever pet
    /// the current step edits. Coordinates are relative to the panel.
    /// </summary>
    public class PetPanelView
    {
        public const int NameLimit = 12;
        public static readonly RectInt PanelRect = new RectInt(8, 132, 254, 226);

        public event Action<PetProfile> OnProfileChanged;
        public PetProfile Profile => _profile;

        private static readonly RectInt PortraitRect = new RectInt(87, 22, 80, 80);
        private static readonly RectInt NameRect = new RectInt(57, 118, 140, 24);
        private static readonly Vector2Int SwatchOrigin = new Vector2Int(64, 154);
        private static readonly Vector2Int ShapeOrigin = new Vector2Int(64, 186);

        private readonly MenuUiFactory _ui;
        private readonly PetAppearanceCatalog _catalog;
        private readonly RectTransform _panel;
        private RectTransform _content;
        private PortraitView _portrait;
        private SwatchRowView _swatches;
        private ShapePickerView _shapes;
        private PetSpecies _species;
        private PetProfile _profile;

        public PetPanelView(MenuUiFactory ui, RectTransform stage, PetAppearanceCatalog catalog)
        {
            _ui = ui;
            _catalog = catalog;
            _panel = ui.Sprite(stage, "panel", PanelRect).rectTransform;
        }

        public void Show(PetSpecies species, PetProfile profile)
        {
            _species = species;
            _profile = profile;
            if (_content != null) UnityEngine.Object.Destroy(_content.gameObject);
            _content = RuntimeUi.CreateRect(_panel, "Content", UiPlacement.Fill());
            BuildStage();
            BuildPickers();
            RefreshLook();
        }

        public void Celebrate() => _portrait.Celebrate();

        private void BuildStage()
        {
            var isCat = _species == PetSpecies.Cat;
            _ui.Sprite(_content, isCat ? "stage_cushion" : "stage_mat", new RectInt(88, 91, 78, isCat ? 22 : 18));
            _portrait = new PortraitView(_ui, _content, PortraitRect);
            var field = _ui.NameField(_content, new UiInputSpec { Name = "NameField", InitialText = _profile.Name, Placeholder = "TYPE A NAME", CharacterLimit = NameLimit, Rect = NameRect });
            field.onValueChanged.AddListener(HandleNameChanged);
        }

        private void BuildPickers()
        {
            PetPanelLabels.Spawn(_ui, _content);
            var appearance = _catalog.For(_species);
            var speciesId = _species == PetSpecies.Cat ? "cat" : "dog";
            _swatches = new SwatchRowView(_ui, _content, new SwatchRowSpec { Origin = SwatchOrigin, FillIds = appearance.Colors.Select(c => $"swatch_{speciesId}_{c.Name.ToLowerInvariant()}").ToList() });
            _swatches.OnPicked += HandleColorPicked;
            _shapes = new ShapePickerView(_ui, _content, new ShapePickerSpec { Origin = ShapeOrigin, TileIds = appearance.Shapes.Select(s => $"shape_{speciesId}_{s.ArtId}").ToList() });
            _shapes.OnPicked += HandleShapePicked;
        }

        private void HandleNameChanged(string newName)
        {
            _profile.Name = newName.Trim();
            OnProfileChanged?.Invoke(_profile);
        }

        private void HandleColorPicked(int index)
        {
            _profile.ColorIndex = index;
            RefreshLook();
            Celebrate();
            OnProfileChanged?.Invoke(_profile);
        }

        private void HandleShapePicked(int index)
        {
            _profile.ShapeIndex = index;
            RefreshLook();
            Celebrate();
            OnProfileChanged?.Invoke(_profile);
        }

        private void RefreshLook()
        {
            var look = PetLook.From(_catalog, _species, _profile);
            _portrait.Show(look);
            _swatches.Select(_profile.ColorIndex);
            _shapes.Select(_profile.ShapeIndex);
        }
    }
}
