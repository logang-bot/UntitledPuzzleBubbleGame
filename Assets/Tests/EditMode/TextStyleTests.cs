using Game.UI;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class TextStyleTests
    {
        [Test]
        public void CharacterSpacing_ConvertsPtToHundredthsOfAnEm()
        {
            Assert.AreEqual(1f / 22f * 100f, TextStyles.ButtonPrimary.CharacterSpacingEm100(), 1e-4f);
        }

        [Test]
        public void WithFill_ReplacesAGradientWithASolidColor()
        {
            var style = TextStyles.Title.WithFill(Color.red);
            Assert.IsFalse(style.HasGradient);
            Assert.AreEqual(Color.red, style.FillBottom);
        }

        [Test]
        public void TitleStyle_HasAnOutlinedDropShadow()
        {
            Assert.IsTrue(TextStyles.Title.HasShadow);
            Assert.AreEqual(3.5f, TextStyles.Title.Outline);
        }
    }
}
