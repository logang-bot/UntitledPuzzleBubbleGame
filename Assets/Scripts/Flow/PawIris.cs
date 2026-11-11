using Game.Art;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Flow
{
    /// <summary>
    /// The paw-print iris, drawn with the stencil in three layers: the paw
    /// mask punches a paw-shaped hole (IrisHole), the dark transition fill
    /// covers everything outside it (IrisFill), and the same paw at 1.06× in
    /// gold shows as a rim around the hole (IrisRim). The paw's pivot sits on
    /// its main pad, at the stage center, so the hole closes on the screen
    /// center. Scale 0 means fully shut.
    /// </summary>
    public class PawIris
    {
        private const string PawSprite = "paw_mask";
        private const float RimScale = 1.06f;
        private static readonly Vector2 StageCenter = new Vector2(180f, 390f);
        private static readonly Color FillColor = new Color32(11, 8, 32, 255);

        private readonly Image _hole;
        private readonly Image _rim;

        public PawIris(MenuUiFactory ui, RectTransform canvas)
        {
            var materials = ui.Art.Materials;
            var stage = NativeStage.CreateStageAligned(canvas, "IrisStage");
            _hole = ui.PivotSprite(stage, PawSprite, StageCenter);
            _hole.material = materials.IrisHole;
            var fill = ui.Fill(canvas, "IrisFill", FillColor);
            fill.material = materials.IrisFill;
            fill.raycastTarget = false;
            _rim = ui.PivotSprite(NativeStage.CreateStageAligned(canvas, "RimStage"), PawSprite, StageCenter);
            _rim.material = materials.IrisRim;
            _rim.color = MenuColors.Gold;
        }

        public void SetScale(float scale)
        {
            _hole.enabled = scale > 0f;
            _rim.enabled = scale > 0f;
            _hole.rectTransform.localScale = Vector3.one * scale;
            _rim.rectTransform.localScale = Vector3.one * (scale * RimScale);
        }
    }
}
