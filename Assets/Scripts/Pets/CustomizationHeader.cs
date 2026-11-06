using Game.Art;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Pets
{
    /// <summary>
    /// Title ("THE AIMER" / "THE FIRER"), step counter and the role hint
    /// under the CAT/DOG toggle.
    /// </summary>
    public class CustomizationHeader
    {
        private const int TitleY = 30;
        private const int StepY = 53;
        private const int HintY = 110;

        private readonly Text _title;
        private readonly Text _step;
        private readonly Text _hint;

        public CustomizationHeader(MenuUiFactory ui, RectTransform stage)
        {
            var muted = ui.Styles.Small.WithColor(MenuPalette.Stone);
            _title = ui.CenteredLabel(stage, "", new LabelLine(TitleY, ui.Styles.Title));
            _step = ui.CenteredLabel(stage, "", new LabelLine(StepY, muted));
            _hint = ui.CenteredLabel(stage, "", new LabelLine(HintY, muted));
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
