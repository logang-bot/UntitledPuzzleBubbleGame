using System;
using System.Linq;
using Game.Grid;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Pets
{
    /// <summary>
    /// Customization block for one pet: name field, fur-color and shape
    /// pickers, and a placeholder preview (a tinted circle until the art pass
    /// supplies recolorable pet sprites).
    /// </summary>
    public class PetEditorPanel
    {
        public event Action OnNameChanged;

        public PetProfile CurrentProfile => new PetProfile
        {
            Name = _nameField.text.Trim(),
            ColorIndex = _colorCycler.Index,
            ShapeIndex = _shapeCycler.Index
        };

        private const int NameCharacterLimit = 12;
        private const int HeaderFontSize = 36;
        private static readonly Vector2 PreviewSize = new Vector2(180f, 180f);

        private readonly PetSpeciesAppearance _appearance;
        private readonly Image _preview;
        private readonly InputField _nameField;
        private readonly OptionCycler _colorCycler;
        private readonly OptionCycler _shapeCycler;

        public PetEditorPanel(RectTransform parent, PetEditorPanelSpec spec)
        {
            _appearance = spec.Appearance;
            var root = RuntimeUi.CreateRect(parent, spec.Species + "Panel", spec.Placement);
            SpawnHeader(root, spec.Species);
            _preview = SpawnPreview(root);
            _nameField = SpawnNameField(root, spec.InitialProfile.Name);
            _colorCycler = SpawnColorCycler(root, spec.InitialProfile.ColorIndex);
            _shapeCycler = SpawnShapeCycler(root, spec.InitialProfile.ShapeIndex);
            WireEvents();
            RefreshPreview();
        }

        private static void SpawnHeader(RectTransform root, PetSpecies species)
        {
            var placement = UiPlacement.Stretch(new Vector2(0f, 0.82f), new Vector2(1f, 1f));
            var header = RuntimeUi.CreateLabel(root, "Header", placement);
            header.text = $"Your {species}";
            header.fontSize = HeaderFontSize;
            header.color = Color.white;
        }

        private static Image SpawnPreview(RectTransform root)
        {
            var preview = RuntimeUi.CreateImage(root, "Preview", UiPlacement.At(new Vector2(0.2f, 0.42f), PreviewSize));
            preview.sprite = CircleSpriteFactory.CreateWhiteCircle();
            return preview;
        }

        private static InputField SpawnNameField(RectTransform root, string initialName)
        {
            return RuntimeUi.CreateInputField(root, new UiInputSpec
            {
                Name = "NameField",
                InitialText = initialName,
                Placeholder = "Name your pet",
                CharacterLimit = NameCharacterLimit,
                Placement = RightColumnRow(0.58f, 0.78f)
            });
        }

        private static UiPlacement RightColumnRow(float bottom, float top)
        {
            return UiPlacement.Stretch(new Vector2(0.42f, bottom), new Vector2(0.98f, top));
        }

        private OptionCycler SpawnColorCycler(RectTransform root, int initialIndex)
        {
            return new OptionCycler(root, new OptionCyclerSpec
            {
                Name = "ColorCycler",
                Placement = RightColumnRow(0.32f, 0.5f),
                Labels = _appearance.Colors.Select(c => c.Name).ToList(),
                InitialIndex = initialIndex
            });
        }

        private OptionCycler SpawnShapeCycler(RectTransform root, int initialIndex)
        {
            return new OptionCycler(root, new OptionCyclerSpec
            {
                Name = "ShapeCycler",
                Placement = RightColumnRow(0.06f, 0.24f),
                Labels = _appearance.Shapes.Select(s => s.Name).ToList(),
                InitialIndex = initialIndex
            });
        }

        private void WireEvents()
        {
            _nameField.onValueChanged.AddListener(_ => OnNameChanged?.Invoke());
            _colorCycler.OnChanged += _ => RefreshPreview();
        }

        private void RefreshPreview()
        {
            var colors = _appearance.Colors;
            _preview.color = colors.Count > 0 ? colors[_colorCycler.Index].Color : Color.white;
        }
    }
}
