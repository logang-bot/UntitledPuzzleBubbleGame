using System;
using System.Linq;
using Game.UI;
using TMPro;
using UnityEngine;

namespace Game.Pets
{
    /// <summary>
    /// The pet card on the customization screen: the 336×400 pt panel with
    /// the stage (cat cushion or dog mat) and pixel portrait, the name field,
    /// fur swatches and shape picker. Rebuilt for whichever pet the current
    /// step edits. Coordinates are the guide's stage pt (layout.json custom_*).
    /// </summary>
    public class PetPanelView
    {
        public const int NameLimit = 12;

        public event Action<PetProfile> OnProfileChanged;
        public PetProfile Profile => _profile;

        private static readonly RectInt PanelRect = new RectInt(12, 186, 336, 400);
        private static readonly Vector2 StageFeet = new Vector2(180f, 370f);
        private static readonly RectInt NameRect = new RectInt(80, 396, 200, 48);
        private static readonly Vector2Int SwatchOrigin = new Vector2Int(92, 458);
        private static readonly Vector2Int ShapeOrigin = new Vector2Int(92, 514);

        private readonly MenuUiFactory _ui;
        private readonly PetAppearanceCatalog _catalog;
        private readonly RectTransform _stage;
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
            _stage = stage;
            ui.Sprite(stage, "panel", PanelRect);
        }

        public void Show(PetSpecies species, PetProfile profile)
        {
            _species = species;
            _profile = profile;
            if (_content != null) UnityEngine.Object.Destroy(_content.gameObject);
            _content = RuntimeUi.CreateRect(_stage, "PetContent", UiPlacement.Fill());
            BuildStage();
            BuildPickers();
            RefreshLook();
        }

        public void Celebrate() => _portrait.Celebrate();

        private void BuildStage()
        {
            _ui.PivotSprite(_content, _species == PetSpecies.Cat ? "stage_cushion" : "stage_mat", StageFeet);
            _portrait = new PortraitView(_ui, _content, (PetLook.From(_catalog, _species, _profile), StageFeet));
            var field = _ui.NameField(_content, new UiInputSpec { Name = "NameField", InitialText = _profile.Name, Placeholder = "TYPE A NAME", CharacterLimit = NameLimit, Rect = NameRect });
            field.textComponent.fontStyle = FontStyles.UpperCase;
            field.onValueChanged.AddListener(HandleNameChanged);
        }

        private void BuildPickers()
        {
            PetPanelLabels.Spawn(_ui, _content);
            var appearance = _catalog.For(_species);
            var speciesId = _species == PetSpecies.Cat ? "cat" : "dog";
            _swatches = new SwatchRowView(_ui, _content, new SwatchRowSpec { Origin = SwatchOrigin, FillIds = appearance.Colors.Select(c => $"swatch_{speciesId}_{c.Name.ToLowerInvariant()}").ToList() });
            _swatches.OnPicked += HandleColorPicked;
            _shapes = new ShapePickerView(_ui, _content, new ShapePickerSpec { Origin = ShapeOrigin, SilhouetteIds = appearance.Shapes.Select(s => $"shape_sil_{speciesId}_{s.ArtId}").ToList() });
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
