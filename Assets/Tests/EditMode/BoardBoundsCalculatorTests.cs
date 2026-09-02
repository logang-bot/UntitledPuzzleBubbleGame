using Game.Shooter;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class BoardBoundsCalculatorTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void Compute_GivenCameraPositionAndBoardWidth_ReturnsSymmetricWallBounds()
        {
            var bounds = BoardBoundsCalculator.Compute(cameraPosition: new Vector2(2f, 3f), boardWidth: 8f, orthographicSize: 5f, ceilingHeight: 0f);

            Assert.That(bounds.LeftWallX, Is.EqualTo(-2f).Within(Tolerance));
            Assert.That(bounds.RightWallX, Is.EqualTo(6f).Within(Tolerance));
        }

        [Test]
        public void Compute_GivenOrthographicSize_ReturnsCeilingAtCameraTop()
        {
            var bounds = BoardBoundsCalculator.Compute(cameraPosition: new Vector2(0f, 0f), boardWidth: 8f, orthographicSize: 5f, ceilingHeight: 0f);

            Assert.That(bounds.CeilingY, Is.EqualTo(5f).Within(Tolerance));
        }

        [Test]
        public void Compute_GivenCeilingHeight_LowersCeilingByThatMuch()
        {
            var bounds = BoardBoundsCalculator.Compute(cameraPosition: new Vector2(0f, 0f), boardWidth: 8f, orthographicSize: 5f, ceilingHeight: 1.2f);

            Assert.That(bounds.CeilingY, Is.EqualTo(3.8f).Within(Tolerance));
        }
    }
}
