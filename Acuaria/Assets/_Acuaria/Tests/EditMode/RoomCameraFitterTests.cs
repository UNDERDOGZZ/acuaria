using NUnit.Framework;

namespace Acuaria.Room.Tests
{
    public sealed class RoomCameraFitterTests
    {
        [TestCase(16f / 9f, 5.625f)]
        [TestCase(20f / 9f, 5.625f)]
        [TestCase(4f / 3f, 7.5f)]
        public void CalculateOrthographicSize_FitsReferenceArea(float aspect, float expected)
        {
            var result = RoomCameraFitter.CalculateOrthographicSize(20f, 11.25f, aspect);

            Assert.That(result, Is.EqualTo(expected).Within(0.001f));
            Assert.That(float.IsNaN(result), Is.False);
            Assert.That(result, Is.Positive);
        }

        [Test]
        public void CalculateOrthographicSize_InvalidInput_ReturnsSafePositiveValue()
        {
            var result = RoomCameraFitter.CalculateOrthographicSize(0f, 0f, 0f);

            Assert.That(result, Is.EqualTo(0.01f));
        }
    }
}
