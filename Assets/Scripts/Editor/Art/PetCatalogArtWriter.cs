using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools.Art
{
    /// <summary>
    /// Copies the Claude Design colorway ramps and the sprite shape ids into
    /// DefaultPetAppearanceCatalog, matching colors by name and shapes by order.
    /// </summary>
    public static class PetCatalogArtWriter
    {
        private const string CatalogPath = "Assets/ScriptableObjects/DefaultPetAppearanceCatalog.asset";
        private static readonly string[] CatShapeIds = { "round", "fluffy", "slim", "chonk" };
        private static readonly string[] DogShapeIds = { "floppy", "pointy", "fluffy", "short" };

        public static void Write(ColorwaysFile colorways)
        {
            var catalog = new SerializedObject(AssetDatabase.LoadMainAssetAtPath(CatalogPath));
            WriteSpecies(catalog.FindProperty("cat"), colorways.cat, CatShapeIds);
            WriteSpecies(catalog.FindProperty("dog"), colorways.dog, DogShapeIds);
            catalog.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WriteSpecies(SerializedProperty species, Colorway[] colorways, string[] shapeIds)
        {
            var colors = species.FindPropertyRelative("Colors");
            for (var i = 0; i < colors.arraySize; i++)
                WriteRamp(colors.GetArrayElementAtIndex(i), colorways);
            var shapes = species.FindPropertyRelative("Shapes");
            for (var i = 0; i < shapes.arraySize && i < shapeIds.Length; i++)
                shapes.GetArrayElementAtIndex(i).FindPropertyRelative("ArtId").stringValue = shapeIds[i];
        }

        private static void WriteRamp(SerializedProperty color, Colorway[] colorways)
        {
            var name = color.FindPropertyRelative("Name").stringValue;
            var match = colorways.FirstOrDefault(c => c.name == name);
            if (match == null) { Debug.LogWarning($"No colorway named '{name}' in colorways.json"); return; }
            var ramp = color.FindPropertyRelative("Ramp");
            ramp.arraySize = match.ramp.Length;
            for (var shade = 0; shade < match.ramp.Length; shade++)
                ramp.GetArrayElementAtIndex(shade).colorValue = ParseHex(match.ramp[shade]);
        }

        private static Color ParseHex(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out var color) ? color : Color.magenta;
        }
    }
}
