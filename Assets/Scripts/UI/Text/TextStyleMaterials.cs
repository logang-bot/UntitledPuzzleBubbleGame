using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// The shared TMP material for a style's outline layer (TextOutlineLayer),
    /// one per (font, style). The glyph is dilated out to the outline's outer
    /// edge and drawn in the outline color; the drop shadow is the underlay,
    /// which follows that dilated shape, offset down. Cached for the app's
    /// lifetime.
    /// </summary>
    public static class TextStyleMaterials
    {
        private static readonly Dictionary<(TMP_FontAsset, string), Material> Cache = new();

        public static Material ForOutline(TMP_FontAsset font, TextStyle style)
        {
            var key = (font, style.MaterialKey);
            if (!Cache.TryGetValue(key, out var material)) Cache[key] = material = Create(font, style);
            return material;
        }

        private static Material Create(TMP_FontAsset font, TextStyle style)
        {
            var material = new Material(font.material) { name = $"{font.name} {style.Id} outline" };
            var sdf = new SdfScale(font.faceInfo.pointSize / style.Size, material.GetFloat(ShaderUtilities.ID_GradientScale));
            material.SetFloat(ShaderUtilities.ID_FaceDilate, sdf.Dilate(style.Outline));
            if (style.HasShadow) ApplyShadow(material, style, sdf);
            return material;
        }

        private static void ApplyShadow(Material material, TextStyle style, SdfScale sdf)
        {
            material.EnableKeyword(ShaderUtilities.Keyword_Underlay);
            material.SetColor(ShaderUtilities.ID_UnderlayColor, style.ShadowColor);
            material.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, sdf.Offset(style.ShadowOffset.x));
            material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -sdf.Offset(style.ShadowOffset.y));
            material.SetFloat(ShaderUtilities.ID_UnderlayDilate, 0f);
            material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0f);
        }

        /// <summary>
        /// Converts pt at the style's size into TMP's SDF units. Atlas pixels
        /// per pt is samplingSize / textSize; the SDF spans the gradient scale
        /// in atlas pixels, and the shader moves the edge by dilate / 2 of that
        /// span (so a d-pixel dilation needs 2d / span), while an underlay
        /// offset of 1 moves it by one full span.
        /// </summary>
        private readonly struct SdfScale
        {
            private readonly float _atlasPixelsPerPt;
            private readonly float _gradientScale;

            public SdfScale(float atlasPixelsPerPt, float gradientScale)
            {
                _atlasPixelsPerPt = atlasPixelsPerPt;
                _gradientScale = gradientScale;
            }

            public float Dilate(float pt) => Mathf.Min(1f, 2f * pt * _atlasPixelsPerPt / _gradientScale);

            public float Offset(float pt) => Mathf.Clamp(pt * _atlasPixelsPerPt / _gradientScale, -1f, 1f);
        }
    }
}
