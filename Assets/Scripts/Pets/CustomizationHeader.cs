using Game.UI;
using TMPro;
using UnityEngine;

namespace Game.Pets
{
    /// <summary>
    /// Title ("THE AIMER" / "THE FIRER"), step counter, and the role hint
    /// under the CAT/DOG toggle, at the guide's positions.
    /// </summary>
    public class CustomizationHeader
    {
        private static readonly RectInt TitleRect = new RectInt(70, 40, 220, 42);
        private static readonly RectInt StepRect = new RectInt(70, 82, 220, 18);
        private static readonly RectInt HintRect = new RectInt(40, 158, 280, 18);

        private readonly TMP_Text _title;
        private readonly TMP_Text _step;
        private readonly TMP_Text _hint;

        public CustomizationHeader(MenuUiFactory ui, RectTransform stage)
        {
            _title = ui.LabelIn(stage, TitleRect, ("Title", TextStyles.Title));
            _step = ui.LabelIn(stage, StepRect, ("Step", TextStyles.SmallLabel));
            _hint = ui.LabelIn(stage, HintRect, ("Hint", TextStyles.SmallLabel));
        }

        public void Show(CustomizationStep step, string aimerDescription)
        {
            var isAimer = step == CustomizationStep.Aimer;
            _title.text = isAimer ? "THE AIMER" : "THE FIRER";
            _step.text = isAimer ? "STEP 1 OF 2" : "STEP 2 OF 2";
            _hint.text = isAimer ? "THE OTHER PET FIRES" : $"{aimerDescription} AIMS";
        }
    }
}
